using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EF.BulkInsert.Cascade.Helpers;
using EntityFramework.Metadata;
using EntityFramework.Metadata.Extensions;

namespace EF.BulkInsert.Cascade
{
    public static class ContextExtender
    {
        private const int BulkCopyTimeout = 10 * 60;

        /// <summary>
        ///     retrieve Ids for every Entity from <paramref name="entities" />
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database Context</param>
        /// <param name="entities">List of entities which should be inserted into database</param>
        public static void RetrieveIds<TEntity>(this DbContext context, TEntity[] entities)
            where TEntity : class
        {
            var tableName = context.Db<TEntity>().TableName;
            var entitiesLength = entities.Length;
            var minId = context.Database.SqlQuery<long>(
                            $@"
declare
     @Maxid bigint;
begin
	select @Maxid=IDENT_CURRENT ('{tableName}');
	set @Maxid =@Maxid+{entitiesLength};
	DBCC CHECKIDENT ('{tableName}',Reseed,@Maxid);
	select @Maxid;
end;
").First() - entitiesLength;
            var pkSetter = context.GetPkSetter<TEntity>();
            foreach (var entity in entities)
            {
                pkSetter(entity, ++minId);
            }
        }

        /// <summary>
        ///     Inserts <paramref name="entities" /> in Bulk operation
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="context">Database Context</param>
        /// <param name="transaction">Open transaction</param>
        /// <param name="entities">List of entities which should be inserted into database</param>
        /// <param name="keepIdentity">True, when id should be inserted ,false when id should be generated</param>
        public static void BulkInsert<T>(this DbContext context, DbContextTransaction transaction, IEnumerable<T> entities,
            bool keepIdentity = false)
        {
            var underlyingTransaction = (SqlTransaction)transaction.UnderlyingTransaction;

            using (var sqlBulkCopy = new SqlBulkCopy(underlyingTransaction.Connection,
                keepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default, underlyingTransaction))
            {
                sqlBulkCopy.BulkCopyTimeout = BulkCopyTimeout;
                sqlBulkCopy.DestinationTableName = context.Db<T>().TableName;
                var table = context.GetDataReader(entities);
                foreach (var column in table.Columns.OfType<DataColumn>().Select(o => o.ColumnName))
                {
                    sqlBulkCopy.ColumnMappings.Add(column, column);
                }
                sqlBulkCopy.WriteToServer(table);
            }
        }

        public static DataTable GetDataReader<T>(this DbContext context, IEnumerable<T> entities)
        {
            var propertyMaps = context.Db<T>().Properties;
            var table = propertyMaps.CreateTable();
            var columns = propertyMaps.Select(o => new
            {
                o.ColumnName,
                GetValue = o.IsDiscriminator
                    ? x => typeof(T).Name
                    : ExpressHelper.GetPropGetter<T>(o.PropertyName).Compile()
            }).ToArray();
            foreach (var entity in entities)
            {
                var dataRow = table.Rows.Add();
                foreach (var column in columns)
                {
                    dataRow[column.ColumnName] = column.GetValue(entity) ?? DBNull.Value;
                }
            }
            return table;
        }

        private static DataTable CreateTable(this IEnumerable<IPropertyMap> columns)
        {
            var result = new DataTable();
            foreach (var column in columns)
            {
                var propertyType = column.Type;
                result.Columns.Add(column.PropertyName,
                    propertyType.IsNullable() ? propertyType.GetGenericArguments().First() : propertyType);
            }
            return result;
        }

        public static void BulkInsertWithIdGeneration<T>(this DbContext context, DbContextTransaction transaction, IList<T> forSave)
            where T : class
        {
            if (forSave == null)
            {
                return;
            }
            var idGetter = context.GetPkGetter<T>();
            Func<T, bool> isNew = o => idGetter(o) == default(long);
            var newObjects = forSave.Where(isNew).ToArray();
            if (!newObjects.Any())
            {
                return;
            }
            context.RetrieveIds(newObjects);
            context.BulkInsert(transaction, newObjects, true);
        }

        public static void BulkInsertCascade<T>(this DbContext context, DbContextTransaction transaction, IList<T> forSave,
            params ICascade<T>[] cascades) where T : class
        {
            if (forSave == null || !forSave.Any())
            {
                return;
            }
            foreach (var cascade in cascades.Where(o => o.IsCascadeBeforeInsert))
            {
                cascade.InnerInsert(forSave, context, transaction);
            }

            if (cascades.Any())
            {
                context.BulkInsertWithIdGeneration(transaction, forSave);
            }
            else
            {
                context.BulkInsertWithIdGeneration(transaction, forSave);
            }
            foreach (var cascade in cascades.Where(o => !o.IsCascadeBeforeInsert))
            {
                cascade.InnerInsert(forSave, context, transaction);
            }
        }
    }
}