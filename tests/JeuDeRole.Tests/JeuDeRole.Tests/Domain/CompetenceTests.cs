using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Tests.DomainTests;

public class CompetenceTests
{
    [Fact]
    public void CreerCompetence_ProprietesCohérentes()
    {
        var comp = new Competence("Boule de Feu", 15, 25, TypeDegat.Magique,
                                  CibleType.UnEnnemi, StatutEffet.Aucun);

        Assert.Equal("Boule de Feu", comp.Nom);
        Assert.Equal(15, comp.CoutMana);
        Assert.Equal(25, comp.Puissance);
        Assert.Equal(TypeDegat.Magique, comp.TypeDegat);
        Assert.Equal(CibleType.UnEnnemi, comp.Cible);
        Assert.Equal(StatutEffet.Aucun, comp.EffetSecondaire);
    }

    [Fact]
    public void CreerSort_HeritageCorrect()
    {
        var sort = new Sort("Blizzard", 25, 18, TypeDegat.Magique,
                            CibleType.TousLesEnnemis, "Glace");

        Assert.Equal("Blizzard", sort.Nom);
        Assert.Equal("Glace", sort.Ecole);
        Assert.Equal(TypeDegat.Magique, sort.TypeDegat);
        Assert.IsAssignableFrom<Competence>(sort);
    }

    [Fact]
    public void Competence_AvecEffetSecondaire_Poison()
    {
        var comp = new Competence("Dard", 5, 8, TypeDegat.Physique,
                                  CibleType.UnEnnemi, StatutEffet.Poison);

        Assert.Equal(StatutEffet.Poison, comp.EffetSecondaire);
    }
}
