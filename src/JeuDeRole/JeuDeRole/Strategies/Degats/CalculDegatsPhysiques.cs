using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Strategies.Degats;

/// <summary>
/// Stratégie de calcul des dommages physiques.
/// Se base sur la Force de l'attaquant et la Défense de la cible.
/// </summary>
public class CalculDegatsPhysiques : ICalculDegats
{
    private readonly Random _random;

    public CalculDegatsPhysiques(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Calcule les dégâts finaux d'une attaque physique.
    /// Formule : (Force + Puissance) - Défense + Variation (-2 à +2)
    /// Applique ensuite le multiplicateur d'élément (si l'arme a un élément).
    /// </summary>
    public int Calculer(ICombattant attaquant, ICombattant cible, ICompetence competence)
    {
        int forceAttaquant = attaquant.StatsActuelles.Force;
        
        // La défense physique est utilisée pour réduire les dégâts
        int defenseCible = cible.StatsActuelles.Defense;
        
        // Légère variation aléatoire
        int variation = _random.Next(-2, 3);

        int degatsBase = (forceAttaquant + competence.Puissance) - defenseCible + variation;
        
        // Multiplicateur élémentaire (souvent neutre pour le physique, sauf armes enchantées)
        double multiplicateur = cible.GetResistance(competence.Element);
        int degats = (int)(Math.Max(1, degatsBase) * multiplicateur);
        
        // Toujours au moins 1 point de dégât
        return Math.Max(1, degats);
    }
}
