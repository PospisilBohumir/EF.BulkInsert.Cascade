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
    public class Cascade<TSource, TDestination> : ICascade<TSource> where TDestination : class
    {
        private readonly ICascade<TDestination>[] _cascades;
        private readonly Expression<Func<TSource, IEnumerable<TDestination>>> _extractor;
        private readonly string _propertyName;

        public Cascade([NotNull] Expression<Func<TSource, IEnumerable<TDestination>>> extractor,
            [NotNull] Expression<Func<TDestination, long>> idGetter,
            params ICascade<TDestination>[] cascades)
            : this(extractor, ExpressHelper.GetPath(idGetter), cascades)
        {
        }

        public Cascade([NotNull] Expression<Func<TSource, IEnumerable<TDestination>>> extractor,
            [NotNull] Expression<Func<TDestination, long?>> idGetter,
            params ICascade<TDestination>[] cascades)
            : this(extractor, ExpressHelper.GetPath(idGetter), cascades)
        {
        }

        public Cascade([NotNull] Expression<Func<TSource, IEnumerable<TDestination>>> extractor,
            params ICascade<TDestination>[] cascades)
            : this(extractor, (string) null, cascades)
        {
        }


        private Cascade(
            [NotNull] Expression<Func<TSource, IEnumerable<TDestination>>> extractor,
            string propertyName,
            params ICascade<TDestination>[] cascades)
        {
            _extractor = extractor;
            _propertyName = propertyName;
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
            var fkSetter = GetSetter(context);
            var idGetter = context.GetPkGetter<TSource>();
            foreach (var parent in s)
            {
                foreach (var child in extractor(parent))
                {
                    fkSetter.Invoke(child, idGetter(parent));
                }
            }
            context.BulkInsertCascade(transaction, s.SelectMany(extractor).Distinct().ToArray(), _cascades);
        }

        private Action<TDestination, object> GetSetter(DbContext context)
        {
            var propertyName = _propertyName ?? context.Db<TDestination>()
                                   .Properties.Single(o => o.NavigationProperty?.Type == typeof(TSource))
                                   .PropertyName;
            return ExpressHelper.GetPropSetter<TDestination, object>(propertyName).Compile();
        }

        public bool IsCascadeBeforeInsert => false;

        protected bool Equals(Cascade<TSource, TDestination> other)
        {
            return Equals(_extractor, other._extractor) && string.Equals(_propertyName, other._propertyName) &&
                   Equals(_cascades, other._cascades);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Cascade<TSource, TDestination>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _extractor?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (_propertyName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_cascades?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}