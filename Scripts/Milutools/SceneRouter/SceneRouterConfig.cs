using System;
using System.Collections.Generic;
using UnityEngine;

namespace Milutools.SceneRouter
{
    public class SceneRouterConfig
    {
        public IEnumerable<SceneRouterNode> SceneNodes = Array.Empty<SceneRouterNode>();
        public IEnumerable<LoadingAnimatorData> LoadingAnimators = Array.Empty<LoadingAnimatorData>();
    }
}
