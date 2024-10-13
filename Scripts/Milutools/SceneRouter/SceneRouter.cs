using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.SceneRouter
{
    public class SceneRouter : MonoBehaviour
    {
        private const char PathSeparator = '/';
        
        internal static bool Enabled = false;
        
        internal readonly static Dictionary<EnumIdentifier, SceneRouterNode> Nodes = new();
        internal readonly static Dictionary<EnumIdentifier, LoadingAnimatorData> LoadingAnimators = new();
        
        internal static SceneRouterNode RootNode, CurrentNode;

        internal static LoadingAnimatorData LoadingAnimatorPrefab;
        internal static object Parameters;

        /// <summary>
        /// When calling the Back() method at the root node, should the game exit directly?
        /// </summary>
        public static bool QuitOnRootNode { get; set; } = true;
        
        public static void Setup(SceneRouterConfig config)
        {
            if (Enabled)
            {
                DebugLog.LogError("Duplicated setup is not allowed.");
                return;
            }
            
            LoadingAnimatorPrefab = new LoadingAnimatorData()
            {
                Prefab = Resources.Load<GameObject>("BlackFade")
            };

            foreach (var node in config.SceneNodes)
            {
                if (node.IsRoot)
                {
                    if (RootNode != null)
                    {
                        DebugLog.LogError("There should be only an root node.");
                        return;
                    }

                    RootNode = node;
                }
                Nodes.Add(node.Identifier, node);
            }

            CurrentNode = config.SceneNodes.FirstOrDefault(x => x.Scene == SceneManager.GetActiveScene().name);
            if (CurrentNode == null)
            {
                DebugLog.LogWarning($"Current scene '{SceneManager.GetActiveScene().name}' is not included in this scene notes, " +
                                    $"SceneRouter.Back() won't work properly in this scene.");
            }

            foreach (var animator in config.LoadingAnimators)
            {
                LoadingAnimators.Add(animator.Identifier, animator);
            }
            
#if UNITY_EDITOR
            SceneManager.activeSceneChanged += (_, scene) =>
            {
                if (CurrentNode != null && CurrentNode.Scene == scene.name)
                {
                    return;
                }
                DebugLog.LogError("You seem to be trying to manage the scenes on your own, " +
                                  "but this might hinder the normal functioning of the scene router, " +
                                  "and it doesn't align with the design principles.");
            };
#endif
            
            Enabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ValidateLoadingPrefab(GameObject prefab)
        {
            var result = prefab.TryGetComponent<LoadingAnimator>(out _);
            if (!result)
            {
                DebugLog.LogError($"The loading prefab '{prefab.name}' must has a LoadingAnimator component on its root object.");
            }
            return result;
        }

        public static T FetchParameters<T>()
            => (T)Parameters;
        
        public static void SetLoadingAnimator<T>(T animator) where T : Enum
        {
            var key = EnumIdentifier.Wrap(animator);
            if (!LoadingAnimators.TryGetValue(key, out var loadingAnimator))
            {
                DebugLog.LogError($"Specific loading animator '{key}' is not configured.");
                return;
            }

            LoadingAnimatorPrefab = loadingAnimator;
        }
        
        public static LoadingAnimatorData GetLoadingAnimator<T>(T animator) where T : Enum
        {
            var key = EnumIdentifier.Wrap(animator);
            if (!LoadingAnimators.TryGetValue(key, out var loadingAnimator))
            {
                DebugLog.LogError($"Specific loading animator '{key}' is not configured.");
                return null;
            }

            return loadingAnimator;
        }

        public static LoadingAnimatorData LoadingAnimator<T>(T identifier, GameObject prefab) where T : Enum
        {
            return new LoadingAnimatorData()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Prefab = prefab
            };
        }
        
        private static SceneRouterNode Node<T>(T identifier, string path, string scene, bool isRoot) where T : Enum
        {
            return new SceneRouterNode()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Path = path.Split(PathSeparator),
                FullPath = path,
                Scene = scene,
                IsRoot = isRoot
            };
        }

        public static SceneRouterNode Root<T>(T identifier, string scene) where T : Enum
            => Node(identifier, "", scene, true);
        
        public static SceneRouterNode Node<T>(T identifier, string path, string scene) where T : Enum
            => Node(identifier, path, scene, false);

        private static SceneRouterContext GoTo(SceneRouterNode node, LoadingAnimatorData loadingAnimator = null)
        {
            if (!Enabled)
            {
                DebugLog.LogError("Scene router is not enabled, please configure the scene nodes first.");
                return null;
            }
            var data = loadingAnimator ?? LoadingAnimatorPrefab;
            var go = Instantiate(data.Prefab);
            var animator = go.GetComponent<LoadingAnimator>();
            animator.TargetScene = node.Scene;
            go.SetActive(true);

            Parameters = null;
            CurrentNode = node;

            return new SceneRouterContext();
        }
        
        public static SceneRouterContext GoTo<T>(T scene, LoadingAnimatorData loadingAnimator = null) where T : Enum 
        {
            var key = EnumIdentifier.Wrap(scene);
            if (!Nodes.ContainsKey(key))
            {
                DebugLog.LogError($"The specific scene node '{key}' is not found.");
            }
            
            return GoTo(Nodes[key], loadingAnimator);
        }

        public static SceneRouterContext Back(LoadingAnimatorData loadingAnimator = null)
        {
            if (CurrentNode.Path.Length < 2)
            {
                if (QuitOnRootNode && CurrentNode == RootNode)
                {
                    DebugLog.LogWarning("At this point, the compiled game will terminate the process.");
                    Application.Quit();
                    return null;
                }
                return GoTo(RootNode, loadingAnimator);
            }
            var path = string.Join(PathSeparator, CurrentNode.Path[..^1]);
            var node = Nodes.Values.FirstOrDefault(x => x.FullPath == path);
            if (node == null)
            {
                DebugLog.LogWarning($"The parent node of scene node '{CurrentNode.Identifier}' is not configured, the router will navigate to the root node.");
                node = RootNode;
            }
            return GoTo(node, loadingAnimator);
        }
    }
}
