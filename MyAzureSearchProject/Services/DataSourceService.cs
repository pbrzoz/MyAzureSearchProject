using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace MyAzureSearchProject.Services
{
    public class DataSourceService
    {
        private readonly SearchIndexerClient _indexerClient;
        private readonly string _connectionString;

        public DataSourceService(SearchIndexerClient indexerClient, string connectionString)
        {
            _indexerClient = indexerClient;
            _connectionString = connectionString;
        }

        public async Task Create(string connectionName, string tableName)
        {
            SearchIndexerDataSourceConnection dataSourceConnection = new SearchIndexerDataSourceConnection(
                connectionName,
                SearchIndexerDataSourceType.AzureSql,
                _connectionString,
                new SearchIndexerDataContainer(tableName));

            await _indexerClient.CreateDataSourceConnectionAsync(dataSourceConnection);
        }

        public async Task Delete(string connectionName)
        {
            await _indexerClient.DeleteDataSourceConnectionAsync(connectionName);
        }
    }
}
