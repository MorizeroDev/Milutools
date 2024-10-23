using System;
using UnityEditor;
using UnityEngine;

namespace Milutools.Audio.Editor
{
    [CustomEditor(typeof(AudioPreset))]
    public class AudioPresetEditor : UnityEditor.Editor
    {
        private void DrawInspector(string title, AudioPreset.PresetData data)
        {
            EditorGUILayout.LabelField(title);
            EditorGUI.indentLevel++;
            data.Behaviour = (AudioPreset.AudioBehaviour)EditorGUILayout.EnumPopup("Behaviour", data.Behaviour);
            if (data.Behaviour == AudioPreset.AudioBehaviour.Replace)
            {
                var resources = (AudioResources)
                    EditorGUILayout.ObjectField("Resources", data.Resources, typeof(AudioResources), true);
                if (resources != data.Resources)
                {
                    data.Resources = resources;
                    data.StartTime = 0f;
                    data.ID = -1;
                }
                if (!data.Resources)
                {
                    goto end;
                }
                
                var type = data.Resources.GetType();
                while (type != null && type.BaseType != null)
                {
                    if (type.IsGenericType && type.BaseType == typeof(AudioResources))
                    {
                        break;
                    }
                    type = type.BaseType;
                }

                var enumType = type.GetGenericArguments()[0];
                var id = (int)(object)EditorGUILayout.EnumPopup("ID", (Enum)Enum.ToObject(enumType, data.ID));
                if (id != data.ID)
                {
                    data.ID = id;
                    data.StartTime = 0f;
                }

                var clip = resources.GetClip(id);
                if (!clip)
                {
                    EditorGUILayout.LabelField("⚠ Specific ID links to no audio.");
                }
            }
            
            end:
            EditorGUI.indentLevel--;
        }
        public override void OnInspectorGUI()
        {
            var preset = (AudioPreset)target;
            DrawInspector("BGM", preset.BGM);
            DrawInspector("BGS", preset.BGS);
        }
    }
}
