using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace MyAzureSearchProject.Models
{
    public class Document
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public int Id { get; set; }

        [SearchableField(IsSortable = true)]
        public string Title { get; set; }
        
        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string Content { get; set; }
    }
}
