using System;
using System.Collections.Generic;
using System.Linq;
using Milutools.Logger;
using UnityEngine;
using UnityEngine.Serialization;

namespace Milutools.Recycle
{
    [DisallowMultipleComponent]
    public sealed class RecyclableObject : MonoBehaviour
    {
        [FormerlySerializedAs("RecycleTimeOut")]
        [Tooltip("Automatically return the object to the pool after specific seconds.\n* -1 means disabling auto recycling.")]
        public float AutoRecycleTimeOut = -1f;
        
        [FormerlySerializedAs("LinkComponent")] 
        [Tooltip("Link a main component, then you can access it using RecyclePool.Request<T, E> function.\n" +
                 "This would avoid using GetComponent function frequently as GetComponent function may made the program slow.")]
        public Component MainComponent;
        
        [FormerlySerializedAs("DictComponent")] 
        [Tooltip("Link more components, then you can access it using RecycleCollection.GetComponent function.\n" +
                 "This would avoid using GetComponent function frequently as GetComponent function may made the program slow.\n" +
                 "NOTICE: involving multiple components with a same type is not supported.")]
        public Component[] Components;

        internal bool Using { get; set; }
        internal bool IsPrefab { get; set; } = false;
        internal bool ReadyToDestroy = false;
        
        private RecycleCollection _recycleCollection;
        private RecycleContext _parentContext;
        private int _objectHash;
        
        private float recycleTick = 0f;
        
        internal void Initialize(RecycleContext context, RecycleCollection collection)
        {
            _parentContext = context;
            _recycleCollection = collection;
            _objectHash = GetHashCode();
            
            DebugLog.Log($"RecyclableObject created: Hash={_objectHash}, Name={gameObject.name}, PrefabName={context.Name}");
        }
        
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (IsPrefab)
            {
                throw new Exception(
                    "The prefab object is unexpectedly destroyed, the recycle pool would fail when producing new objects!");
            }
            
            if (_parentContext == null)
            {
                return;
            }

            if (!ReadyToDestroy)
            {
                if (_parentContext.LifeCyclePolicy == PoolLifeCyclePolicy.Eternity)
                {
                    throw new Exception($"RecyclableObject is unexpectedly destroyed, this has broken the recycle pool: Hash={_objectHash}, Name={gameObject.name}, PrefabName={_parentContext.Name}");
                }
                else
                {
                    SceneRecycleGuard.Instance.DestroyRecords.AppendLine($"Hash={_objectHash}, Name={gameObject.name}, PrefabName={_parentContext.Name}");
                }
            }
            else
            {
                _parentContext.CurrentUsage--;
            }
            
            DebugLog.Log($"RecyclableObject destroyed: Hash={_objectHash}, Name={gameObject.name}, PrefabName={_parentContext.Name}");
#endif
        }

        /// <summary>
        /// Return the object to the pool
        /// </summary>
        public void WaitForRecycle()
        {
#if UNITY_EDITOR
            if (IsPrefab)
            {
                DebugLog.LogError("You are trying to recycle a prefab, this is not allowed.");
                return;
            }
            if (_parentContext == null)
            {
                DebugLog.LogError("You are trying to recycle an object that is not managed by the recycle pool, this is not allowed.");
                return;
            }
#endif
            Using = false;
            _recycleCollection.Transform.SetParent(null);
            _parentContext.ReturnToPool(_recycleCollection);
        }
        
        private void FixedUpdate()
        {
#if UNITY_EDITOR
            if (IsPrefab)
            {
                DebugLog.LogWarning("The prefab for the recycle pool must be inactive.");
                return;
            }
            
            if (_parentContext == null)
            {
                DebugLog.LogWarning("This object is not managed by the recycle pool, please use 'RecyclePool.Request' function to create the object.");
                return;
            }
#endif
            
            if (AutoRecycleTimeOut <= 0f)
            {
                return;
            }
            
            recycleTick += Time.fixedDeltaTime;
            if (recycleTick >= AutoRecycleTimeOut)
            {
                WaitForRecycle();
                recycleTick = 0f;
            }
        }
    }
}
