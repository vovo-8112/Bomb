using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    public class MMExecutionOrderAttribute : Attribute
    {
#if UNITY_EDITOR
        public int ExecutionOrder = 0;

        protected static Dictionary<MonoScript, MMExecutionOrderAttribute> _monoScripts;
        protected static Type _executionOrderAttributeType;
        protected static Assembly _typeAssembly;
        protected static Type[] _assemblyTypes;
        public MMExecutionOrderAttribute(int newExecutionOrder)
        {
            ExecutionOrder = newExecutionOrder;
        }
            [InitializeOnLoadMethod]        
            protected static void ModifyExecutionOrder()
            {
                Initialization();

                FindExecutionOrderAttributes();
            
                if (ExecutionOrderHasChanged())
                {
                    UpdateExecutionOrders();
                }
            }
            protected static void Initialization()
            {
                _monoScripts = new Dictionary<MonoScript, MMExecutionOrderAttribute>();
                _executionOrderAttributeType = typeof(MMExecutionOrderAttribute);
                _typeAssembly = _executionOrderAttributeType.Assembly;
                _assemblyTypes = _typeAssembly.GetTypes();
            }
            protected static void FindExecutionOrderAttributes()
            {
                foreach (Type assemblyType in _assemblyTypes)
                {
                    if (!HasExecutionOrderAttribute(assemblyType))
                    {
                        continue;
                    }

                    object[] attributes = assemblyType.GetCustomAttributes(_executionOrderAttributeType, false);
                    MMExecutionOrderAttribute attribute = attributes[0] as MMExecutionOrderAttribute;

                    string asset = "";
                    string[] guids = AssetDatabase.FindAssets(assemblyType.Name + " t:script");

                    if (guids.Length != 0)
                    {
                        foreach (string guid in guids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            string filename = Path.GetFileNameWithoutExtension(assetPath);
                            if (filename == assemblyType.Name)
                            {
                                asset = guid;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("MMTools' ExecutionOrderAttribute : Can't change "+ assemblyType.Name + "'s execution order");
                        return;
                    }

                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(asset));
                    _monoScripts.Add(monoScript, attribute);
                }
            }
            protected static bool HasExecutionOrderAttribute(Type assemblyType)
            {
                object[] attributes = assemblyType.GetCustomAttributes(_executionOrderAttributeType, false);
                return (attributes.Length == 1);
            }
            protected static bool ExecutionOrderHasChanged()
            {
                bool executionOrderHasChanged = false;
                foreach (KeyValuePair<MonoScript, MMExecutionOrderAttribute> monoScript in _monoScripts)
                {
                    if (monoScript.Key != null)
                    {
                        if (MonoImporter.GetExecutionOrder(monoScript.Key) != monoScript.Value.ExecutionOrder)
                        {
                            executionOrderHasChanged = true;
                            break;
                        }
                    }                    
                }
                return executionOrderHasChanged;
            }
            protected static void UpdateExecutionOrders()
            {
                foreach (KeyValuePair<MonoScript, MMExecutionOrderAttribute> monoScript in _monoScripts)
                {
                    if (monoScript.Key != null)
                    {
                        if (MonoImporter.GetExecutionOrder(monoScript.Key) != monoScript.Value.ExecutionOrder)
                        {
                            MonoImporter.SetExecutionOrder(monoScript.Key, monoScript.Value.ExecutionOrder);
                        }
                    }
                }
            }

#endif
    }
}
