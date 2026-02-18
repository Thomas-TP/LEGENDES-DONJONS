using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;

namespace JeuDeRole.Strategies.IA;

/// <summary>
/// Stratégie IA avancée qui cible intelligemment les adversaires.
/// Convient aux ennemis "intelligents" ou "tacticiens".
/// </summary>
public class IACiblee : IStrategieIA
{
    /// <summary>
    /// Sélectionne l'action la plus efficace.
    /// Cible systématiquement le héros le plus faible (Focus Fire).
    /// </summary>
    public ActionCombat ChoisirAction(Monstre monstre, List<ICombattant> cibles)
    {
        var ciblesVivantes = cibles.Where(c => c.EstVivant).ToList();
        if (ciblesVivantes.Count == 0)
            return ActionCombat.Defendre(monstre);

        var competences = monstre.GetCompetences();
        if (competences.Count == 0)
            return ActionCombat.Defendre(monstre);

        // Analyse tactique : trouver le maillon faible
        // Cible le héros avec le moins de points de vie actuels
        var cibleFaible = ciblesVivantes
            .OrderBy(c => c.PointsDeVie)
            .First();

        // Analyse des ressources : trouver l'attaque la plus puissante disponible
        var meilleureCompetence = competences
            .Where(c => c.CoutMana <= monstre.PointsDeMana) // Vérifie le mana
            .OrderByDescending(c => c.Puissance)         // Trie par dégâts théoriques
            .FirstOrDefault();

        // Si pas de compétence utilisable (mana vide), se défendre
        if (meilleureCompetence == null)
            return ActionCombat.Defendre(monstre);

        // Lance l'attaque la plus forte sur le plus faible
        return ActionCombat.Attaquer(monstre, meilleureCompetence, new List<ICombattant> { cibleFaible });
    }
}
