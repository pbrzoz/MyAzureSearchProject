using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAzureSearchProject.DTOs;
using System.Data;
using System.Data.SqlClient;
using Document = MyAzureSearchProject.Models.Document;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

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
        private SearchClient searchClient;
        private SearchIndexClient adminClient;
        public AppController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DBConnectionString");
            serviceName = _configuration.GetSection("AzureSearchConfig").GetValue<string>("ServiceName");
            indexName= _configuration.GetSection("AzureSearchConfig").GetValue<string>("IndexName");
            apiKey= _configuration.GetSection("AzureSearchConfig").GetValue<string>("ApiKey");

            Uri serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            adminClient = new SearchIndexClient(serviceEndpoint, credential);

            // Create a SearchClient to load and query documents
            searchClient = new SearchClient(serviceEndpoint, indexName, credential);
        }

        [HttpPost("AddContent")]
        public async Task<IActionResult> AddContent([FromForm] InsertDocument document)
        {
            using(IDbConnection db= new SqlConnection(connectionString))
            {
                string query = "Insert into Documents values (@Title,@Content)";
                await db.ExecuteAsync(query,document);                
            }

            IndexDocumentsBatch<InsertDocument> batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Upload(document));
            SearchClient ingesterClient = adminClient.GetSearchClient(indexName);
            try
            {
                IndexDocumentsResult result = ingesterClient.IndexDocuments(batch);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to index some of the documents: {0}");
            }
            return Ok("Inserted data to the database and added to index");
                
        }

        [HttpPost("DeleteContent")]
        public async Task<IActionResult> DeleteContent([FromForm] int id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string query = "Delete from Documents where ID = @ID";
                var parameters = new { ID = id };
                await db.ExecuteAsync(query, parameters);
                return Ok($"Deleted Document with ID = {id}");
            }
            //IndexDocumentsBatch batch = IndexDocumentsBatch.Delete()
        }

        [HttpGet("GetContentFromDB/{id}")]
        public async Task<IActionResult> GetContentFromDB(int id)
        {
            using(IDbConnection db= new SqlConnection(connectionString))
            {
                string query = "Select ID,Title,Content from Documents where ID = @ID";
                var parameters = new { ID = id };
                Document doc= await db.QuerySingleAsync<Document>(query,parameters);
                return Ok(doc);
            }
        }

        [HttpPost("SearchContent")]
        public void SearchContent([FromForm] InsertDocument document)
        {
            SearchOptions options;
            SearchResults<Document> response;

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                OrderBy = { "" }
            };

            options.Select.Add("ID");
            options.Select.Add("Title");
            options.Select.Add("Content");

            response = searchClient.Search<Document>("*", options);
            WriteDocuments(response);
        }

        [HttpPost("CreateIndex")]
        public void CreateIndex(string indexName, SearchIndexClient adminClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Document));

            var definition = new SearchIndex(indexName, searchFields);

            var suggester = new SearchSuggester("sg", new[] { "Title", "Content" });
            definition.Suggesters.Add(suggester);

            adminClient.CreateOrUpdateIndex(definition);
        }

        [HttpPost("CreateDataSource")]
        public void CreateDataSource()
        {

        }

        [HttpPost("CreateIndexer")]
        public void CreateIndexer()
        {

        }

        // Write search results to console
        private static void WriteDocuments(SearchResults<Document> searchResults)
        {
            foreach (SearchResult<Document> result in searchResults.GetResults())
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }
    }
}
