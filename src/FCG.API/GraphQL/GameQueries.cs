using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;

namespace FCG.API.GraphQL;

[ExtendObjectType("Query")]
public class GameQueries
{
    /// <summary>
    /// Consulta jogos com filtragem, ordenação e projeção dinâmica via GraphQL.
    /// Exemplo: { games(where: { genre: { eq: \"RPG\" } }) { id title price } }
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Game> GetGames([Service] IGameRepository repository)
        => repository.Query();
}
