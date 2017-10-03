namespace EF.BulkInsert.Cascade.Tests.TestContext
{
    public abstract class InheritanceBase
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class InheritanceA : InheritanceBase
    {
        public string ColumnA { get; set; }
    }

    public class InheritanceAa : InheritanceA
    {
        public string ColumnAa { get; set; }
    }


    public class InheritanceB : InheritanceBase
    {
    }
}