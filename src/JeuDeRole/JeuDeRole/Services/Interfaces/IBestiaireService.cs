using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant le bestiaire (encyclopédie des monstres rencontrés).
/// Permet de suivre les kills, les premières rencontres et les stats des ennemis.
/// </summary>
public interface IBestiaireService
{
    /// <summary>
    /// Enregistre une rencontre avec un nouveau type de monstre.
    /// Si le monstre est déjà connu, met à jour ses stats si nécessaire.
    /// </summary>
    void EnregistrerMonstre(string nom, int pvMax, int force, int defense,
                            Dictionary<Element, double> faiblesses, int xp);

    /// <summary>
    /// Incrémente le compteur de kills pour un monstre donné.
    /// </summary>
    void EnregistrerKill(string nom);

    /// <summary>
    /// Retourne la liste complète des monstres rencontrés.
    /// </summary>
    List<EntreeBestiaire> ObtenirBestiaire();

    /// <summary>
    /// Cherche une entrée spécifique dans le bestiaire.
    /// </summary>
    EntreeBestiaire? Obtenir(string nom);

    /// <summary>
    /// Nombre total d'ennemis vaincus toutes catégories confondues.
    /// </summary>
    int TotalKills { get; }

    /// <summary>
    /// Restaure l'état du bestiaire depuis une sauvegarde.
    /// </summary>
    void Restaurer(List<EntreeBestiaire> entrees);
}

/// <summary>
/// Représente une entrée (une page) dans le bestiaire.
/// Détaille les infos connues sur un monstre.
/// </summary>
public class EntreeBestiaire
{
    public string Nom { get; set; } = "";
    public int PvMax { get; set; }
    public int Force { get; set; }
    public int Defense { get; set; }
    public Dictionary<Element, double> Faiblesses { get; set; } = new();
    public int Xp { get; set; }
    public int NombreKills { get; set; }
    public DateTime PremiereRencontre { get; set; } = DateTime.Now;
}
