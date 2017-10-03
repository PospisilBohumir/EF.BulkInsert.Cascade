using System.ComponentModel.DataAnnotations.Schema;

namespace EF.BulkInsert.Cascade.Tests.TestContext
{
    [Table("TestTableName")]
    public class TableNameSpecifiedEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ComplexType ComplexType { get; set; }
    }

    [ComplexType]
    public class ComplexType
    {
        public string ComplexColumn { get; set; }
    }
}