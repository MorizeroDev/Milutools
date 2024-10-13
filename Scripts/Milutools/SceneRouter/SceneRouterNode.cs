using System;
using Milutools.Milutools.General;
using UnityEngine;

namespace Milutools.SceneRouter
{
    public class SceneRouterNode
    {
        internal EnumIdentifier Identifier;
        internal string[] Path;
        internal string FullPath;
        internal string Scene;
        internal bool IsRoot;
        
        internal SceneRouterNode()
        {
            
        }
    }
}
