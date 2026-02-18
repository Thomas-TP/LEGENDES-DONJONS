using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un héros contrôlé par le joueur.
/// Étend la classe de base Personnage avec des spécificités liées à la classe (Guerrier, Mage, etc.) et à l'équipement.
/// </summary>
public class Heros : Personnage
{
    // Classe du héros, détermine les bonus de stats, les compétences disponibles, etc.
    public ClasseHeros Classe { get; }
    
    // Équipement actuel du héros (Arme, Armure, Accessoire)
    public IEquipement? Arme { get; private set; }
    public IEquipement? Armure { get; private set; }
    public IEquipement? Accessoire { get; private set; }

    /// <summary>
    /// Initialise un nouveau héros.
    /// </summary>
    /// <param name="nom">Nom du héros</param>
    /// <param name="classe">Classe choisie</param>
    /// <param name="statsBase">Statistiques de base (souvent liées à la classe)</param>
    /// <param name="niveau">Niveau initial</param>
    public Heros(string nom, ClasseHeros classe, Stats statsBase, int niveau = 1)
        : base(nom, statsBase, niveau)
    {
        Classe = classe;
    }

    /// <summary>
    /// Calcule les statistiques totales du héros, en incluant les bonus d'équipement en plus des effets temporaires.
    /// </summary>
    public override Stats StatsActuelles
    {
        get
        {
            // Récupère les stats de base modifiées par les buffs/debuffs (appel à la base)
            var stats = base.StatsActuelles;
            
            // Ajoute les bonus des équipements équipés
            if (Arme != null) stats = stats + Arme.BonusStats;
            if (Armure != null) stats = stats + Armure.BonusStats;
            if (Accessoire != null) stats = stats + Accessoire.BonusStats;
            return stats;
        }
    }

    /// <summary>
    /// Équipe un objet au héros. Remplace l'équipement existant du même type.
    /// </summary>
    public void Equiper(IEquipement equipement)
    {
        switch (equipement.Type)
        {
            case TypeEquipement.Arme:
                Arme = equipement;
                break;
            case TypeEquipement.Armure:
                Armure = equipement;
                break;
            case TypeEquipement.Accessoire:
                Accessoire = equipement;
                break;
        }
    }

    /// <summary>
    /// Ajoute de l'expérience au héros et gère la montée de niveau.
    /// Retourne true si le héros a gagné au moins un niveau.
    /// </summary>
    public bool GagnerExperience(int xp)
    {
        Experience += xp;
        bool aMonteDeNiveau = false;

        // Boucle pour gérer plusieurs niveaux gagnés d'un coup
        while (Experience >= ExperiencePourProchainNiveau)
        {
            Experience -= ExperiencePourProchainNiveau;
            MonterDeNiveau();
            aMonteDeNiveau = true;
        }

        return aMonteDeNiveau;
    }

    /// <summary>
    /// Gère la logique de montée de niveau : augmentation du compteur de niveau et application des bonus de stats.
    /// </summary>
    private void MonterDeNiveau()
    {
        Niveau++;

        // Bonus stats définis par niveau selon la classe du héros
        // (PV, PM, Force, Intel, Agi, Def, ResMag)
        var bonus = Classe switch
        {
            ClasseHeros.Guerrier => new Stats(12, 2, 3, 1, 1, 2, 1),
            ClasseHeros.Mage => new Stats(5, 10, 1, 3, 1, 1, 2),
            ClasseHeros.Voleur => new Stats(8, 3, 2, 1, 3, 1, 1),
            ClasseHeros.Clerc => new Stats(8, 7, 1, 2, 1, 1, 2),
            ClasseHeros.Paladin => new Stats(10, 4, 2, 2, 1, 3, 2),
            ClasseHeros.Necromancien => new Stats(5, 9, 1, 3, 1, 1, 2),
            ClasseHeros.Assassin => new Stats(7, 3, 2, 1, 3, 1, 1),
            ClasseHeros.Druide => new Stats(8, 6, 1, 2, 2, 2, 2),
            _ => Stats.Zero
        };

        StatsBase = StatsBase + bonus;

        // Restauration complète des PV et PM lors du passage de niveau
        PointsDeVie = StatsBase.PointsDeVieMax;
        PointsDeMana = StatsBase.PointsDeManaMax;
    }
}
