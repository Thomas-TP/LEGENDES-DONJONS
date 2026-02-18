using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un monstre contrôlé par l'IA.
/// Étend la classe Personnage avec des fonctionnalités spécifiques aux ennemis (IA, expérience donnée, scaling).
/// </summary>
public class Monstre : Personnage
{
    // Expérience octroyée aux héros en cas de victoire
    public int ExperienceDonnee { get; protected set; }
    
    // Stratégie d'IA utilisée pour décider des actions du monstre
    public IStrategieIA StrategieIA { get; }

    /// <summary>
    /// Initialise un nouveau monstre.
    /// </summary>
    /// <param name="nom">Nom du monstre</param>
    /// <param name="statsBase">Statistiques de base</param>
    /// <param name="experienceDonnee">XP donnée aux vainqueurs</param>
    /// <param name="strategieIA">Comportement de l'IA</param>
    public Monstre(string nom, Stats statsBase, int experienceDonnee, IStrategieIA strategieIA)
        : base(nom, statsBase)
    {
        ExperienceDonnee = experienceDonnee;
        StrategieIA = strategieIA;
    }

    /// <summary>
    /// Applique un multiplicateur de difficulté aux statistiques et à l'expérience donnée.
    /// Utile pour les modes de difficulté ou les événements spéciaux.
    /// </summary>
    /// <param name="multStats">Multiplicateur appliqué aux PV, Force, Defense, etc.</param>
    /// <param name="multXP">Multiplicateur appliqué à l'expérience</param>
    public virtual void AppliquerDifficulte(double multStats, double multXP)
    {
        // Ne modifie les stats que si le multiplicateur est significativement différent de 1.0
        if (Math.Abs(multStats - 1.0) > 0.01)
        {
            StatsBase = new Stats(
                (int)(StatsBase.PointsDeVieMax * multStats), StatsBase.PointsDeManaMax,
                (int)(StatsBase.Force * multStats), StatsBase.Intelligence,
                StatsBase.Agilite, (int)(StatsBase.Defense * multStats), StatsBase.ResistanceMagique);
            
            // Soigne instantanément le monstre lors de l'application de la difficulté
            PointsDeVie = StatsBase.PointsDeVieMax;
        }
        
        // Ajuste l'expérience donnée
        if (Math.Abs(multXP - 1.0) > 0.01)
            ExperienceDonnee = Math.Max(1, (int)(ExperienceDonnee * multXP));
    }

    /// <summary>
    /// Applique une mise à l'échelle (scaling) globale des statistiques.
    /// Réinitialise également les PV et PM au maximum.
    /// </summary>
    /// <param name="multiplicateur">Facteur de mise à l'échelle</param>
    public void AppliquerScaling(double multiplicateur)
    {
        StatsBase = new Stats(
            (int)(StatsBase.PointsDeVieMax * multiplicateur), StatsBase.PointsDeManaMax,
            (int)(StatsBase.Force * multiplicateur), StatsBase.Intelligence,
            StatsBase.Agilite, (int)(StatsBase.Defense * multiplicateur), StatsBase.ResistanceMagique);
        PointsDeVie = StatsBase.PointsDeVieMax;
        PointsDeMana = StatsBase.PointsDeManaMax;
    }
}
