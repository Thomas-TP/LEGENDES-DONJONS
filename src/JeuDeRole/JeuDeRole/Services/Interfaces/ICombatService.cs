using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Models;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Interface pour le service gérant la logique des combats.
/// Coordonne les tours, les actions des héros et des ennemis, et l'usage de l'inventaire en combat.
/// </summary>
public interface ICombatService
{
    /// <summary>
    /// Lance un combat entre un groupe de héros et un groupe de monstres.
    /// Exécute la boucle de combat jusqu'à la victoire ou la défaite.
    /// Retourne un objet ResultatCombat contenant le résumé de la bataille.
    /// </summary>
    /// <param name="heros">La liste des héros participants.</param>
    /// <param name="monstres">La liste des ennemis.</param>
    /// <param name="inventaire">L'inventaire commun accessible pendant le combat (consommables).</param>
    ResultatCombat LancerCombat(List<Heros> heros, List<Monstre> monstres, Domain.Entities.Inventaire inventaire);
}
