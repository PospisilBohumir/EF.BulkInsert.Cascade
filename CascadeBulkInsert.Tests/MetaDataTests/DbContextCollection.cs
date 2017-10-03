using EF.BulkInsert.Cascade.Tests.TestContext;
using Xunit;

namespace EF.BulkInsert.Cascade.Tests.MetaDataTests
{
    [CollectionDefinition(nameof(DbContextCollection))]
    public class DbContextCollection : ICollectionFixture<TableNameTestContext>
    {
    }
}