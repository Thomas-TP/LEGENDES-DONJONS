using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.Degats;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Tests.StrategiesTests;

public class CalculDegatsPhysiquesTests
{
    [Fact]
    public void Calculer_DegatsPositifs()
    {
        // Random figé pour déterminisme
        var random = new Random(42);
        var calcul = new CalculDegatsPhysiques(random);

        var attaquant = new Heros("Guerrier", ClasseHeros.Guerrier,
            new Stats(100, 20, 18, 6, 10, 15, 8));
        var cible = new Monstre("Gobelin",
            new Stats(40, 10, 8, 4, 14, 5, 3), 20, new IAAleatoire());
        var competence = new Competence("Frappe", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi);

        int degats = calcul.Calculer(attaquant, cible, competence);

        // (18 + 10) - 5 + variation = 23 + variation, minimum 1
        Assert.True(degats >= 1);
    }

    [Fact]
    public void Calculer_DegatsMinimumUn_SiDefenseSuperieure()
    {
        var random = new Random(42);
        var calcul = new CalculDegatsPhysiques(random);

        var attaquant = new Heros("Faible", ClasseHeros.Mage,
            new Stats(70, 80, 2, 20, 12, 6, 16));
        var cible = new Monstre("Tank",
            new Stats(200, 10, 5, 2, 5, 50, 3), 30, new IAAleatoire());
        var competence = new Competence("Coup faible", 0, 1, TypeDegat.Physique, CibleType.UnEnnemi);

        int degats = calcul.Calculer(attaquant, cible, competence);

        Assert.True(degats >= 1);
    }
}
