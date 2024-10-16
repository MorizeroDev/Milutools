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
        internal Type TypeDefinition;
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

            var type = ui.GetType();
            while (type != null && type.BaseType != null)
            {
                if (type.IsGenericType && type.BaseType == typeof(ManagedUI))
                {
                    break;
                }
                type = type.BaseType;
            }
            
            var data = new UI()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Prefab = prefab,
                TypeDefinition = type
            };
            prefab.SetActive(false);

            var genericType = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();
            
            if (genericType == typeof(ManagedUI<,>))
            {
                data.ParameterType = args[1];
            }
            else if (genericType == typeof(ManagedUI<,,>))
            {
                data.ParameterType = args[1];
                data.ReturnValueType = args[2];
            }
            else if (genericType == typeof(ManagedUIReturnValueOnly<,>))
            {
                data.ReturnValueType = args[1];
            }
            
            return data;
        }

        public UI SingletonMode()
        {
            Mode = UIMode.Singleton;
            return this;
        }
    }
}
