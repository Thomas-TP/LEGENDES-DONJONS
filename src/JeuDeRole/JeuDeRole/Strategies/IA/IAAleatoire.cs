using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;

namespace JeuDeRole.Strategies.IA;

/// <summary>
/// Stratégie IA simple qui choisit ses actions complètement au hasard.
/// Utilisée pour les monstres basiques ou "stupides".
/// </summary>
public class IAAleatoire : IStrategieIA
{
    private readonly Random _random;

    public IAAleatoire(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Sélectionne une action au hasard parmi celles possibles.
    /// Ne prend pas en compte l'état du combat (PV ennemis, etc.).
    /// </summary>
    public ActionCombat ChoisirAction(Monstre monstre, List<ICombattant> cibles)
    {
        // Filtre les cibles vivantes
        var ciblesVivantes = cibles.Where(c => c.EstVivant).ToList();
        if (ciblesVivantes.Count == 0)
            return ActionCombat.Defendre(monstre);

        // Récupère les compétences
        var competences = monstre.GetCompetences();
        if (competences.Count == 0)
            return ActionCombat.Defendre(monstre); // Fallback si aucune compétence

        // Filtre les compétences dont le coût en mana est abordable
        var competencesUtilisables = competences
            .Where(c => c.CoutMana <= monstre.PointsDeMana)
            .ToList();

        if (competencesUtilisables.Count == 0)
            return ActionCombat.Defendre(monstre); // Pas de mana => Défense

        // Choix totalement aléatoire
        var competenceChoisie = competencesUtilisables[_random.Next(competencesUtilisables.Count)];
        var cibleChoisie = ciblesVivantes[_random.Next(ciblesVivantes.Count)];

        return ActionCombat.Attaquer(monstre, competenceChoisie, new List<ICombattant> { cibleChoisie });
    }
}
