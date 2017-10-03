using System.Data;
using FluentAssertions.Execution;
using KellermanSoftware.CompareNetObjects;

namespace EF.BulkInsert.Cascade.Tests
{
    public static class DataTableExtension
    {
        public static void ShouldBeSame(this DataTable t1, DataTable t2)
        {
            var comparisonResult = new CompareLogic().Compare(t1, t2);
            if (!comparisonResult.AreEqual)
            {
                throw new AssertionFailedException(comparisonResult.DifferencesString);
            }
        }
    }
}