using System;
using System.Collections.Generic;
using System.Linq;
using Milutools.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.Recycle
{
    public static class RecyclePool
    {
        internal static readonly Dictionary<RecycleKey, RecycleContext> contexts = new();

        private static bool initialized = false;

        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            CreateSceneRecycleGuard();
            
            SceneManager.sceneLoaded += SceneLoadedCallback;
        }

        private static void CreateSceneRecycleGuard()
        {
            var guard = new GameObject("[SceneRecycleGuard]", typeof(SceneRecycleGuard));
            guard.SetActive(true);
        }

        private static void SceneLoadedCallback(Scene scene, LoadSceneMode mode)
        {
            CreateSceneRecycleGuard();
        }
        
        /// <summary>
        /// To ensure the prefab is registered.
        /// You must first register it before requesting a recyclable object from the prefab.
        /// </summary>
        /// <param name="id">an enum value to identify a specific prefab</param>
        /// <param name="prefab">the prefab object</param>
        /// <param name="lifeCyclePolicy">when the prefab and its objects get destroyed</param>
        public static void EnsurePrefabRegistered<T>(T id, GameObject prefab, 
            PoolLifeCyclePolicy lifeCyclePolicy = PoolLifeCyclePolicy.DestroyOnLoad) where T : Enum
        {
            EnsureInitialized();

            var key = new RecycleKey()
            {
                EnumType = typeof(T),
                Value = id
            };
            
            // 强制检查
            if (contexts.TryGetValue(key, out var existing))
            {
                if (existing.Prefab == prefab && existing.LifeCyclePolicy == lifeCyclePolicy)
                {
                    DebugLog.LogWarning($"Prefab '{key}' is already registered.");
                    return;
                }
                
                throw new ArgumentException($"Prefab '{key}' is already registered. " +
                                            $"Each prefab must have a unique name.", nameof(id));
            }

            if (lifeCyclePolicy == PoolLifeCyclePolicy.DestroyOnLoad)
            {
                SceneRecycleGuard.Instance.PrefabInScene.Add(key);
            }

            var recyclableObject = prefab.GetComponent<RecyclableObject>();
            if (!recyclableObject)
            {
                throw new InvalidOperationException($"Prefab '{key}' must have a RecyclableObject component. " +
                                                    $"Please add the component manually before registering.");
            }

            recyclableObject.IsPrefab = true;
            
            var context = new RecycleContext()
            {
                Prefab = prefab,
                Name = $"{typeof(T).FullName}.{id}",
                ID = id,
                LifeCyclePolicy = lifeCyclePolicy,
                ComponentTypes = recyclableObject.Components.Select(x => x.GetType()).ToArray()
            };
            
            contexts.Add(key, context);
        }

        /// <summary>
        /// Prepare a specific amount of objects beforehand
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        /// <param name="count">how many objects should be prepared</param>
        public static void Prepare<T>(T prefab, int count = 10) where T : Enum
        {
            // Debug.Log($"Preparing RecyclableObjects for prefab: {prefab}, count: {count}");
            var key = new RecycleKey()
            {
                EnumType = typeof(T),
                Value = prefab
            };
            
            if (!contexts.ContainsKey(key))
            {
                throw new ArgumentException($"Prefab '{key}' is not registered. " +
                                            $"Please register the prefab before calling Prepare.", nameof(prefab));
            }

            DebugLog.Log($"Current pool size for {key}: {contexts[key].GetObjectCount()}");

            contexts[key].Prepare(count);
        }

        /// <summary>
        /// Retrieve an object with the specified prefab ID from the pool and obtain its object set,
        /// including all associated components and related information.
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        /// <param name="parent">the parent of the retrieved object to be set</param>
        /// <returns></returns>
        public static RecycleCollection RequestWithCollection<T>(T prefab, Transform parent = null) where T : Enum
        {
            var key = new RecycleKey()
            {
                EnumType = typeof(T),
                Value = prefab
            };
            var collection = contexts[key].Request();
            collection.Transform.SetParent(parent, false);
            return collection;
        }
        
        /// <summary>
        /// Retrieve an object with the specified prefab ID from the pool and obtain its GameObject.
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        /// <param name="parent">the parent of the retrieved object to be set</param>
        /// <returns></returns>
        public static GameObject Request<T>(T prefab, Transform parent = null) where T : Enum
        {
            return RequestWithCollection(prefab, parent).GameObject;
        }

        /// <summary>
        /// Retrieve an object with the specified prefab ID from the pool and obtain its associated primary component.
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        /// <param name="parent">the parent of the retrieved object to be set</param>
        /// <typeparam name="T">Component Type</typeparam>
        /// <typeparam name="E">Prefab ID Enum</typeparam>
        /// <returns></returns>
        public static T Request<T, E>(E prefab, Transform parent = null) where T : Component where E : Enum
        {
            return (T)RequestWithCollection(prefab, parent).MainComponent;
        }
        
        /// <summary>
        /// Retrieve an object with the specified prefab ID from the pool and obtain its associated component of a specific type.
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        /// <param name="parent">the parent of the retrieved object to be set</param>
        /// <typeparam name="T">Component Type</typeparam>
        /// <typeparam name="E">Prefab ID Enum</typeparam>
        /// <returns></returns>
        public static T RequestWithComponent<T, E>(E prefab, Transform parent = null) where T : Component where E : Enum
        {
            return RequestWithCollection(prefab, parent).GetComponent<T>();
        }

        /// <summary>
        /// Immediately return all objects with the specified prefab ID to the pool.
        /// </summary>
        /// <param name="prefab">an enum value to identify a specific prefab</param>
        public static void RecycleAllObjects<T>(T prefab) where T : Enum
        {
            var key = new RecycleKey()
            {
                EnumType = typeof(T),
                Value = prefab
            };
            contexts[key].RecycleAllObjects();
        }
    }
}
