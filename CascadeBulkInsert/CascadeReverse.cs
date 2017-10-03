using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EF.BulkInsert.Cascade.Helpers;
using EntityFramework.Metadata.Extensions;
using JetBrains.Annotations;

namespace EF.BulkInsert.Cascade
{
    public class CascadeReverse<TSource, TDestination> : ICascade<TSource> where TDestination : class
    {
        [NotNull]
        private readonly ICascade<TDestination>[] _cascades;
        [NotNull]
        private readonly Expression<Func<TSource, TDestination>> _extractor;

        private CascadeReverse([NotNull] Expression<Func<TSource, TDestination>> extractor,[NotNull] params ICascade<TDestination>[] cascades)
        {
            _extractor = extractor;
            _cascades = cascades;
        }

        public void InnerInsert(IEnumerable<TSource> source, DbContext context, DbContextTransaction transaction)
        {
            if (source == null)
            {
                return;
            }
            var extractor = _extractor.Compile();
            var s = source.Where(o => extractor(o) != null).ToArray();
            if (!s.Any())
            {
                return;
            }
            var pkGetter = context.GetPkGetter<TDestination>();
            var destinations = s.Select(extractor).Distinct().ToArray();
            context.BulkInsertCascade(transaction, destinations, _cascades);
            var fkSetter = GetSetter(context);
            foreach (var destination in s)
            {
                var parrentId = pkGetter(extractor(destination));
                fkSetter.Invoke(destination, parrentId);
            }
        }

        public bool IsCascadeBeforeInsert => true;

        private Action<TSource, object> GetSetter(DbContext context)
        {
            var path = ExpressHelper.GetPath(_extractor);
            var propertyName = context.Db<TSource>().Properties.Single(o => o.NavigationProperty?.PropertyName == path).PropertyName;
            return ExpressHelper.GetPropSetter<TSource, object>(propertyName).Compile();
        }
    }
}