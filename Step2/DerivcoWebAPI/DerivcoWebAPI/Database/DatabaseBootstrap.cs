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
                "BetID uniqueidentifier NOT NULL PRIMARY KEY," +
                "BetNumber int NOT NULL," +
                "BetAmount float NOT NULL," +
                "BetType int NOT NULL" +
                ");");

            connection.Execute("Create Table if not exists Spin (" +
                "SpinID uniqueidentifier NOT NULL PRIMARY KEY," +
                "Timestamp DateTime NOT NULL," +
                "SpinValue int NOT NULL" +
                ");");

            connection.Execute("Create Table if not exists Payout (" +
                "PayoutID uniqueidentifier NOT NULL PRIMARY KEY," +
                "Win bit NOT NULL," +
                "WinningNumber int NOT NULL," +
                "WinningAmount float NOT NULL," +
                "TotalWinningAmount float NOT NULL," +
                "BetID uniqueidentifier NOT NULL," +
                "FOREIGN KEY (BetID) REFERENCES Bet(BetID)" +
                ");");
    }
        public string? GetConnectionString()
        {
            return databaseConfig.Name;
        }
    }
}
