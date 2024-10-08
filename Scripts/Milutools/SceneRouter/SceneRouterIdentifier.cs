using System;

namespace Milutools.SceneRouter
{
    internal struct SceneRouterIdentifier
    {
        public int Value;
        public Type Type;

        public string Name { get; private set; }

        internal static SceneRouterIdentifier Wrap<T>(T identifier) where T : Enum
        {
            return new SceneRouterIdentifier()
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
