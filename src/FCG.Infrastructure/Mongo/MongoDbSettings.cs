namespace FCG.Infrastructure.Mongo;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = "fcg";
    public string LibraryCollection { get; set; } = "library_items";
}
