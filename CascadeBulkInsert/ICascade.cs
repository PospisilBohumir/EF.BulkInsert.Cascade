using System.Collections.Generic;
using System.Data.Entity;

namespace EF.BulkInsert.Cascade
{
    public interface ICascade<in T>
    {
        bool IsCascadeBeforeInsert { get; } 

        void InnerInsert(IEnumerable<T> source, DbContext context, DbContextTransaction transaction);
    }
}