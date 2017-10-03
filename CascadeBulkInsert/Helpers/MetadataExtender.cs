using System;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Metadata.Extensions;

namespace EF.BulkInsert.Cascade.Helpers
{
    internal static class MetadataExtender
    {
        public static Action<T,long> GetPkSetter<T>(this DbContext context)
        {
            var primaryKey = context.Db<T>().Pks.Single();
            if (primaryKey.Type != typeof(long))
            {
                throw new NotSupportedException("Just long type primary key are supported");
            }
            return ExpressHelper.GetPropSetter<T,long>(primaryKey.PropertyName).Compile();
        }

        public static Func<T, long> GetPkGetter<T>(this DbContext context)
        {
            var primaryKey = context.Db<T>().Pks.Single();
            if (primaryKey.Type != typeof(long))
            {
                throw new NotSupportedException("Just long type primary key are supported");
            }
            return ExpressHelper.GetPropGetter<T, long>(primaryKey.PropertyName).Compile();
        }
    }
}