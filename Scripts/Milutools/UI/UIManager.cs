using System;
using System.Collections.Generic;
using Milutools.Logger;
using Milutools.Milutools.General;

namespace Milutools.Milutools.UI
{
    public static class UIManager
    {
        internal static readonly Dictionary<EnumIdentifier, UI> UIDict = new();
        internal static readonly Dictionary<Type, UI> UIDictInternal = new();
        
        internal static int CurrentSortingOrder = 1000;
        
        private static bool configured = false;
        
        public static void Setup(IEnumerable<UI> ui)
        {
            if (configured)
            {
                DebugLog.LogError("Duplicated configuring UI manager.");
                return;
            }
            
            foreach (var u in ui)
            {
                UIDict.Add(u.Identifier, u);
                if (u.TypeDefinition == typeof(SimpleManagedUI))
                {
                    continue;
                }

                if (!UIDictInternal.TryAdd(u.TypeDefinition, u))
                {
                    DebugLog.LogError($"Duplicated UI type: {u.TypeDefinition.FullName}");
                }
            }

            configured = true;
        }

        public static UIContext Get<T>(T identifier) where T : Enum
        {
            if (!configured)
            {
                DebugLog.LogError("UI manager has not been setup.");   
                return null;
            }
            
            var key = EnumIdentifier.Wrap(identifier);
            if (!UIDict.ContainsKey(key))
            {
                DebugLog.LogError($"Specific UI '{key}' was not registered, please configure the UI manager first.");
            }

            return new UIContext()
            {
                UI = UIDict[key]
            };
        }
        
        internal static UIContext Get(Type type)
        {
            if (!configured)
            {
                DebugLog.LogError("UI manager has not been setup.");   
                return null;
            }

            if (type == typeof(SimpleManagedUI))
            {
                DebugLog.LogError("Cannot open a SimpleManagedUI by this method.");   
                return null;
            }
            
            if (!UIDictInternal.ContainsKey(type))
            {
                DebugLog.LogError($"Specific UI '{type.FullName}' was not registered, please configure the UI manager first.");
            }

            return new UIContext()
            {
                UI = UIDictInternal[type]
            };
        }
    }
}
