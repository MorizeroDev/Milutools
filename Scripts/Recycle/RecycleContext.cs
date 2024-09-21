using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Milutools.Recycle
{
    public class RecycleContext
    {
        public GameObject Prefab { get; internal set; }
        public string Name { get; internal set; }
        public Stack<RecycleCollection> ObjectPool { get; } = new();
        public List<RecycleCollection> AllObjects { get; } = new();
        public PoolLifeCyclePolicy LifeCyclePolicy { get; internal set; }
        
        internal Type[] ComponentTypes;
        internal object ID;

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
                collection.Components.Add(ComponentTypes[i], recyclableComponent.Components[i]);
            }

            recyclableComponent.Initialize(this, collection);
            
            AllObjects.Add(collection);
            
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
                ObjectPool.Push(Produce());
            }
        }

        internal RecycleCollection Request()
        {
            if (!ObjectPool.TryPop(out var collection))
            {
                collection = Produce();
            }

            collection.RecyclingController.Using = true;
            return collection;
        }

        internal void ReturnToPool(RecycleCollection collection)
        {
            collection.GameObject.SetActive(false);
            ObjectPool.Push(collection);
        }
    }
}
