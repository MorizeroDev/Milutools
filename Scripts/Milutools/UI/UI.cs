using System;
using Milutools.Milutools.General;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Milutools.Milutools.UI
{
    public enum UIMode
    {
        Default, Singleton
    }
    
    public class UI
    {
        internal EnumIdentifier Identifier;
        internal Type ParameterType = null;
        internal Type ReturnValueType = null;
        internal GameObject Prefab;
        internal UIMode Mode = UIMode.Default;

        internal GameObject Instance;

        internal UI()
        {
            
        }

        internal GameObject Create()
        {
            if (Mode == UIMode.Singleton)
            {
                if (!Instance)
                {
                    Instance = Object.Instantiate(Prefab);
                }
                Object.DontDestroyOnLoad(Instance);
                return Instance;
            }

            return Object.Instantiate(Prefab);
        }

        public static UI FromResources<T>(T identifier, string prefabPath) where T : Enum
            => FromPrefab(identifier, Resources.Load<GameObject>(prefabPath));
        
        public static UI FromPrefab<T>(T identifier, GameObject prefab) where T : Enum
        {
            if (!prefab.TryGetComponent<ManagedUI>(out var ui))
            {
                throw new Exception($"UI '{identifier}' must have a ManagedUI component.");
            }

            ui.Source = new UI()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Prefab = prefab
            };
            prefab.SetActive(false);
            
            return ui.Source;
        }

        public UI SingletonMode()
        {
            Mode = UIMode.Singleton;
            return this;
        }
        
        public UI SetParameterType<T>()
        {
            ParameterType = typeof(T);
            return this;
        }

        public UI SetReturnValueType<T>()
        {
            ReturnValueType = typeof(T);
            return this;
        }
    }
}
