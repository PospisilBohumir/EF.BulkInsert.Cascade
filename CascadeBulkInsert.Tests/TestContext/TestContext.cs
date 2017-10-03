using System.Data.Entity;

namespace EF.BulkInsert.Cascade.Tests.TestContext
{
    public class TableNameTestContext : DbContext
    {
        public TableNameTestContext()
        {
            Database.SetInitializer<DbContext>(null);
        }

        public IDbSet<BasicEntity> BasicEntities { get; set; }
        public IDbSet<TableNameSpecifiedEntity> TableNameSpecifiedEntities { get; set; }

        public IDbSet<InheritanceA> InheritanceAs { get; set; }

        public IDbSet<InheritanceB> InheritanceBs { get; set; }
    }
}