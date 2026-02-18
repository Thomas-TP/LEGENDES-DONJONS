using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Strategies.Degats;

/// <summary>
/// Interface pour les stratégies de calcul de dégâts.
/// Permet d'implémenter différentes formules (physique, magique, mixte, brute, etc.).
/// </summary>
public interface ICalculDegats
{
    /// <summary>
    /// Calcule le montant de dégâts à infliger.
    /// </summary>
    /// <param name="attaquant">L'entité qui lance l'attaque</param>
    /// <param name="cible">L'entité qui reçoit l'attaque</param>
    /// <param name="competence">La compétence ou l'attaque utilisée</param>
    /// <returns>Le montant de dégâts final (positif)</returns>
    int Calculer(ICombattant attaquant, ICombattant cible, ICompetence competence);
}
