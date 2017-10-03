using Altairis.Samples.Data.Northwind.CodeFirst;
using EntityFramework.Metadata.Extensions;
using Xunit;

namespace EF.BulkInsert.Cascade.Tests
{
    public class BulkInsertTest
    {
        public BulkInsertTest()
        {
        }
        
        [Fact]
        public void StuctualTest()
        {
            using (var context = new NorthwindDbContext())
            {
                var propertyMaps = context.Db<Supplier>().Properties;
//                var columns = propertyMaps.Select(o => new
//                {
//                    o.ColumnName,
//                    GetValue = o.IsDiscriminator
//                        ? x => typeof(T).Name
//                        : ExpressHelper.GetPropGetter<T>(o.PropertyName).Compile()
//                }).ToArray();
            }
        }
    }

}