using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FluentAssertions;

namespace FCG.Domain.Tests.Entities;

public class PromotionTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriar()
    {
        var promo = Promotion.Create(Guid.NewGuid(), 20m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        promo.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(95)]
    public void Create_ComDescontoForaDoIntervalo_DeveLancar(decimal discount)
    {
        var act = () => Promotion.Create(Guid.NewGuid(), discount, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ComEndsAtAntesDeStartsAt_DeveLancar()
    {
        var act = () => Promotion.Create(Guid.NewGuid(), 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsValidAt_ForaDoIntervalo_DeveSerFalse()
    {
        var promo = Promotion.Create(Guid.NewGuid(), 10m, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));
        promo.IsValidAt(DateTime.UtcNow).Should().BeFalse();
    }
}
