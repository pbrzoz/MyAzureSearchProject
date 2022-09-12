using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace MyAzureSearchProject.Models
{
    public class DocumentSearch
    {
        public DocumentSearch(Document document)
        {
            Id = document.Id.ToString();
            Title = document.Title;
            Content = document.Content;
        }

        public DocumentSearch() { }

        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }

        [SearchableField(IsSortable = true)]
        public string Title { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string Content { get; set; }
    }
}
