using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Milutools.Recycle
{
    internal class SceneRecycleGuard : MonoBehaviour
    {
        public static SceneRecycleGuard Instance { get; private set; }
        
        internal readonly List<RecycleKey> PrefabInScene = new();
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
                Debug.LogError("Several recyclable objects were unexpectedly destroyed, this will break the recycle pool!\n" +
                               DestroyRecords);
                DestroyRecords.Clear();
            }
        }
    }
}
