namespace DerivcoWebAPI.Database
{
    public class DatabaseConfig
    {
        public string? Name { get; set; }
        public DatabaseConfig()
        {
            //Get database connection string from the appsettings.json file on instantionation
            Name = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Database")["DatabaseName"];
        }
    }
}
