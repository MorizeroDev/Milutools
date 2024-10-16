﻿using System;
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

        public static UI Managed<T>(T identifier, GameObject prefab) where T : Enum
        {
            return new UI()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Prefab = prefab
            };
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