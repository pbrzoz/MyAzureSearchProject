using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using MyAzureSearchProject.DTOs;
using MyAzureSearchProject.Models;
using MyAzureSearchProject.Services;
using Document = MyAzureSearchProject.Models.Document;

namespace MyAzureSearchProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly string connectionString;
        private readonly string serviceName;
        private readonly string indexName;
        private readonly string apiKey;
        private readonly string datasourceConnectionName;
        private readonly string tableName;
        private readonly string indexerName;
        private readonly string suggesterName;
        private string[] mappings;

        private readonly SearchClient searchClient;
        private readonly SearchIndexClient adminClient;
        private readonly SearchIndexerClient indexerClient;
        private readonly Uri serviceEndpoint;
        private readonly AzureKeyCredential credential;

        private readonly IndexService _indexService;
        private readonly DataSourceService _dataSourceService;
        private readonly IndexerService _indexerService;
        private readonly DatabaseService _databaseService;
        private readonly SearchService _searchService;

        public AppController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DBConnectionString");
            serviceName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("ServiceName");
            indexName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("IndexName");
            apiKey = _configuration.GetSection("AzureSearchConfig").GetValue<string>("ApiKey");
            datasourceConnectionName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("DatasourceConnectionName");
            tableName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("TableName");
            indexerName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("IndexerName");
            suggesterName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("SuggesterName");

            mappings = new string[3] { "Id", "Title", "Content" };

            serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            credential = new AzureKeyCredential(apiKey);

            adminClient = new SearchIndexClient(serviceEndpoint, credential);
            indexerClient = new SearchIndexerClient(serviceEndpoint, credential);
            searchClient = new SearchClient(serviceEndpoint, indexName, credential);

            _indexService = new IndexService(adminClient, suggesterName);
            _dataSourceService = new DataSourceService(indexerClient, connectionString);
            _indexerService = new IndexerService(indexerClient);
            _databaseService = new DatabaseService(connectionString);
            _searchService = new SearchService(adminClient, searchClient, indexName, suggesterName);
        }

        [HttpPost("AddContent")]
        public async Task<IActionResult> AddContent([FromForm] InsertDocument document)
        {
            DocumentSearch docSearch = await _databaseService.Insert(document);
            await _searchService.AddContent(docSearch);

            return Ok("Inserted data to the database and added to index");
        }

        [HttpPost("DeleteContent")]
        public async Task<IActionResult> DeleteContent([FromForm] int id)
        {
            await _databaseService.Delete(id);

            await _searchService.DeleteContent(id);

            return Ok($"Deleted Document with ID = {id}");
        }

        [HttpGet("GetContentFromDB/{id}")]
        public async Task<IActionResult> GetContentFromDB(int id)
        {
            Document document = await _databaseService.Get(id);

            return Ok(document);
        }

        [HttpPost("SearchContent")]
        public async Task<IActionResult> SearchContent([FromForm] string document)
        {
            SearchResults<DocumentSearch> response = await _searchService.Search(document, mappings);

            return Ok(response.GetResults());
        }

        [HttpPost("AutocompleteSearch")]
        public async Task<IActionResult> AutocompleteSearch([FromForm] string search)
        {
            Response<AutocompleteResults> autocomplete = await _searchService.AutocompleteSearch(search);

            return Ok(autocomplete.Value.Results);
        }

        [HttpPost("CreateInfrastructure")]
        public async void CreateInfrastructure()
        {
            //Create Index
            string[] sourceFields = new[] { "Title", "Content" };
            await _indexService.Create(indexName, sourceFields);

            //Create Data Source
            await _dataSourceService.Create(datasourceConnectionName, tableName);

            //Create Indexer            
            await _indexerService.Create(indexerName, datasourceConnectionName, indexName, mappings);

        }

        [HttpPost("DeleteInfrastructure")]
        public async void DeleteInfrastructure()
        {
            //Delete Index
            await _indexService.Delete(indexName);

            //Delete Data Source
            await _dataSourceService.Delete(datasourceConnectionName);

            //Delete Indexer
            await _indexerService.Delete(indexerName);
        }
    }
}
