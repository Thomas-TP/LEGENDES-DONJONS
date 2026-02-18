using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Tests.DomainTests;

public class MonstreTests
{
    private static Stats StatsGobelin => new(40, 10, 8, 4, 14, 5, 3);

    [Fact]
    public void CreerMonstre_StatsInitialisees()
    {
        var monstre = new Monstre("Gobelin", StatsGobelin, 20, new IAAleatoire());

        Assert.Equal("Gobelin", monstre.Nom);
        Assert.Equal(40, monstre.PointsDeVie);
        Assert.Equal(20, monstre.ExperienceDonnee);
        Assert.True(monstre.EstVivant);
    }

    [Fact]
    public void Monstre_SubirDegatsMortels_PVAZero()
    {
        var monstre = new Monstre("Gobelin", StatsGobelin, 20, new IAAleatoire());

        monstre.SubirDegats(40);

        Assert.Equal(0, monstre.PointsDeVie);
        Assert.False(monstre.EstVivant);
    }

    [Fact]
    public void Monstre_AjouterCompetence_FonctionneCorrectement()
    {
        var monstre = new Monstre("Gobelin", StatsGobelin, 20, new IAAleatoire());
        var comp = new Competence("Morsure", 0, 6, TypeDegat.Physique, CibleType.UnEnnemi);

        monstre.AjouterCompetence(comp);

        Assert.Single(monstre.GetCompetences());
    }
}
