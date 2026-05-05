using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace FCG.Infrastructure.Mongo;

public class LibraryRepository : ILibraryRepository
{
    private readonly IMongoCollection<LibraryItem> _collection;

    static LibraryRepository()
    {
        // Convenções para mapear classes com construtor privado / propriedades private set.
        var pack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new ImmutableTypeClassMapConvention()
        };
        ConventionRegistry.Register("FCG", pack, _ => true);

        if (!BsonClassMap.IsClassMapRegistered(typeof(LibraryItem)))
        {
            BsonClassMap.RegisterClassMap<LibraryItem>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.Id).SetSerializer(new GuidSerializer(BsonType.String));
                cm.MapProperty(c => c.UserId).SetSerializer(new GuidSerializer(BsonType.String));
                cm.MapProperty(c => c.GameId).SetSerializer(new GuidSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public LibraryRepository(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.Database);
        _collection = database.GetCollection<LibraryItem>(settings.LibraryCollection);

        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var keys = Builders<LibraryItem>.IndexKeys
            .Ascending(x => x.UserId)
            .Ascending(x => x.GameId);
        var model = new CreateIndexModel<LibraryItem>(keys, new CreateIndexOptions { Unique = true, Name = "ux_user_game" });
        _collection.Indexes.CreateOne(model);
    }

    public async Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId, CancellationToken ct = default)
    {
        var count = await _collection.CountDocumentsAsync(
            x => x.UserId == userId && x.GameId == gameId,
            cancellationToken: ct);
        return count > 0;
    }

    public async Task<IReadOnlyList<LibraryItem>> ListByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var cursor = await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.AcquiredAt)
            .ToListAsync(ct);
        return cursor;
    }

    public Task AddAsync(LibraryItem item, CancellationToken ct = default)
        => _collection.InsertOneAsync(item, cancellationToken: ct);
}
