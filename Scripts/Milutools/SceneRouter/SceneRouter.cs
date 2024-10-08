using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milutools.Logger;
using Milutools.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.SceneRouter
{
    public class SceneRouter : MonoBehaviour
    {
        private const char PathSeparator = '/';
        
        internal static bool Enabled = false;
        
        internal readonly static Dictionary<SceneRouterIdentifier, SceneRouterNode> Nodes = new();
        internal static SceneRouterNode RootNode, CurrentNode;

        internal static GameObject LoadingAnimatorPrefab;
        
        [RuntimeInitializeOnLoadMethod]
        public static void Setup()
        {
            LoadingAnimatorPrefab = Resources.Load<GameObject>("BlackFade");
            
            var method = ReflectionUtils.GetFirstStaticMethod<SceneRouterConfigAttribute>();
            if (method == null)
            {
                DebugLog.LogWarning("No method has the 'SceneRouterConfig' attribute, the scene router will not be enabled.");
                return;
            }

            if (method.ReturnType != typeof(SceneRouterNode[]))
            {
                DebugLog.LogError("Scene Router config method must return a 'SceneRouterNode[]' value.");
                return;
            }

            foreach (var node in (SceneRouterNode[])method.Invoke(null, null))
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
            
            Enabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ValidateLoadingPrefab(GameObject prefab)
        {
            var result = prefab.TryGetComponent<LoadingAnimator>(out _);
            if (!result)
            {
                DebugLog.LogError("The loading prefab must has a LoadingAnimator component on its root object.");
            }
            return result;
        }
        
        public static void SetLoadingAnimator(GameObject prefab)
        {
            if (!ValidateLoadingPrefab(prefab))
            {
                return;
            }

            LoadingAnimatorPrefab = prefab;
        }
        
        private static SceneRouterNode Node<T>(T identifier, string path, string scene, bool isRoot) where T : Enum
        {
            return new SceneRouterNode()
            {
                Identifier = SceneRouterIdentifier.Wrap(identifier),
                Path = path.Split(PathSeparator),
                Scene = scene,
                IsRoot = isRoot
            };
        }

        public static SceneRouterNode Root<T>(T identifier, string scene) where T : Enum
            => Node(identifier, "", scene, true);
        
        public static SceneRouterNode Node<T>(T identifier, string path, string scene) where T : Enum
            => Node(identifier, path, scene, false);

        private static void GoTo(SceneRouterNode node, GameObject loadingPrefab = null)
        {
            var prefab = loadingPrefab ?? LoadingAnimatorPrefab;
            var go = Instantiate(prefab);
            var animator = go.GetComponent<LoadingAnimator>();
            animator.TargetScene = node.Scene;
            go.SetActive(true);

            CurrentNode = node;
        }
        
        public static void GoTo<T>(T scene, GameObject loadingPrefab = null) where T : Enum
        {
            var key = SceneRouterIdentifier.Wrap(scene);
            if (!Nodes.ContainsKey(key))
            {
                DebugLog.LogError($"The specific scene node '{key}' is not found.");
            }
            
            GoTo(Nodes[key], loadingPrefab);
        }

        public static void Back(GameObject loadingPrefab = null)
        {
            if (CurrentNode.Path.Length < 2)
            {
                GoTo(RootNode, loadingPrefab);
                return;
            }
            var path = CurrentNode.Path[..^2];
            var node = Nodes.Values.FirstOrDefault(x => x.Path.Equals(path));
            if (node == null)
            {
                DebugLog.LogWarning($"The parent node of scene node '{CurrentNode}' is not configured, the router will navigate to the root node.");
                node = RootNode;
            }
            GoTo(node, loadingPrefab);
        }
    }
}
