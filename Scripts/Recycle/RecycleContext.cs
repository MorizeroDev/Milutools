using System;
using System.Collections.Generic;
using Milutools.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Milutools.Recycle
{
    public class RecycleContext
    {
        public GameObject Prefab { get; internal set; }
        public string Name { get; internal set; }
        public IReadOnlyList<RecycleCollection> AllObjects => _allObjects;
        public PoolLifeCyclePolicy LifeCyclePolicy { get; internal set; }
        
        internal Type[] ComponentTypes;
        internal object ID;
        
        private Stack<RecycleCollection> _objectPool { get; } = new();
        private List<RecycleCollection> _allObjects { get; } = new();
        
        public T GetID<T>() where T : Enum
        {
            return (T)ID;
        }
        
        private RecycleCollection Produce()
        {
            var gameObject = Object.Instantiate(Prefab);
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
            
            _allObjects.Add(collection);
            
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
                obj.Transform.SetParent(null);
                obj.RecyclingController.WaitForRecycle();
            }
        }

        internal void Prepare(int count)
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

            collection.RecyclingController.Using = true;
            return collection;
        }

        internal void ReturnToPool(RecycleCollection collection)
        {
            collection.GameObject.SetActive(false);
            _objectPool.Push(collection);
        }

        internal int GetObjectCount()
            => _objectPool.Count;
    }
}
