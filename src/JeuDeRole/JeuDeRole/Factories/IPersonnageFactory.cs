using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Factories;

/// <summary>
/// Interface pour la création de héros.
/// Centralise la logique d'initialisation des stats et compétences selon la classe choisie.
/// </summary>
public interface IPersonnageFactory
{
    /// <summary>
    /// Crée un nouveau héros (personnage joueur).
    /// </summary>
    /// <param name="nom">Nom choisi par le joueur</param>
    /// <param name="classe">Classe (Guerrier, Mage, etc.)</param>
    /// <returns>Une instance de Heros prête à jouer</returns>
    Heros CreerHeros(string nom, ClasseHeros classe);
}
