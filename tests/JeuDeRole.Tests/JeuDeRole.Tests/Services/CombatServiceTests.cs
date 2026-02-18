using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Logging;
using JeuDeRole.Services.Combat;
using JeuDeRole.Services.Inventaire;
using JeuDeRole.Strategies.Degats;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Tests.ServicesTests;

public class CombatServiceTests
{
    private static CombatService CreerService(ICombatLogger? logger = null)
    {
        logger ??= new FakeLogger();
        var calculPhys = new CalculDegatsPhysiques(new Random(42));
        var calculMag = new CalculDegatsMagiques(new Random(42));
        var inventaireService = new InventaireService(logger);
        return new CombatService(logger, calculPhys, calculMag, inventaireService);
    }

    private static Heros CreerGuerrier(string nom = "Guerrier")
    {
        var h = new Heros(nom, ClasseHeros.Guerrier, new Stats(120, 20, 18, 6, 10, 15, 8));
        h.AjouterCompetence(new Competence("Attaque", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi));
        return h;
    }

    private static Monstre CreerGobelin()
    {
        var m = new Monstre("Gobelin", new Stats(30, 10, 8, 4, 14, 5, 3), 20, new IAAleatoire(new Random(42)));
        m.AjouterCompetence(new Competence("Morsure", 0, 6, TypeDegat.Physique, CibleType.UnEnnemi));
        return m;
    }

    [Fact]
    public void LancerCombat_HerosGagnent_ContreGobelinFaible()
    {
        var service = CreerService();

        // Le héros attaque toujours avec la première compétence sur le premier ennemi
        service.DemanderActionJoueur = (heros, ennemis, allies, inv) =>
        {
            var comp = heros.GetCompetences().First();
            return ActionCombat.Attaquer(heros, comp, new List<ICombattant> { ennemis.First() });
        };

        var heros = new List<Heros> { CreerGuerrier() };
        var monstres = new List<Monstre> { CreerGobelin() };
        var inventaire = new Inventaire();

        var resultat = service.LancerCombat(heros, monstres, inventaire);

        Assert.True(resultat.VictoireHeros);
        Assert.True(resultat.TotalDegatsInfliges > 0);
        Assert.True(resultat.NombreTours >= 1);
    }

    [Fact]
    public void LancerCombat_ResultatContientStatistiques()
    {
        var service = CreerService();

        service.DemanderActionJoueur = (heros, ennemis, allies, inv) =>
        {
            var comp = heros.GetCompetences().First();
            return ActionCombat.Attaquer(heros, comp, new List<ICombattant> { ennemis.First() });
        };

        var heros = new List<Heros> { CreerGuerrier() };
        var monstres = new List<Monstre> { CreerGobelin() };
        var inventaire = new Inventaire();

        var resultat = service.LancerCombat(heros, monstres, inventaire);

        Assert.True(resultat.NombreTours > 0);
        Assert.True(resultat.TotalDegatsInfliges > 0);
    }

    [Fact]
    public void LancerCombat_Defense_NInfligePasDeDegats()
    {
        var service = CreerService();
        int tourCount = 0;

        // Le héros défend les 2 premiers tours puis attaque
        service.DemanderActionJoueur = (heros, ennemis, allies, inv) =>
        {
            tourCount++;
            if (tourCount <= 2)
                return ActionCombat.Defendre(heros);

            var comp = heros.GetCompetences().First();
            return ActionCombat.Attaquer(heros, comp, new List<ICombattant> { ennemis.First() });
        };

        var heros = new List<Heros> { CreerGuerrier() };
        var monstres = new List<Monstre> { CreerGobelin() };

        var resultat = service.LancerCombat(heros, monstres, new Inventaire());

        Assert.True(resultat.NombreTours >= 3);
    }

    /// <summary>
    /// Logger factice pour les tests — ne fait rien.
    /// </summary>
    private class FakeLogger : ICombatLogger
    {
        public void LogAction(string message) { }
        public void LogDegats(ICombattant source, ICombattant cible, int degats) { }
        public void LogSoin(ICombattant source, ICombattant cible, int montant) { }
        public void LogMort(ICombattant combattant) { }
        public void LogStatut(ICombattant cible, StatutEffet statut) { }
        public void LogDebutTour(int numeroTour) { }
        public void LogFinCombat(bool victoireHeros) { }
        public void LogExperience(string nomHeros, int xp, int niveauActuel, bool levelUp) { }
        public void LogPhaseChangement(string nomBoss, int phase, string nomPhase) { }
        public void LogActionMonstre(string nomMonstre, string nomCompetence, string nomCible, bool estAoE) { }
        public void LogDebutActionMonstre(string nomMonstre) { }
        public void LogDefense(string nomCombattant) { }
        public void LogCritique(ICombattant source, ICombattant cible, int degats) { }
        public void LogEsquive(ICombattant cible) { }
        public void LogElement(Element element, double multiplicateur) { }
    }
}
