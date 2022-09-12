using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using MyAzureSearchProject.Models;

namespace MyAzureSearchProject.Services
{
    public class SearchService
    {
        private readonly SearchIndexClient _adminClient;
        private readonly SearchClient _searchClient;
        private readonly SearchClient _ingesterClient;
        private readonly string _indexName;
        private readonly string _suggesterName;

        public SearchService(SearchIndexClient adminClient, SearchClient searchClient, string indexName, string suggesterName)
        {
            _indexName = indexName;
            _adminClient = adminClient;
            _searchClient = searchClient;
            _ingesterClient = _adminClient.GetSearchClient(_indexName);
            _suggesterName = suggesterName;
        }

        public async Task AddContent(DocumentSearch docSearch)
        {
            IndexDocumentsBatch<DocumentSearch> batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Upload(docSearch));

            await _ingesterClient.IndexDocumentsAsync(batch);
        }

        public async Task DeleteContent(int id)
        {
            var list = new List<string> { id.ToString() }; //?
            await _ingesterClient.DeleteDocumentsAsync("Id", list);
        }

        public async Task<SearchResults<DocumentSearch>> Search(string document, string[] mappings)
        {
            SearchOptions options;
            SearchResults<DocumentSearch> response;

            options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                OrderBy = { "" }
            };

            foreach (var mapping in mappings)
            {
                options.Select.Add(mapping);
            }

            response = await _searchClient.SearchAsync<DocumentSearch>(document, options);

            return response;
        }

        public async Task<Response<AutocompleteResults>> AutocompleteSearch(string search)
        {
            return await _searchClient.AutocompleteAsync(search, _suggesterName);
        }
    }
}
