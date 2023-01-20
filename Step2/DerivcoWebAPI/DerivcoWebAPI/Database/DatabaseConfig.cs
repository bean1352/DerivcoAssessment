namespace DerivcoWebAPI.Database
{
    public class DatabaseConfig
    {
        public string? Name { get; set; }
        public DatabaseConfig()
        {
            Name = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Database")["DatabaseName"];
        }
    }
}
