using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Minity.ResourceManager.Handlers;
using Minity.ResourceManager.UsageDetector;
using Minity.Infra;
using Minity.Logger;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Minity.ResourceManager
{
    [ExecuteInEditMode]
    public class ResManager : MonoBehaviour
    {
        private static ResManager _instance;

        public static ResManager Instance
        {
            get
            {
                if (!_instance)
                {
                    var go = new GameObject($"[ResManager]", typeof(ResManager));
                    go.SetActive(true);
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                    _instance = go.GetComponent<ResManager>();
                }

                return _instance;
            }
        }
        
        private const int DEFAULT_RES_TIME_OUT = 3000;

        private static bool _initialized = false;
        private static readonly Dictionary<string, Type> _resScheme = new();
        
        private readonly Dictionary<string, ResHandle> _resources = new();
        private readonly List<ResHandle> _trackingRes = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeResSchemes()
        {
            if (_initialized)
            {
                return;
            }
            
            var baseType = typeof(IResHandler);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            var types = assemblies.SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null);
                }
            }).Where(t => baseType.IsAssignableFrom(t)
                          && t != baseType
                          && !t.IsAbstract);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ResHandlerAttribute>();
                if (attribute == null)
                {
                    DebugLog.LogWarning($"'{type.FullName}' does not have ResHandlerAttribute, ResHandler must specify the scheme.");
                    continue;
                }
                _resScheme.Add(attribute.Scheme, type);
            }

            _initialized = true;
        }
        
        private async Task<Object> LoadAsyncInternal<T>(ResHandle handle) where T : Object
        {
            try
            {
                handle.Resource = await handle.Handler.LoadAsync<T>();
                return handle.Resource ?? throw new Exception($"Resource load failed: {handle.UriStr}");
            }
            finally
            {
                lock (handle)
                {
                    handle.LoadTask = null;
                }
            }
        }
        
        /// <summary>
        /// <para><b>We recommend integrating Minity with asset management systems such as Addressables.</b></para>
        /// <para>Please note that the Resources system <b>does not expose any reference counting API.</b>
        /// When you call `Resources.Unload` to unload an asset, Unity will <b>forcibly remove</b> it from memory,
        /// regardless of whether there are still existing references to it.</para>
        /// This means that you <b>should not</b> bind an asset from the Resources folder through Unity’s serialization system
        /// while also attempting to manage its lifecycle with Minity.
        /// </summary>
        public async Task<T> LoadAsync<T>(string uriStr, IUsageDetector detector, int timeOut = DEFAULT_RES_TIME_OUT) where T : Object
        {
            var handle = GetOrRegister(uriStr);

            lock (handle)
            {
                handle.UsageDetector = handle.UsageDetector.CombineDetector(detector);
                handle.LoadTask ??= LoadAsyncInternal<T>(handle);
            }

            if (handle.Resource)
            {
                return handle.GetResource<T>();
            }
            
            var delayTask = Task.Delay(timeOut);
            var finished = await Task.WhenAny(handle.LoadTask, delayTask);

            if (finished == delayTask)
            {
                throw new TimeoutException();
            }

            return handle.GetResource<T>();
        }

        /// <summary>
        /// <para><b>We recommend integrating Minity with asset management systems such as Addressables.</b></para>
        /// <para>Please note that the Resources system <b>does not expose any reference counting API.</b>
        /// When you call `Resources.Unload` to unload an asset, Unity will <b>forcibly remove</b> it from memory,
        /// regardless of whether there are still existing references to it.</para>
        /// This means that you <b>should not</b> bind an asset from the Resources folder through Unity’s serialization system
        /// while also attempting to manage its lifecycle with Minity.
        /// </summary>
        public T Load<T>(string uriStr, IUsageDetector detector, int timeOut = DEFAULT_RES_TIME_OUT) where T : Object
        {
            var handle = GetOrRegister(uriStr);
            lock (handle)
            {
                handle.UsageDetector = handle.UsageDetector.CombineDetector(detector);
            }
            
            if (handle.Resource)
            {
                return handle.GetResource<T>();
            }

            handle.Resource = handle.Handler.Load<T>();
            return handle.GetResource<T>();
        }
        
        private ResHandle GetOrRegister(string uriStr)
        {
            lock (_resources)
            {
                return _resources.TryGetValue(uriStr, out var handle) ? handle : RegisterResource(uriStr);
            }
        }
        
        private ResHandle RegisterResource(string uriStr)
        {
            if (!_initialized)
            {
                InitializeResSchemes();
            }
            
            if (!Uri.TryCreate(uriStr, UriKind.Absolute, out var uri))
            {
                throw new Exception($"Invalid uri '{uri}'");
            }

            if (!_resScheme.TryGetValue(uri.Scheme, out var scheme))
            {
                throw new Exception($"Invalid scheme '{uri.Scheme}'");
            }

            var handle = new ResHandle()
            {
                Handler = (IResHandler)Activator.CreateInstance(scheme),
                Uri = uri,
                UriStr = uriStr,
                UsageDetector = new ComposeUD()
            };
            handle.Handler.Initialize(uri);

            lock (_resources)
            {
                _resources.Add(uriStr, handle);
                _trackingRes.Add(handle);
            }

            return handle;
        }

        private void Awake()
        {
            hideFlags = HideFlags.DontSave;
            if (!Application.isPlaying)
            {
                hideFlags = HideFlags.HideAndDontSave;
            }

            Application.quitting += ReleaseAllResources;
        }

        private void OnDestroy()
        {
            ReleaseAllResources();
        }

        // Exiting play mode directly in the Editor will immediately lose tracking of loaded resources
        // Release everything when exiting play mode
        private void ReleaseAllResources()
        {
            Application.quitting -= ReleaseAllResources;
            lock (_resources)
            {
                foreach (var handle in _trackingRes)
                {
                    if (handle.LoadTask != null)
                    {
                        continue;
                    }
                    handle.Handler.Release();
                }
                _trackingRes.Clear();
                _resources.Clear();
            }
        }

        private void LateUpdate()
        {
            lock (_resources)
            {
                var aliveTill = _trackingRes.Count - 1;
                for (var i = aliveTill; i >= 0; i--)
                {
                    var resHandle = _trackingRes[i];
                    if (resHandle == null || resHandle.LoadTask != null)
                    {
                        continue;
                    }

                    if (resHandle.UsageDetector.IsUsing())
                    {
                        continue;
                    }

                    resHandle.Handler.Release();
                    _resources.Remove(resHandle.UriStr);
                    _trackingRes[i] = null;
                }
                
                for (var i = 0; i <= aliveTill; i++)
                {
                    if (_trackingRes[i] != null)
                    {
                        continue;
                    }
                    
                    (_trackingRes[i], _trackingRes[aliveTill]) = (_trackingRes[aliveTill], _trackingRes[i]);
                    i--;
                    aliveTill--;
                }

                if (aliveTill < _trackingRes.Count - 1)
                {
                    _trackingRes.RemoveRange(aliveTill + 1, _trackingRes.Count - aliveTill - 1);
                }
            }
        }
        
#if UNITY_EDITOR
        public void DrawEditorWindow()
        {
            lock (_resources)
            {
                EditorGUILayout.LabelField("Usages", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"{_trackingRes.Count} resources.");
            
                EditorGUILayout.Space();
                
                foreach (var res in _trackingRes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(res.UriStr, EditorStyles.boldLabel);
                    if (res.UsageDetector is ComposeUD ud)
                    {
                        foreach (var u in ud.GetDetectors())
                        {
                            if (u.IsUsing())
                            {
                                EditorGUILayout.LabelField("- " + u switch
                                {
                                    SceneUD _ => "Release after current scene is unloaded",
                                    ManualUD _ => "Manual release",
                                    ObjectLinkUD olu => $"Using by Object '{(olu.GetLinkObject().name)}'",
                                    TransientAfterReadyUD fu => $"Release after {fu.GetRemainingFrames()} frames",
                                    _ => "Unknown usage"
                                });
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }
#endif
    }
}
