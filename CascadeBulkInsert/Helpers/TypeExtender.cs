using System;

namespace EF.BulkInsert.Cascade.Helpers
{
    internal static class TypeExtender
    {
        internal static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}