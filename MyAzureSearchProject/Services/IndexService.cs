using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using MyAzureSearchProject.Models;

namespace MyAzureSearchProject.Services
{
    public class IndexService
    {
        private readonly SearchIndexClient _adminClient;
        private readonly string _searchSuggester;

        public IndexService(SearchIndexClient adminClient, string searchSuggester)
        {
            _adminClient = adminClient;
            _searchSuggester = searchSuggester;
        }

        public async Task Create(string indexName, string[] sourceFields)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(DocumentSearch));

            SearchIndex definition = new SearchIndex(indexName, searchFields);

            SearchSuggester suggester = new SearchSuggester(_searchSuggester, sourceFields);
            definition.Suggesters.Add(suggester);

            await _adminClient.CreateOrUpdateIndexAsync(definition);
        }

        public async Task Delete(string indexName)
        {
            await _adminClient.DeleteIndexAsync(indexName);
        }
    }
}
