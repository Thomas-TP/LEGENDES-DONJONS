using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Strategies.Degats;

/// <summary>
/// Stratégie de calcul des dommages magiques.
/// Se base sur l'Intelligence de l'attaquant et la Résistance Magique de la cible.
/// </summary>
public class CalculDegatsMagiques : ICalculDegats
{
    private readonly Random _random;

    public CalculDegatsMagiques(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Calcule les dégâts finaux d'une compétence magique.
    /// Formule : (Intelligence + Puissance) - RésistanceMagique + Variation (-2 à +2)
    /// Applique ensuite le multiplicateur de faiblesse/résistance élémentaire.
    /// </summary>
    public int Calculer(ICombattant attaquant, ICombattant cible, ICompetence competence)
    {
        int intelligenceAttaquant = attaquant.StatsActuelles.Intelligence;
        int resistanceCible = cible.StatsActuelles.ResistanceMagique;
        
        // Variation aléatoire pour éviter des valeurs fixes
        int variation = _random.Next(-2, 3);

        int degatsBase = (intelligenceAttaquant + competence.Puissance) - resistanceCible + variation;
        
        // Prise en compte des affinités élémentaires
        double multiplicateur = cible.GetResistance(competence.Element);
        int degats = (int)(Math.Max(1, degatsBase) * multiplicateur);
        
        // Minimum 1 dégât garanti
        return Math.Max(1, degats);
    }
}
