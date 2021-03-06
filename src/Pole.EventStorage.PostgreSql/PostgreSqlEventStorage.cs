﻿using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Npgsql;
using Pole.Core;
using Pole.EventBus;
using Pole.EventBus.EventStorage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pole.EventStorage.PostgreSql
{
    class PostgreSqlEventStorage : IEventStorage
    {
        private readonly string tableName;
        private readonly PoleEventBusOption producerOptions;
        private readonly PostgreSqlOptions options;
        private readonly IEventStorageInitializer eventStorageInitializer;
        public PostgreSqlEventStorage(IOptions<PostgreSqlOptions> postgreSqlOptions, IOptions<PoleEventBusOption> producerOptions, IEventStorageInitializer eventStorageInitializer)
        {
            this.producerOptions = producerOptions.Value;
            this.options = postgreSqlOptions.Value;
            this.eventStorageInitializer = eventStorageInitializer;
            tableName = eventStorageInitializer.GetTableName();
        }

        public async Task ChangePublishStateAsync(IEnumerable<EventEntity> events)
        {
            var sql =
$"UPDATE {tableName} SET \"Retries\"=@Retries,\"ExpiresAt\"=@ExpiresAt,\"StatusName\"=@StatusName WHERE \"Id\" = @Id";
            using (var connection = new NpgsqlConnection(options.ConnectionString))
            {
                var result = await connection.ExecuteAsync(sql, events.Select(@event => new
                {
                    Id = @event.Id,
                    @event.Retries,
                    @event.ExpiresAt,
                    @event.StatusName
                }).ToList());
            }
        }
        public async Task BulkChangePublishStateAsync(IEnumerable<EventEntity> events)
        {
            using (var connection = new NpgsqlConnection(options.ConnectionString))
            {
                var uploader = new PoleNpgsqlBulkUploader(connection);
                await uploader.UpdateAsync(tableName, events);
            }
        }

        public async Task ChangePublishStateAsync(EventEntity message, EventStatus state)
        {
            var sql =
    $"UPDATE {tableName} SET \"Retries\"=@Retries,\"ExpiresAt\"=@ExpiresAt,\"StatusName\"=@StatusName WHERE \"Id\"=@Id";
            using var connection = new NpgsqlConnection(options.ConnectionString);
            await connection.ExecuteAsync(sql, new
            {
                Id = long.Parse(message.Id),
                message.Retries,
                message.ExpiresAt,
                StatusName = state.ToString("G")
            });
        }

        public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000, CancellationToken token = default)
        {
            using (var connection = new NpgsqlConnection(options.ConnectionString))
            {
                var result = await connection.ExecuteAsync(
    $"DELETE FROM {table} WHERE   \"Id\" IN (SELECT \"Id\" FROM {table} WHERE \"ExpiresAt\" < @timeout LIMIT @batchCount);",
    new { timeout, batchCount });
                return result;
            }
        }

        public async Task<IEnumerable<EventEntity>> GetEventsOfNeedRetry()
        {
            var fourMinAgo = DateTime.UtcNow.AddMinutes(-4).ToString("O");
            var sql =
                $"SELECT * FROM {tableName} WHERE \"Retries\"<{producerOptions.MaxFailedRetryCount} AND \"Added\"<'{fourMinAgo}' AND  \"StatusName\"='{EventStatus.Pending}' for update skip locked LIMIT 200;";

            using (var connection = new NpgsqlConnection(options.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var result = await connection.QueryAsync<EventEntity>(sql,null, transaction);
  
                    await transaction.CommitAsync();
                    return result;
                }
            }
        }

        public async Task<bool> StoreEvent(EventEntity eventEntity, object dbTransaction = null)
        {
            var sql =
                $"INSERT INTO {tableName} (\"Id\",\"Version\",\"Name\",\"Content\",\"Retries\",\"Added\",\"ExpiresAt\",\"StatusName\")" +
                $"VALUES(@Id,'1',@Name,@Content,@Retries,@Added,@ExpiresAt,@StatusName);";

            if (dbTransaction == null)
            {
                using var connection = new NpgsqlConnection(options.ConnectionString);
                return await connection.ExecuteAsync(sql, eventEntity) > 0;
            }
            else
            {
                var dbTrans = dbTransaction as IDbTransaction;
                if (dbTrans == null && dbTransaction is IDbContextTransaction dbContextTrans)
                    dbTrans = dbContextTrans.GetDbTransaction();

                var conn = dbTrans?.Connection;
                return await conn.ExecuteAsync(sql, eventEntity, dbTrans) > 0;
            }
        }

        public async Task<int> GetFailedEventsCount()
        {
            using (var connection = new NpgsqlConnection(options.ConnectionString))
            {
                var count = await connection.ExecuteScalarAsync<int>(
    $"select count(1) FROM {tableName} WHERE   \"StatusName\" = '{nameof(EventStatus.Failed)}'");
                return count;
            }
        }
    }
}
