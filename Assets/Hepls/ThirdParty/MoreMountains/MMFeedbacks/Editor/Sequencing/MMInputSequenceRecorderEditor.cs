using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks {
  [CustomEditor(typeof(MMInputSequenceRecorder), true)]
  [CanEditMultipleObjects]
  public class MMInputSequenceRecorderEditor : Editor {
    protected SerializedProperty _Recording;
    protected float _inspectorWidth;
    protected int _externalMargin = 10;
    protected Rect _rect;
    protected Color _recordingColor = Color.red;
    protected Color _recordingTextColor = Color.white;
    protected Vector2 _boxPosition;
    protected Vector2 _boxSize;
    protected GUIStyle _recordingStyle;
    protected MMInputSequenceRecorder _targetRecorder;
    protected Event _currentEvent;
    public override bool RequiresConstantRepaint() {
      return true;
    }
    protected virtual void OnEnable() {
      _Recording = serializedObject.FindProperty("Recording");

      _recordingStyle = new GUIStyle();
      _recordingStyle.normal.textColor = Color.white;
      _recordingStyle.fontSize = 30;
      _recordingStyle.alignment = TextAnchor.MiddleCenter;
      _targetRecorder = (MMInputSequenceRecorder) target;

      System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler",
        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
      EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction) info.GetValue(null);
      value += EditorGlobalKeyPress;
      info.SetValue(null, value);
    }
    protected virtual void EditorGlobalKeyPress() {
      if (Application.isPlaying) {
        return;
      }

      HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
      _currentEvent = Event.current;

      if (_currentEvent == null) {
        return;
      }

      DetectStartAndEnd();
      EditorDetectRecording();
    }
    protected virtual void DetectStartAndEnd() {
      if (_currentEvent.isKey) {
        if (!_targetRecorder.Recording) {
          if ((_currentEvent.keyCode == _targetRecorder.StartRecordingHotkey) &&
              (_currentEvent.type == EventType.KeyDown)) {
            _targetRecorder.StartRecording();
          }
        } else {
          if ((_currentEvent.keyCode == _targetRecorder.StopRecordingHotkey) &&
              (_currentEvent.type == EventType.KeyDown)) {
            _targetRecorder.StopRecording();
          }
        }
      }
    }
    protected virtual void EditorDetectRecording() {
      if (_targetRecorder.Recording && (_targetRecorder.SequenceScriptableObject != null)) {
        if (_currentEvent.isKey) {
          foreach (MMSequenceTrack track in _targetRecorder.SequenceScriptableObject.SequenceTracks) {
            if (_currentEvent.keyCode == (track.Key)) {
              if (track.State == MMSequenceTrackStates.Up) {
                track.State = MMSequenceTrackStates.Idle;
              }

              if (_currentEvent.type == EventType.KeyDown) {
                if (track.State != MMSequenceTrackStates.Down) {
                  _targetRecorder.AddNoteToTrack(track);
                }

                track.State = MMSequenceTrackStates.Down;
              }

              if (_currentEvent.type == EventType.KeyUp) {
                track.State = MMSequenceTrackStates.Up;
              }
            }
          }
        }
      }
    }
    public override void OnInspectorGUI() {
      serializedObject.Update();
      Undo.RecordObject(target, "Modified Sequence Recorder");

      _inspectorWidth = EditorGUIUtility.currentViewWidth - 24;
      if (_Recording.boolValue) {
        GUILayout.Box("", GUILayout.Width(_inspectorWidth - _externalMargin), GUILayout.Height(50));
        _boxPosition = GUILayoutUtility.GetLastRect().position;
        _boxSize = GUILayoutUtility.GetLastRect().size;
        _rect.x = _boxPosition.x;
        _rect.y = _boxPosition.y;
        _rect.width = _boxSize.x;
        _rect.height = _boxSize.y;
        EditorGUI.DrawRect(_rect, _recordingColor);

        EditorGUI.LabelField(_rect, "RECORDING", _recordingStyle);
      }

      DrawDefaultInspector();
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

      if (!_Recording.boolValue) {
        if (GUILayout.Button("Start Recording")) {
          _targetRecorder.StartRecording();
        }
      } else {
        if (GUILayout.Button("Stop Recording")) {
          _targetRecorder.StopRecording();
        }
      }

      serializedObject.ApplyModifiedProperties();
    }
  }
}