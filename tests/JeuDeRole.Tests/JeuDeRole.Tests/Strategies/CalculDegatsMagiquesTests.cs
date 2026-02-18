using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.Degats;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Tests.StrategiesTests;

public class CalculDegatsMagiquesTests
{
    [Fact]
    public void Calculer_UtiliseIntelligenceEtResistance()
    {
        var random = new Random(42);
        var calcul = new CalculDegatsMagiques(random);

        var attaquant = new Heros("Mage", ClasseHeros.Mage,
            new Stats(70, 80, 5, 20, 12, 6, 16));
        var cible = new Monstre("Gobelin",
            new Stats(40, 10, 8, 4, 14, 5, 3), 20, new IAAleatoire());
        var sort = new Competence("Boule de Feu", 15, 25, TypeDegat.Magique, CibleType.UnEnnemi);

        int degats = calcul.Calculer(attaquant, cible, sort);

        // (20 + 25) - 3 + variation = 42 + variation, minimum 1
        Assert.True(degats >= 1);
        Assert.True(degats >= 39); // Au minimum 42 - 2 = 40, mais always >= 1
    }

    [Fact]
    public void Calculer_DegatsMinimumUn_SiResistanceElevee()
    {
        var random = new Random(42);
        var calcul = new CalculDegatsMagiques(random);

        var attaquant = new Heros("Faible", ClasseHeros.Guerrier,
            new Stats(120, 20, 18, 3, 10, 15, 8));
        var cible = new Monstre("Dragon",
            new Stats(200, 50, 20, 18, 10, 18, 50), 100, new IAAleatoire());
        var sort = new Competence("Petit sort", 5, 2, TypeDegat.Magique, CibleType.UnEnnemi);

        int degats = calcul.Calculer(attaquant, cible, sort);

        Assert.True(degats >= 1);
    }
}
