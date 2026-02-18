using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Tests.DomainTests;

public class InventaireTests
{
    [Fact]
    public void Ajouter_ObjetAjoute()
    {
        var inventaire = new Inventaire();
        var potion = new ObjetConsommable("Potion", "Soigne 30 PV", 3, c => c.Soigner(30));

        inventaire.Ajouter(potion);

        Assert.Single(inventaire.ListerObjets());
    }

    [Fact]
    public void Retirer_ObjetRetire()
    {
        var inventaire = new Inventaire();
        var potion = new ObjetConsommable("Potion", "Soigne 30 PV", 3, c => c.Soigner(30));
        inventaire.Ajouter(potion);

        bool resultat = inventaire.Retirer(potion);

        Assert.True(resultat);
        Assert.Empty(inventaire.ListerObjets());
    }

    [Fact]
    public void ListerObjets_FiltreObjetsEpuises()
    {
        var inventaire = new Inventaire();
        var potion = new ObjetConsommable("Potion", "Soigne", 1, c => c.Soigner(30));
        inventaire.Ajouter(potion);

        // Utiliser l'unique exemplaire
        var heros = new Heros("Test", ClasseHeros.Guerrier,
            new Stats(100, 20, 10, 10, 10, 10, 10));
        heros.SubirDegats(50);
        potion.Utiliser(heros);

        Assert.Empty(inventaire.ListerObjets());
    }

    [Fact]
    public void ObjetConsommable_UtiliserDecremeneQuantite()
    {
        var potion = new ObjetConsommable("Potion", "Soigne", 3, c => c.Soigner(30));
        var heros = new Heros("Test", ClasseHeros.Guerrier,
            new Stats(100, 20, 10, 10, 10, 10, 10));
        heros.SubirDegats(50);

        potion.Utiliser(heros);

        Assert.Equal(2, potion.Quantite);
        Assert.Equal(80, heros.PointsDeVie);
    }

    [Fact]
    public void ObjetConsommable_QuantiteZero_NeAppliquePasEffet()
    {
        var potion = new ObjetConsommable("Potion", "Soigne", 0, c => c.Soigner(30));
        var heros = new Heros("Test", ClasseHeros.Guerrier,
            new Stats(100, 20, 10, 10, 10, 10, 10));
        heros.SubirDegats(50);

        potion.Utiliser(heros);

        Assert.Equal(50, heros.PointsDeVie);
    }
}
