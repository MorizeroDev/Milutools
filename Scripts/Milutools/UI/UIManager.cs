using System;
using System.Collections.Generic;
using Milutools.Logger;
using Milutools.Milutools.General;

namespace Milutools.Milutools.UI
{
    public class UIManager
    {
        internal static readonly Dictionary<EnumIdentifier, UI> UIDict = new();
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
    }
}
