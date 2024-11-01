﻿using System;

namespace Milutools.Milutools.General
{
    public struct EnumIdentifier
    {
        public int Value;
        public Type Type;

        public string Name { get; private set; }

        public static EnumIdentifier Wrap<T>(T identifier) where T : Enum
        {
            return new EnumIdentifier()
            {
                Value = (int)(object)identifier,
                Type = typeof(T),
                Name = typeof(T).FullName + "." + identifier
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
