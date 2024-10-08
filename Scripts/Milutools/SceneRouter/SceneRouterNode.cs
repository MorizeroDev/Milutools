using System;
using UnityEngine;

namespace Milutools.SceneRouter
{
    public class SceneRouterNode
    {
        internal SceneRouterIdentifier Identifier;
        internal string[] Path;
        internal string Scene;
        internal bool IsRoot;
        
        internal SceneRouterNode()
        {
            
        }
    }
}
