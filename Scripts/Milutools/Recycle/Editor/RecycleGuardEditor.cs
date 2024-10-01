#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Milutools.Recycle.Editor
{
    [CustomEditor(typeof(RecycleGuard))]
    public class RecycleGuardEditor : UnityEditor.Editor
    {
        private static Dictionary<PoolLifeCyclePolicy, bool> foldout = new();
        private static Dictionary<PoolLifeCyclePolicy, RecycleContext> objFoldout = new();
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Active prefab count: " + RecyclePool.contexts.Count);
            EditorGUILayout.Separator();
            
            foreach (PoolLifeCyclePolicy policy in Enum.GetValues(typeof(PoolLifeCyclePolicy)))
            {
                if (!foldout.ContainsKey(policy))
                {
                    foldout.Add(policy, false);
                    objFoldout.Add(policy, null);
                }

                var collection = 
                    RecyclePool.contexts.Values.Where(x => x.LifeCyclePolicy == policy).ToArray();
                
                EditorGUI.indentLevel = 0;
                foldout[policy] = EditorGUILayout.Foldout(foldout[policy], policy + " (" + collection.Length + ")", true);
                if (!foldout[policy])
                {
                    continue;
                }
                foreach (var context in collection)
                {
                    var open = objFoldout[policy] == context;
                    EditorGUI.indentLevel = 1;
                    open = EditorGUILayout.Foldout(open, context.Name, true);
                    if (open)
                    {
                        EditorGUI.indentLevel = 2;
                        EditorGUILayout.LabelField("Base Amount", context.MinimumObjectCount.ToString());
                        EditorGUILayout.LabelField("Current Amount", context.Objects.Count.ToString());
                        EditorGUILayout.LabelField("Current Usage", context.CurrentUsage.ToString());
                        EditorGUILayout.LabelField("Recent Usage", (context.PeriodUsage / RecycleGuard.usageTrackCount).ToString());
                        if (objFoldout[policy] != context)
                        {
                            objFoldout[policy] = context;
                        }
                    }
                }
            }
            
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
