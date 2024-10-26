using System;
using System.Collections.Generic;
using System.Linq;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.Recycle
{
    public static class RecyclePool
    {
        /// <summary>
        /// When enabled, the object pool will track the usage of objects and periodically release
        /// the excess, infrequently used objects generated during peak usage.
        /// However, if the number of prefabs in the pool is large,
        /// enabling this option may cause stuttering.
        /// </summary>
        public static bool AutoReleaseUnusedObjects { get; set; } = true;
        
        internal static readonly Dictionary<EnumIdentifier, RecycleContext> contexts = new();

        private static bool initialized = false;

        internal static Transform scenePoolParent { get; private set; }
        internal static Transform poolParent { get; private set; }

        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            var go = new GameObject("[Object Pool]", typeof(RecycleGuard));
            GameObject.DontDestroyOnLoad(go);
            go.SetActive(true);
            poolParent = go.transform;
        }

        internal static void CreateSceneRecycleGuard()
        {
            if (scenePoolParent)
            {
                return;
            }
            var guard = new GameObject("[Scene Object Pool]", typeof(SceneRecycleGuard));
            guard.SetActive(true);
            scenePoolParent = guard.transform;
        }
        
        /// <summary>
        /// To ensure the prefab is registered.
        /// You must first register it before requesting a recyclable object from the prefab.
        /// </summary>
        /// <param name="id">an enum value to identify a specific prefab</param>
        /// <param name="prefab">the prefab object</param>
        /// <param name="minimumObjectCount">set the minimum object count and prepare specific amount of objects beforehand</param>
        /// <param name="lifeCyclePolicy">when the prefab and its objects get destroyed</param>
        public static void EnsurePrefabRegistered<T>(T id, GameObject prefab, 
            uint minimumObjectCount,
            PoolLifeCyclePolicy lifeCyclePolicy = PoolLifeCyclePolicy.DestroyOnLoad) where T : Enum
        {
            EnsureInitialized();

            var key = EnumIdentifier.Wrap(id);
            
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
                if (!SceneRecycleGuard.Instance)
                {
                    CreateSceneRecycleGuard();
                }
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
                MinimumObjectCount = minimumObjectCount,
                ComponentTypes = recyclableObject.Components.Select(x => x.GetType()).ToArray()
            };
            
            contexts.Add(key, context);
            
            context.Prepare(minimumObjectCount);
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
            var key = EnumIdentifier.Wrap(prefab);
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
            var key = EnumIdentifier.Wrap(prefab);
            contexts[key].RecycleAllObjects();
        }
    }
}
