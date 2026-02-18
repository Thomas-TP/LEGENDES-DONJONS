using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Tests.DomainTests;

public class HerosTests
{
    private static Stats StatsGuerrier => new(120, 20, 18, 6, 10, 15, 8);

    [Fact]
    public void CreerHeros_StatsInitialisees()
    {
        var heros = new Heros("Arthas", ClasseHeros.Guerrier, StatsGuerrier);

        Assert.Equal("Arthas", heros.Nom);
        Assert.Equal(ClasseHeros.Guerrier, heros.Classe);
        Assert.Equal(120, heros.PointsDeVie);
        Assert.Equal(20, heros.PointsDeMana);
        Assert.True(heros.EstVivant);
    }

    [Fact]
    public void SubirDegats_ReduitPV()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);

        heros.SubirDegats(30);

        Assert.Equal(90, heros.PointsDeVie);
        Assert.True(heros.EstVivant);
    }

    [Fact]
    public void SubirDegats_PVNeDescendPasSousZero()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);

        heros.SubirDegats(999);

        Assert.Equal(0, heros.PointsDeVie);
        Assert.False(heros.EstVivant);
    }

    [Fact]
    public void SubirDegats_MontantNegatifIgnore()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);

        heros.SubirDegats(-10);

        Assert.Equal(120, heros.PointsDeVie);
    }

    [Fact]
    public void Soigner_RestaurePV()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        heros.SubirDegats(50);

        heros.Soigner(30);

        Assert.Equal(100, heros.PointsDeVie);
    }

    [Fact]
    public void Soigner_NeDepassePasMax()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        heros.SubirDegats(10);

        heros.Soigner(999);

        Assert.Equal(120, heros.PointsDeVie);
    }

    [Fact]
    public void ConsommerMana_ReduitPM()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);

        heros.ConsommerMana(10);

        Assert.Equal(10, heros.PointsDeMana);
    }

    [Fact]
    public void RestaurerMana_AugmentePM()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        heros.ConsommerMana(15);

        heros.RestaurerMana(10);

        Assert.Equal(15, heros.PointsDeMana);
    }

    [Fact]
    public void Equiper_Arme_AjouteBonusStats()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        var arme = new Equipement("Épée", TypeEquipement.Arme, new Stats(0, 0, 5, 0, 0, 0, 0));

        heros.Equiper(arme);

        Assert.Equal(23, heros.StatsActuelles.Force); // 18 + 5
    }

    [Fact]
    public void Equiper_ArmureEtAccessoire_CumulBonusStats()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        var armure = new Equipement("Plates", TypeEquipement.Armure, new Stats(10, 0, 0, 0, 0, 8, 0));
        var accessoire = new Equipement("Anneau", TypeEquipement.Accessoire, new Stats(0, 0, 3, 0, 0, 0, 0));

        heros.Equiper(armure);
        heros.Equiper(accessoire);

        Assert.Equal(130, heros.StatsActuelles.PointsDeVieMax); // 120 + 10
        Assert.Equal(23, heros.StatsActuelles.Defense);          // 15 + 8
        Assert.Equal(21, heros.StatsActuelles.Force);            // 18 + 3
    }

    [Fact]
    public void AjouterCompetence_CompetenceAjoutee()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);
        var competence = new Competence("Frappe", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi);

        heros.AjouterCompetence(competence);

        Assert.Single(heros.GetCompetences());
        Assert.Equal("Frappe", heros.GetCompetences()[0].Nom);
    }

    [Fact]
    public void AppliquerStatut_ChangeLEtat()
    {
        var heros = new Heros("Test", ClasseHeros.Guerrier, StatsGuerrier);

        heros.AppliquerStatut(StatutEffet.Poison);

        Assert.Equal(StatutEffet.Poison, heros.StatutActuel);
    }
}
