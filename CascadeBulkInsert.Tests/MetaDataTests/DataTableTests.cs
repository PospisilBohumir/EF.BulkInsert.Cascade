using System.Data;
using EF.BulkInsert.Cascade.Tests.TestContext;
using Xunit;

namespace EF.BulkInsert.Cascade.Tests.MetaDataTests
{
    [Collection(nameof(DbContextCollection))]
    public class DataTableTests
    {
        private readonly TableNameTestContext _context;

        public DataTableTests(TableNameTestContext context)
        {
            _context = context;
        }



        [Fact]
        public void StuctualTest()
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn(nameof(BasicEntity.Id),typeof(long)),
                new DataColumn(nameof(BasicEntity.Name),typeof(string)),
            });
            var dataRow = dataTable.Rows.Add();
            dataRow[nameof(BasicEntity.Id)] = 1;
            dataRow[nameof(BasicEntity.Name)] = "Test";
            _context.GetDataReader(new[]{new BasicEntity {Id = 1,Name = "Test"}, }).ShouldBeSame(dataTable);
        }
/*
        [Fact]
        public void BasicInheritanceTest()
        {
            var columnInfos = _context.GetColumns<InheritanceA>().ToArray();
            columnInfos.ShouldAllBeEquivalentTo(
                new IColumnInfo<InheritanceA>[]
                {
                    new ColumnInfo<InheritanceA>(nameof(InheritanceA.Id)),
                    new ColumnInfo<InheritanceA>(nameof(InheritanceA.Name)),
                    new ColumnInfo<InheritanceA>(nameof(InheritanceA.ColumnA)),
                    new ColumnInfoDescriptor<InheritanceA>(),
                });
        }

        [Fact]
        public void InheritanceTest()
        {
            _context.GetColumns<InheritanceB>().ShouldAllBeEquivalentTo(
                new IColumnInfo<InheritanceB>[]
                {
                    new ColumnInfo<InheritanceB>(nameof(InheritanceB.Id)),
                    new ColumnInfo<InheritanceB>(nameof(InheritanceB.Name)),
                });
        }


        [Fact]
        public void Inheritance2Test()
        {
            var columnInfos = _context.GetColumns<InheritanceAa>().ToArray();
            columnInfos.ShouldAllBeEquivalentTo(
                new IColumnInfo<InheritanceAa>[]
                {
                    new ColumnInfo<InheritanceAa>(nameof(InheritanceAa.Id)),
                    new ColumnInfo<InheritanceAa>(nameof(InheritanceAa.Name)),
                    new ColumnInfo<InheritanceAa>(nameof(InheritanceAa.ColumnA)),
                    new ColumnInfo<InheritanceAa>(nameof(InheritanceAa.ColumnAa)),
                    new ColumnInfoDescriptor<InheritanceA>(),
                });
        }
*/

    }

}