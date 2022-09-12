using Dapper;
using MyAzureSearchProject.DTOs;
using MyAzureSearchProject.Models;
using System.Data;
using System.Data.SqlClient;
using Document = MyAzureSearchProject.Models.Document;

namespace MyAzureSearchProject.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DocumentSearch> Insert(InsertDocument document)
        {
            DocumentSearch docSearch;
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = "Insert into Documents OUTPUT INSERTED.Id values (@Title,@Content)";
                int insertedId = await db.QuerySingleAsync<int>(query, document);
                docSearch = new DocumentSearch(new Document { Id = insertedId, Title = document.Title, Content = document.Content });
            }

            return docSearch;
        }

        public async Task Delete(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = "Delete from Documents where ID = @ID";
                var parameters = new { ID = id };
                await db.ExecuteAsync(query, parameters);
            }
        }

        public async Task<Document> Get(int id)
        {
            Document document;

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = "Select ID,Title,Content from Documents where ID = @ID";
                var parameters = new { ID = id };
                document = await db.QuerySingleOrDefaultAsync<Document>(query, parameters);
            }

            return document;
        }
    }
}
