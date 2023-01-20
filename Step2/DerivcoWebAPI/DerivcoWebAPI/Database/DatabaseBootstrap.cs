using SqliteDapper.Demo.Database;
using Dapper;
using System.Linq;
using System.Data.SQLite;

namespace SqliteDapper.Demo.Database
{
    public interface IDatabaseBootstrap
    {
        void Setup();
        string? GetConnectionString();
    }
}

namespace DerivcoWebAPI.Database
{
    public class DatabaseBootstrap : IDatabaseBootstrap
    {
        public readonly DatabaseConfig databaseConfig;

        public DatabaseBootstrap(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
            Setup();
        }

        public void Setup()
        {
            if (databaseConfig.Name is null)
            {
                Console.WriteLine("Database name is null");
                return;
            }

            using var connection = new SQLiteConnection(databaseConfig.Name);

            //create database table if it does not exist
            var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Bet';");
            var tableName = table.FirstOrDefault();
            if (!string.IsNullOrEmpty(tableName) && tableName == "Bet")
                return;

            connection.Execute("Create Table if not exists Bet (" +
                "UserID uniqueidentifier NOT NULL," +
                "BetID uniqueidentifier NOT NULL," +
                "BetNumber int NOT NULL," +
                "BetAmount float NOT NULL," +
                "BetType int NOT NULL" +
                ");");
        }
        public string? GetConnectionString()
        {
            return databaseConfig.Name;
        }
    }
}
