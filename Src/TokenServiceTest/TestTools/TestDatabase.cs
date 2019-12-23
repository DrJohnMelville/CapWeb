using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TokenService.Data;

namespace TokenServiceTest.TestTools
{
    public class TestDatabase
    {
        private readonly DbContextOptions<ApplicationDbContext> option;
        public ApplicationDbContext NewContext() => new ApplicationDbContext(option);

        public TestDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            option = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

            using var context = NewContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

        }
    }
}