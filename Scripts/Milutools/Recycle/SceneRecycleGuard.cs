using System;
using System.Collections.Generic;
using System.Text;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;

namespace Milutools.Recycle
{
    [AddComponentMenu("")]
    internal class SceneRecycleGuard : MonoBehaviour
    {
        public static SceneRecycleGuard Instance { get; private set; }
        
        internal readonly List<EnumIdentifier> PrefabInScene = new();
        internal readonly StringBuilder DestroyRecords = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            foreach (var prefab in PrefabInScene)
            {
                RecyclePool.contexts.Remove(prefab);
            }
        }

        private void Update()
        {
            if (DestroyRecords.Length > 0)
            {
                DebugLog.LogError("Several recyclable objects were unexpectedly destroyed, this will break the recycle pool!\n" +
                               DestroyRecords);
                DestroyRecords.Clear();
            }
        }
    }
}
