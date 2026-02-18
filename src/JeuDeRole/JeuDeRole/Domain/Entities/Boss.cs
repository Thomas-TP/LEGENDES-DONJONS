using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un monstre puissant de type "Boss".
/// Gère plusieurs phases de combat, changeant ses statistiques et compétences au fur et à mesure que ses PV diminuent.
/// </summary>
public class Boss : Monstre
{
    // Phase actuelle du combat (commence à 1)
    public int PhaseActuelle { get; private set; }
    public int NombrePhases { get; }
    
    // Dictionnaires stockant les configurations pour chaque phase
    private readonly Dictionary<int, Stats> _statsParPhase;
    private readonly Dictionary<int, List<Competence>> _competencesParPhase;
    private readonly Dictionary<int, IStrategieIA> _iaParPhase;

    /// <summary>
    /// Initialise un nouveau Boss avec ses différentes phases.
    /// </summary>
    /// <param name="nom">Nom du boss</param>
    /// <param name="statsParPhase">Map des stats par numéro de phase</param>
    /// <param name="competencesParPhase">Map des compétences disponibles par phase</param>
    /// <param name="iaParPhase">Map des stratégies d'IA par phase</param>
    /// <param name="experienceDonnee">XP totale donnée</param>
    public Boss(string nom, Dictionary<int, Stats> statsParPhase,
                Dictionary<int, List<Competence>> competencesParPhase,
                Dictionary<int, IStrategieIA> iaParPhase,
                int experienceDonnee)
        : base(nom, statsParPhase[1], experienceDonnee, iaParPhase[1])
    {
        _statsParPhase = statsParPhase;
        _competencesParPhase = competencesParPhase;
        _iaParPhase = iaParPhase;
        NombrePhases = statsParPhase.Count;
        PhaseActuelle = 1;

        // Ajout des compétences de la phase 1
        foreach (var comp in competencesParPhase[1])
            AjouterCompetence(comp);
    }

    /// <summary>
    /// Vérifie si le boss doit changer de phase en fonction de ses PV actuels.
    /// Retourne true si un changement de phase a eu lieu.
    /// </summary>
    public bool VerifierChangementPhase()
    {
        if (PhaseActuelle >= NombrePhases) return false;

        // Calcul du seuil de PV pour la prochaine phase (ex: 50% pour 2 phases)
        int seuilPv = _statsParPhase[PhaseActuelle].PointsDeVieMax *
                      (NombrePhases - PhaseActuelle) / NombrePhases;

        if (PointsDeVie <= seuilPv)
        {
            PhaseActuelle++;
            AppliquerPhase();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applique les changements liés à la nouvelle phase (stats, soin partiel, nouvelles compétences).
    /// </summary>
    private void AppliquerPhase()
    {
        var nouvellesStats = _statsParPhase[PhaseActuelle];
        StatsBase = nouvellesStats;

        // Soigner partiellement au changement de phase pour prolonger le combat
        Soigner(nouvellesStats.PointsDeVieMax / 4);

        // Mise à jour des compétences disponibles
        // Note : Idéalement on devrait clear les anciennes, ici on ajoute simplement les nouvelles
        if (_competencesParPhase.TryGetValue(PhaseActuelle, out var nouvellesComps))
        {
            foreach (var comp in nouvellesComps)
                AjouterCompetence(comp);
        }
    }

    public string GetNomPhase() => PhaseActuelle switch
    {
        1 => "Phase Normale",
        2 => "Phase Enragée",
        3 => "Phase Désespérée",
        _ => $"Phase {PhaseActuelle}"
    };

    public override void AppliquerDifficulte(double multStats, double multXP)
    {
        if (Math.Abs(multStats - 1.0) > 0.01)
        {
            // Applique le multiplicateur à toutes les phases
            foreach (var phase in _statsParPhase.Keys.ToList())
            {
                var s = _statsParPhase[phase];
                _statsParPhase[phase] = new Stats(
                    (int)(s.PointsDeVieMax * multStats), s.PointsDeManaMax,
                    (int)(s.Force * multStats), s.Intelligence,
                    s.Agilite, (int)(s.Defense * multStats), s.ResistanceMagique);
            }
            // Met à jour la phase actuelle
            StatsBase = _statsParPhase[PhaseActuelle];
            PointsDeVie = StatsBase.PointsDeVieMax;
        }
        if (Math.Abs(multXP - 1.0) > 0.01)
            ExperienceDonnee = Math.Max(1, (int)(ExperienceDonnee * multXP));
    }
}
