using System;

namespace Milutools.Recycle
{
    internal struct RecycleKey
    {
        public Type EnumType;
        public object Value;
        public override string ToString()
        {
            return $"{EnumType.FullName}.{Value}";
        }
    }
}
