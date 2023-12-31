﻿/*
 * JEngine作者匠心打造的Mono适配器编辑器脚本，作者已经代替你掉了头发，帮你写出了这个编辑器脚本，让你能够直接看对象序列化后的字段
 */
using UnityEditor;
using UnityEngine;
using JEngine.Editor;
using UnityEditor.AnimatedValues;

namespace JEngine.Core.DO_NOT_USE
{
 
[CustomEditor(typeof(ClassBindNonMonoBehaviourAdapter.Adaptor), true)]
public class ClassBindNonMonoBehaviourAdapterEditor : UnityEditor.Editor
{
    private bool _displaying;
    private readonly AnimBool[] _fadeGroup = new AnimBool[SerializeILTypeInstance.FadeGroupNum];

    private async void OnEnable()
    {
        _displaying = true;
        await SerializeILTypeInstance.OnEnable(_fadeGroup, () =>
        {
            if (_displaying)
            {
                Repaint();
            }
        });
    }

    private void OnDestroy()
    {
        SerializeILTypeInstance.OnDestroy(ref _displaying);
    }

    private void OnDisable()
    {
        SerializeILTypeInstance.OnDisable(_fadeGroup,Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ClassBindNonMonoBehaviourAdapter.Adaptor clr = target as ClassBindNonMonoBehaviourAdapter.Adaptor;
        if (clr is null) return;
        
        var instance = clr.ILInstance;
        if (instance != null)
        {
            EditorGUILayout.LabelField("Script", clr.ILInstance.Type.Name);
            SerializeILTypeInstance.SerializeJBehaviourType(_fadeGroup, instance);

            int index = 0;
            foreach (var i in instance.Type.FieldMapping)
            {
                //这里是取的所有字段，没有处理不是public的
                var type = instance.Type.FieldTypes[index];
                var cType = type.TypeForCLR;
                var objName = i.Key;
                GUI.enabled = true;
                index++;

                if (SerializeILTypeInstance.NeedToHide(instance, objName)) continue;

                foreach (var method in SerializeILTypeInstance.GetSerializeMethods())
                {
                    System.Object[] parameters= {_fadeGroup, cType, type, instance, i, objName};
                    var result = method.Value.Invoke(null, parameters);
                    if ((bool) result)
                    {
                        break;
                    }

                    if (method.Key == 0)
                    {
                        EditorGUILayout.LabelField(objName, "(null)");
                    }
                }
            }

        }
        // 应用属性修改
        serializedObject.ApplyModifiedProperties();
    }
}
}