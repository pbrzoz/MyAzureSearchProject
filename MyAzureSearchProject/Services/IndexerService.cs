using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace MyAzureSearchProject.Services
{
    public class IndexerService
    {
        private readonly SearchIndexerClient _indexerClient;

        public IndexerService(SearchIndexerClient indexerClient)
        {
            _indexerClient = indexerClient;
        }

        public async Task Create(string name, string dataSourceName, string targetIndexName, string[] mappings)
        {
            SearchIndexer indexer = new SearchIndexer(name, dataSourceName, targetIndexName)
            {
                FieldMappings =
                {
                    new FieldMapping(mappings[0]),
                    new FieldMapping(mappings[1]),
                    new FieldMapping(mappings[2]),
                }
            };

            await _indexerClient.CreateIndexerAsync(indexer);
        }

        public async Task Delete(string name)
        {
            await _indexerClient.DeleteIndexerAsync(name);
        }
    }
}
