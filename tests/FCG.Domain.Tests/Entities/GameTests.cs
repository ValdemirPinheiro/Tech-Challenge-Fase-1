using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FluentAssertions;

namespace FCG.Domain.Tests.Entities;

public class GameTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarJogoAtivo()
    {
        var game = Game.Create("The Witcher 3", "RPG épico", "RPG", 99.90m, new DateTime(2015, 5, 19));
        game.Id.Should().NotBeEmpty();
        game.IsActive.Should().BeTrue();
        game.GetCurrentPrice(DateTime.UtcNow).Should().Be(99.90m);
    }

    [Theory]
    [InlineData("", "RPG")]
    [InlineData("X", "RPG")]
    [InlineData("Game", "")]
    public void Create_ComCamposInvalidos_DeveLancar(string title, string genre)
    {
        var act = () => Game.Create(title, "desc", genre, 10m, DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ComPrecoNegativo_DeveLancar()
    {
        var act = () => Game.Create("Jogo", "desc", "Action", -1m, DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_DeveDesativar()
    {
        var game = Game.Create("Jogo", "desc", "Action", 10m, DateTime.UtcNow);
        game.Deactivate();
        game.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentPrice_ComPromocaoAtiva_DeveAplicarMaiorDesconto()
    {
        var now = DateTime.UtcNow;
        var game = Game.Create("Jogo", "desc", "Action", 100m, now);
        var p1 = Promotion.Create(game.Id, 10m, now.AddDays(-1), now.AddDays(1));
        var p2 = Promotion.Create(game.Id, 25m, now.AddDays(-1), now.AddDays(1));

        // Adiciona promoções via reflection do agregado (preserva encapsulamento real do código)
        var promosField = typeof(Game).GetField("_promotions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var list = (List<Promotion>)promosField.GetValue(game)!;
        list.Add(p1);
        list.Add(p2);

        game.GetCurrentPrice(now).Should().Be(75m);
    }
}
