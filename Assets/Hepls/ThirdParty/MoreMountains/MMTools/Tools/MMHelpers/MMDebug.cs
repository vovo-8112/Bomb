using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
	public static class MMDebug 
	{
        #region Commands
        private static MethodInfo[] _commands;
        private static readonly int _logHistoryMaxLength = 256;

#if UNITY_EDITOR
        private static bool _debugDrawEnabledSet = false;
#endif
        private static bool _debugDrawEnabled = false;
        private static bool _debugLogEnabled = false;
        private static bool _debugLogEnabledSet = false;
        public static MethodInfo[] Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = AppDomain.CurrentDomain.GetAssemblies()
                      .SelectMany(
                                m => m.GetTypes().SelectMany(
                                    n => n.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                                        .Where(o => o.GetCustomAttribute<MMDebugLogCommandAttribute>() != null))).ToArray();
                }

                return _commands;
            }
        }
        public static void DebugLogCommand(string command)
        {
            if (command == string.Empty || command == null)
            {
                LogCommand("", "#ff2a00");
                return; 
            }
            string[] splitCommand = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitCommand == null || splitCommand.Length == 0)
            {
                LogCommand("Empty command", "#ff2a00");
                return;
            }
            string commandFirst = MMString.UppercaseFirst(splitCommand[0]);
            MethodInfo[] methods = Commands.Where(m => m.Name == commandFirst).ToArray();
            if (methods.Length == 0)
            {
                LogCommand("Command " + commandFirst + " not found.", "#ff2a00");
                return;
            }

            MethodInfo commandInfo;
            object[] parameters = null;

            if (splitCommand.Length > 1)
            {
                commandInfo = methods.Where(m => m.GetParameters().Length > 0).FirstOrDefault();

                if (commandInfo == null)
                {
                    LogCommand("A version of command " + commandFirst + " with arguments could not be found. Maybe try without arguments.", "#ff2a00");
                    return;
                }

                MMDebugLogCommandArgumentCountAttribute argumentAttribute = commandInfo.GetCustomAttributes<MMDebugLogCommandArgumentCountAttribute>(true).FirstOrDefault();
                if (argumentAttribute != null && argumentAttribute.ArgumentCount > splitCommand.Length - 1)
                { 
                    LogCommand("A version of command " + commandFirst + " needs at least " + argumentAttribute.ArgumentCount + " arguments.", "#ff2a00");
                    return;
                }

                parameters = new object[] { splitCommand };
            }
            else
            {
                commandInfo = methods.Where(m => m.GetParameters().Length == 0).FirstOrDefault();

                if (commandInfo == null)
                {
                    LogCommand("A version of command " + commandFirst + " without arguments could not be found.", "#ff2a00");
                    return;
                }
            }

            LogCommand(command, "#FFC400");
            methods[0].Invoke(null, parameters);
        }
        private static void LogCommand(string command, string color)
        {
            DebugLogItem item = new DebugLogItem(command, color, Time.frameCount, Time.time, 3, true);
            LogHistory.Add(item);
            MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
        }

        #endregion

        #region DebugLog
        public struct DebugLogItem
        {
            public object Message;
            public string Color;
            public int Framecount;
            public float Time;
            public int TimePrecision;
            public bool DisplayFrameCount;

            public DebugLogItem(object message, string color, int framecount, float time, int timePrecision, bool displayFrameCount)
            {
                Message = message;
                Color = color;
                Framecount = framecount;
                Time = time;
                TimePrecision = timePrecision;
                DisplayFrameCount = displayFrameCount;
            }
        }
        public static List<DebugLogItem> LogHistory = new List<DebugLogItem>(_logHistoryMaxLength);
        public static string LogHistoryText
        {
            get
            {
                string colorPrefix = "";
                string colorSuffix = "";

                StringBuilder log = new StringBuilder();
                for (int i = 0; i < LogHistory.Count; i++)
                {
                    if (!string.IsNullOrEmpty(LogHistory[i].Color))
                    {
                        colorPrefix = "<color=" + LogHistory[i].Color + ">";
                        colorSuffix = "</color>";
                    }
                    if (LogHistory[i].DisplayFrameCount)
                    {
                        log.Append("<color=#82d3f9>[" + LogHistory[i].Framecount + "]</color> ");
                    }
                    log.Append("<color=#f9a682>[" + MMTime.FloatToTimeString(LogHistory[i].Time, false, true, true, true) + "]</color> ");
                    log.Append(colorPrefix + LogHistory[i].Message + colorSuffix);
                    log.Append(Environment.NewLine);
                }
                return log.ToString();
            }
        }
        public static void DebugLogClear()
        {
            LogHistory.Clear();
            MMDebugLogEvent.Trigger(new DebugLogItem(null, "", Time.frameCount, Time.time, 0, false));
        }
        public static void DebugLogTime(object message, string color = "", int timePrecision = 3, bool displayFrameCount = true)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            string callerObjectName = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            color = (color == "") ? "#00FFFF" : color;
            string colorPrefix = "";
            string colorSuffix = "";
            if (!string.IsNullOrEmpty(color))
            {
                colorPrefix = "<color=" + color + ">";
                colorSuffix = "</color>";
            }
            string output = "";
            if (displayFrameCount)
            {
                output += "<color=#82d3f9>[f" + Time.frameCount + "]</color> ";
            }
            output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
            output += callerObjectName + " : ";
            output += colorPrefix + message + colorSuffix;
            Debug.Log(output);

            DebugLogItem item = new DebugLogItem(message, color, Time.frameCount, Time.time, timePrecision, displayFrameCount);
            if (LogHistory.Count > _logHistoryMaxLength)
            {
                LogHistory.RemoveAt(0);
            }

            LogHistory.Add(item);
            MMDebugLogEvent.Trigger(item);

        }
        public struct MMDebugLogEvent
        {
            public delegate void Delegate(DebugLogItem item);
            static private event Delegate OnEvent;

            static public void Register(Delegate callback)
            {
                OnEvent += callback;
            }

            static public void Unregister(Delegate callback)
            {
                OnEvent -= callback;
            }

            static public void Trigger(DebugLogItem item)
            {
                OnEvent?.Invoke(item);
            }
        }

        #endregion

        #region EnableDisableDebugs
        public static bool DebugLogsEnabled
        {
            get
            {
                if (_debugLogEnabledSet)
                {
                    return _debugLogEnabled;
                }
                
                if (PlayerPrefs.HasKey(_editorPrefsDebugLogs))
                {
                    _debugLogEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugLogs) == 0) ? false : true;
                }
                else
                {
                    _debugLogEnabled = true;
                }

                _debugLogEnabledSet = true;
                return _debugLogEnabled;
            }
            private set
            {
	            _debugLogEnabledSet = true;
	            _debugLogEnabled = value;
            }
        }
        public static bool DebugDrawEnabled
        {
            get
            {
                #if UNITY_EDITOR
                    if (_debugDrawEnabledSet)
                    {
                        return _debugDrawEnabled;
                    }

                    if (PlayerPrefs.HasKey(_editorPrefsDebugDraws))
                    {
                        _debugDrawEnabled = (PlayerPrefs.GetInt(_editorPrefsDebugDraws) == 0) ? false : true;
                    }
                    else
                    {
                        _debugDrawEnabled = true;
                    }
                    _debugDrawEnabledSet = true;
                    return _debugDrawEnabled;
                #else
                    return false;
                #endif
            }
            private set { }
        }

        private const string _editorPrefsDebugLogs = "DebugLogsEnabled";
        private const string _editorPrefsDebugDraws = "DebugDrawsEnabled";
        public static void SetDebugLogsEnabled(bool status)
        {
            DebugLogsEnabled = status;
            _debugLogEnabled = status;
            #if UNITY_EDITOR
            int newStatus = status ? 1 : 0;
                PlayerPrefs.SetInt(_editorPrefsDebugLogs, newStatus);
            #endif
        }
        public static void SetDebugDrawEnabled(bool status)
        {
            DebugDrawEnabled = status;
            _debugDrawEnabled = status;
            #if UNITY_EDITOR
                int newStatus = status ? 1 : 0;
                PlayerPrefs.SetInt(_editorPrefsDebugDraws, newStatus);
            #endif
        }

        #endregion

        #region Casts
        public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);		
		}
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo && DebugDrawEnabled)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                Vector3[] points = new Vector3[8];

                float halfSizeX = size.x / 2f;
                float halfSizeY = size.y / 2f;

                points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY));
                points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY));
                points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY));
                points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY));
                
                points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction);
                points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction);
                points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction);
                points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction);
                                
                Debug.DrawLine(points[0], points[1], color);
                Debug.DrawLine(points[1], points[2], color);
                Debug.DrawLine(points[2], points[3], color);
                Debug.DrawLine(points[3], points[0], color);

                Debug.DrawLine(points[4], points[5], color);
                Debug.DrawLine(points[5], points[6], color);
                Debug.DrawLine(points[6], points[7], color);
                Debug.DrawLine(points[7], points[4], color);
                
                Debug.DrawLine(points[0], points[4], color);
                Debug.DrawLine(points[1], points[5], color);
                Debug.DrawLine(points[2], points[6], color);
                Debug.DrawLine(points[3], points[7], color);

            }
            return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
        }
        public static RaycastHit2D MonoRayCastNonAlloc(RaycastHit2D[] array, Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			if (Physics2D.RaycastNonAlloc(rayOriginPoint, rayDirection, array, rayDistance, mask) > 0)
			{
				return array[0];
			}
			return new RaycastHit2D();        	
		}
		public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			RaycastHit hit;
			Physics.Raycast(rayOriginPoint, rayDirection, out hit, rayDistance, mask);	
			return hit;
		}

        #endregion

        #region DebugOnScreen
        public static MMDebugOnScreenConsole _console;
        private const string _debugConsolePrefabPath = "MMDebugOnScreenConsole";
        public static void DebugOnScreen(string message)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole();
			_console.AddMessage(message, "", 30);
		}
		public static void DebugOnScreen(string label, object value, int fontSize=25)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole(fontSize);
			_console.AddMessage(label, value, fontSize);
		}
		public static void InstantiateOnScreenConsole(int fontSize=25)
		{
            if (!DebugLogsEnabled)
            {
                return;
            }

            if (_console == null)
            {
	            _console = (MMDebugOnScreenConsole) GameObject.FindObjectOfType(typeof(MMDebugOnScreenConsole));
            }


            if (_console == null)
			{
                GameObject loaded = UnityEngine.Object.Instantiate(Resources.Load(_debugConsolePrefabPath) as GameObject);
                loaded.name = "MMDebugOnScreenConsole";
                _console = loaded.GetComponent<MMDebugOnScreenConsole>();                
            }
		}
		public static void SetOnScreenConsole(MMDebugOnScreenConsole newConsole)
		{
			_console = newConsole;
		}

        #endregion

        #region DebugDraw
        public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
	    {
            if (!DebugDrawEnabled)
            {
                return;
            }

	        Gizmos.color = color;
	        Gizmos.DrawRay(origin, direction);
	       
			DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
	    }
	    public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.2f, float arrowHeadAngle = 35f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction, color);
	       
			DrawArrowEnd(false,origin,direction,color,arrowHeadLength,arrowHeadAngle);
	    }
		public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowLength, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction * arrowLength, color);

			DrawArrowEnd(false,origin,direction * arrowLength,color,arrowHeadLength,arrowHeadAngle);
		}
		public static void DebugDrawCross (Vector3 spot, float crossSize, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 tempOrigin = Vector3.zero;
			Vector3 tempDirection = Vector3.zero;

			tempOrigin.x = spot.x - crossSize / 2;
			tempOrigin.y = spot.y - crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
			tempDirection.y = 1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);

			tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y + crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
			tempDirection.y = -1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);
		}
		private static void DrawArrowEnd (bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            if (direction == Vector3.zero)
			{
				return;
			}
	        Vector3 right = Quaternion.LookRotation (direction) * Quaternion.Euler (arrowHeadAngle, 0, 0) * Vector3.back;
	        Vector3 left = Quaternion.LookRotation (direction) * Quaternion.Euler (-arrowHeadAngle, 0, 0) * Vector3.back;
	        Vector3 up = Quaternion.LookRotation (direction) * Quaternion.Euler (0, arrowHeadAngle, 0) * Vector3.back;
	        Vector3 down = Quaternion.LookRotation (direction) * Quaternion.Euler (0, -arrowHeadAngle, 0) * Vector3.back;
	        if (drawGizmos) 
	        {
	            Gizmos.color = color;
	            Gizmos.DrawRay (arrowEndPosition + direction, right * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, left * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, up * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, down * arrowHeadLength);
	        }
	        else
	        {
	            Debug.DrawRay (arrowEndPosition + direction, right * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, left * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, up * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, down * arrowHeadLength, color);
	        }
	    }
		public static void DrawHandlesBounds(Bounds bounds, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

#if UNITY_EDITOR
            Vector3 boundsCenter = bounds.center;
		    Vector3 boundsExtents = bounds.extents;
		  
			Vector3 v3FrontTopLeft     = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);
			Vector3 v3FrontTopRight    = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);
			Vector3 v3FrontBottomLeft  = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);
			Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);
			Vector3 v3BackTopLeft      = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);
			Vector3 v3BackTopRight     = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);
			Vector3 v3BackBottomLeft   = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);
			Vector3 v3BackBottomRight  = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);


			Handles.color = color;

			Handles.DrawLine (v3FrontTopLeft, v3FrontTopRight);
			Handles.DrawLine (v3FrontTopRight, v3FrontBottomRight);
			Handles.DrawLine (v3FrontBottomRight, v3FrontBottomLeft);
			Handles.DrawLine (v3FrontBottomLeft, v3FrontTopLeft);
		         
			Handles.DrawLine (v3BackTopLeft, v3BackTopRight);
			Handles.DrawLine (v3BackTopRight, v3BackBottomRight);
			Handles.DrawLine (v3BackBottomRight, v3BackBottomLeft);
			Handles.DrawLine (v3BackBottomLeft, v3BackTopLeft);
		         
			Handles.DrawLine (v3FrontTopLeft, v3BackTopLeft);
			Handles.DrawLine (v3FrontTopRight, v3BackTopRight);
			Handles.DrawLine (v3FrontBottomRight, v3BackBottomRight);
			Handles.DrawLine (v3FrontBottomLeft, v3BackBottomLeft);  
#endif
		}
        public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

#if UNITY_EDITOR

            Vector3 halfSize = size / 2f;

            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);
            
#endif
        }
        public static void DrawGizmoPoint(Vector3 position, float size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }
            Gizmos.color = color;
			Gizmos.DrawWireSphere(position,size);
		}
		public static void DrawCube (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),			
				position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (wireOnly)
            {
                Gizmos.DrawWireCube(offset, cubeSize);
            }
            else
            {
                Gizmos.DrawCube(offset, cubeSize);
            }
        }
		public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Gizmos.color = color;

			Vector3 v3TopLeft = new Vector3(center.x - size.x/2, center.y + size.y/2, 0);
			Vector3 v3TopRight = new Vector3(center.x + size.x/2, center.y + size.y/2, 0);;
			Vector3 v3BottomRight = new Vector3(center.x + size.x/2, center.y - size.y/2, 0);;
			Vector3 v3BottomLeft = new Vector3(center.x - size.x/2, center.y - size.y/2, 0);;

			Gizmos.DrawLine(v3TopLeft,v3TopRight);
			Gizmos.DrawLine(v3TopRight,v3BottomRight);
			Gizmos.DrawLine(v3BottomRight,v3BottomLeft);
			Gizmos.DrawLine(v3BottomLeft,v3TopLeft);
        }
        public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Matrix4x4 rotationMatrix, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            GL.PushMatrix();

            Gizmos.color = color;

            Vector3 v3TopLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y + size.y / 2, 0);
            Vector3 v3TopRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y + size.y / 2, 0); ;
            Vector3 v3BottomRight = rotationMatrix * new Vector3(center.x + size.x / 2, center.y - size.y / 2, 0); ;
            Vector3 v3BottomLeft = rotationMatrix * new Vector3(center.x - size.x / 2, center.y - size.y / 2, 0); ;

            
            Gizmos.DrawLine(v3TopLeft, v3TopRight);
            Gizmos.DrawLine(v3TopRight, v3BottomRight);
            Gizmos.DrawLine(v3BottomRight, v3BottomLeft);
            Gizmos.DrawLine(v3BottomLeft, v3TopLeft);
            GL.PopMatrix();
        }
        public static void DrawRectangle (Rect rectangle, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 pos = new Vector3( rectangle.x + rectangle.width/2, rectangle.y + rectangle.height/2, 0.0f );
			Vector3 scale = new Vector3 (rectangle.width, rectangle.height, 0.0f );

			MMDebug.DrawRectangle (pos, color, scale); 
		}
		public static void DrawRectangle  (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),	
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}
        public static void DrawPoint (Vector3 position, Color color, float size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3[] points = new Vector3[] 
			{
				position + (Vector3.up * size), 
				position - (Vector3.up * size), 
				position + (Vector3.right * size), 
				position - (Vector3.right * size), 
				position + (Vector3.forward * size), 
				position - (Vector3.forward * size)
			}; 		

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[4], points[5], color ); 
			Debug.DrawLine (points[0], points[2], color ); 
			Debug.DrawLine (points[0], points[3], color ); 
			Debug.DrawLine (points[0], points[4], color ); 
			Debug.DrawLine (points[0], points[5], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[1], points[3], color ); 
			Debug.DrawLine (points[1], points[4], color ); 
			Debug.DrawLine (points[1], points[5], color ); 
			Debug.DrawLine (points[4], points[2], color ); 
			Debug.DrawLine (points[4], points[3], color ); 
			Debug.DrawLine (points[5], points[2], color ); 
			Debug.DrawLine (points[5], points[3], color ); 
		}

        #endregion

        #region Info

        public static string GetSystemInfo()
        {
            string result = "SYSTEM INFO";

            #if UNITY_IOS
                 result += "\n[iPhone generation]iPhone.generation.ToString()";
            #endif

            #if UNITY_ANDROID
                result += "\n[system info]" + SystemInfo.deviceModel;
            #endif

            result += "\n<color=#FFFFFF>Device Type :</color> " + SystemInfo.deviceType;
            result += "\n<color=#FFFFFF>OS Version :</color> " + SystemInfo.operatingSystem;
            result += "\n<color=#FFFFFF>System Memory Size :</color> " + SystemInfo.systemMemorySize;
            result += "\n<color=#FFFFFF>Graphic Device Name :</color> " + SystemInfo.graphicsDeviceName + " (version " + SystemInfo.graphicsDeviceVersion + ")";
            result += "\n<color=#FFFFFF>Graphic Memory Size :</color> " + SystemInfo.graphicsMemorySize;
            result += "\n<color=#FFFFFF>Graphic Max Texture Size :</color> " + SystemInfo.maxTextureSize;
            result += "\n<color=#FFFFFF>Graphic Shader Level :</color> " + SystemInfo.graphicsShaderLevel;
            result += "\n<color=#FFFFFF>Compute Shader Support :</color> " + SystemInfo.supportsComputeShaders;

            result += "\n<color=#FFFFFF>Processor Count :</color> " + SystemInfo.processorCount;
            result += "\n<color=#FFFFFF>Processor Type :</color> " + SystemInfo.processorType;
            result += "\n<color=#FFFFFF>3D Texture Support :</color> " + SystemInfo.supports3DTextures;
            result += "\n<color=#FFFFFF>Shadow Support :</color> " + SystemInfo.supportsShadows;

            result += "\n<color=#FFFFFF>Platform :</color> " + Application.platform;
            result += "\n<color=#FFFFFF>Screen Size :</color> " + Screen.width + " x " + Screen.height;
            result += "\n<color=#FFFFFF>DPI :</color> " + Screen.dpi;

            return result;
        }

        #endregion
        
        #region Console
        
        public static void ClearConsole()
        {
            Type logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            if (logEntries != null)
            {
	            MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
	            if (clearMethod != null)
	            {
		            clearMethod.Invoke(null, null);    
	            }
            }
        }
        
        #endregion
    }
}