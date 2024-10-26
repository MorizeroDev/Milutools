using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Milutools.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Milutools.Recycle
{
    public class RecycleContext
    {
        public GameObject Prefab { get; internal set; }
        public string Name { get; internal set; }
        public IReadOnlyList<RecycleCollection> AllObjects => Objects;
        public PoolLifeCyclePolicy LifeCyclePolicy { get; internal set; }
        public uint MinimumObjectCount { get; internal set; }
        
        internal List<RecycleCollection> Objects { get; } = new();
        
        internal Type[] ComponentTypes;
        internal object ID;
        
        internal readonly Queue<uint> UsageRecords = new();
        internal uint PeriodUsage = 0;
        internal uint CurrentUsage = 0;
        
        private Stack<RecycleCollection> _objectPool { get; } = new();
        
        public T GetID<T>() where T : Enum
        {
            return (T)ID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Transform GetPoolParent()
        {
            return LifeCyclePolicy switch
            {
                PoolLifeCyclePolicy.Eternity => RecyclePool.poolParent,
                PoolLifeCyclePolicy.DestroyOnLoad => RecyclePool.scenePoolParent,
                _ => null
            };
        }
        
        private RecycleCollection Produce()
        {
            var gameObject = Object.Instantiate(Prefab, GetPoolParent());
            gameObject.name = $"[RE{gameObject.GetInstanceID()}] {Name}";
            gameObject.SetActive(false);
            
            var recyclableComponent = gameObject.GetComponent<RecyclableObject>();
            
            if (LifeCyclePolicy == PoolLifeCyclePolicy.Eternity)
            {
                Object.DontDestroyOnLoad(gameObject);
            }

            var collection = new RecycleCollection()
            {
                GameObject = gameObject,
                Transform = gameObject.transform,
                RecyclingController = recyclableComponent,
                MainComponent = recyclableComponent.MainComponent
            };

            for (var i = 0; i < ComponentTypes.Length; i++)
            {
#if UNITY_EDITOR
                if (collection.Components.ContainsKey(ComponentTypes[i]))
                {
                    DebugLog.LogError($"You are trying to link multiple components with a same type '{ComponentTypes[i]}'\n" +
                                      $", this is not supported. (ID: {ID})");
                    continue;
                }
#endif
                collection.Components.Add(ComponentTypes[i], recyclableComponent.Components[i]);
            }

            recyclableComponent.Initialize(this, collection);
            
            Objects.Add(collection);
            
            return collection;
        }

        internal void RecycleAllObjects()
        {
            foreach (var obj in AllObjects)
            {
                if (!obj.RecyclingController.Using)
                {
                    continue;
                }
                obj.RecyclingController.WaitForRecycle();
            }
        }

        internal void Prepare(uint count)
        {
            for (var i = 0; i < count; i++)
            {
                _objectPool.Push(Produce());
            }
        }

        internal RecycleCollection Request()
        {
            if (!_objectPool.TryPop(out var collection))
            {
                collection = Produce();
            }

            CurrentUsage++;
            collection.RecyclingController.Using = true;
            return collection;
        }

        internal void ReturnToPool(RecycleCollection collection)
        {
            CurrentUsage--;
            collection.Transform.SetParent(GetPoolParent());
            collection.GameObject.SetActive(false);
            _objectPool.Push(collection);
        }

        internal int GetObjectCount()
            => _objectPool.Count;
    }
}
