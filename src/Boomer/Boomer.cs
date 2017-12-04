using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Boomer
{
    public class Boomer
    {
        public List<string> Tables { get; set; }
        private IMemoryCache _cache;

        public Boomer()
        {
            var cacheOptions = new MemoryCacheOptions()
            {

            };
            _cache = new MemoryCache(cacheOptions);
        }

        public async Task Delete(AmazonDynamoDBClient client)
        {
            foreach (var tableName in Tables)
            {
                Table table = null;
                Table.TryLoadTable(client, tableName, out table);
                var scan = table.Scan(new ScanFilter());
                var data = scan.GetRemainingAsync().Result;
                var batchDelete = table.CreateBatchWrite();

                foreach (var item in data)
                {
                    batchDelete.AddItemToDelete(item);
                }
                await batchDelete.ExecuteAsync();
            }
        }

        public async Task Restore(AmazonDynamoDBClient client)
        {
            foreach (var tableName in Tables)
            {
                Table table = null;
                Table.TryLoadTable(client, tableName, out table);
                var dataToRestore = _cache.Get(tableName) as List<Document>;
                var batchWrite = table.CreateBatchWrite();
                if (dataToRestore != null)
                {
                    foreach (var item in dataToRestore)
                    {
                        batchWrite.
                            AddDocumentToPut(item);
                    }
                    await batchWrite.ExecuteAsync();
                }
            }
        }

        public void TakeSnapshot(AmazonDynamoDBClient amazonDynamoDBClient)
        {
            foreach (var tableName in Tables)
            {
                Table table = null;
                Table.TryLoadTable(amazonDynamoDBClient, tableName, out table);
                var scan = table.Scan(new ScanFilter());
                var data = scan.GetRemainingAsync().Result;
                _cache.Set(tableName, data);
            }
        }
    }
}
