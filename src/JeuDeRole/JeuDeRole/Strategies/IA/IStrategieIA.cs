using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;

namespace JeuDeRole.Strategies.IA;

/// <summary>
/// Interface définissant le comportement d'une Intelligence Artificielle.
/// Permet de faire varier la difficulté/stratégie des monstres.
/// </summary>
public interface IStrategieIA
{
    /// <summary>
    /// Décide de la prochaine action à entreprendre durant un tour de combat.
    /// Doit retourner une ActionCombat valide (Attaquer, Défendre, Utiliser Objet/Compétence).
    /// </summary>
    /// <param name="monstre">Le monstre qui agit</param>
    /// <param name="cibles">Liste des adversaires potentiels (héros)</param>
    /// <returns>L'action choisie par l'IA</returns>
    ActionCombat ChoisirAction(Monstre monstre, List<ICombattant> cibles);
}
