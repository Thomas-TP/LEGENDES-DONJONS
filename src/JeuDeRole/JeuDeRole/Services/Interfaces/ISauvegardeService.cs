using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Models;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant la persistance des données.
/// Sérialise et désérialise l'état complet du jeu en fichier JSON.
/// </summary>
public interface ISauvegardeService
{
    /// <summary>
    /// Sauvegarde toutes les données importantes (héros, inventaire, progression).
    /// </summary>
    /// <param name="donnees">L'objet DTO contenant toutes les infos.</param>
    /// <param name="chemin">Chemin vers le fichier (ex: sauvegarde.json).</param>
    void Sauvegarder(DonneesSauvegarde donnees, string chemin);

    /// <summary>
    /// Charge une sauvegarde existante.
    /// Retourne null si le fichier n'existe pas ou est corrompu.
    /// </summary>
    DonneesSauvegarde? Charger(string chemin);

    /// <summary>
    /// Vérifie rapidement si une sauvegarde est présente.
    /// </summary>
    bool SauvegardeExiste(string chemin);
}

/// <summary>
/// DTO (Data Transfer Object) représentant l'intégralité d'une sauvegarde.
/// </summary>
public class DonneesSauvegarde
{
    public List<HerosSauvegarde> Heros { get; set; } = new();
    public List<ResultatCombat> Historique { get; set; } = new();
    public List<EntreeBestiaire> Bestiaire { get; set; } = new();
    public List<SuccesSauvegarde> Succes { get; set; } = new();
    public int TotalVictoires { get; set; }
    public int TotalDefaites { get; set; }
    public int BossVaincus { get; set; }
    public int VaguesAreneMax { get; set; }
    public int DonjonProfondeurMax { get; set; }
    public int Or { get; set; }
    public List<QueteSauvegarde> Quetes { get; set; } = new();
    public List<string> BossVaincusNoms { get; set; } = new();
    public string Difficulte { get; set; } = "Normal";
    public DateTime DateSauvegarde { get; set; } = DateTime.Now;
}

/// <summary>
/// Version allégée d'un Héros pour la sauvegarde (sans méthodes, juste les données).
/// </summary>
public class HerosSauvegarde
{
    public string Nom { get; set; } = "";
    public string Classe { get; set; } = "";
    public int Niveau { get; set; } = 1;
    public int Experience { get; set; }
    public int PvMax { get; set; }
    public int PmMax { get; set; }
    public int Force { get; set; }
    public int Intelligence { get; set; }
    public int Agilite { get; set; }
    public int Defense { get; set; }
    public int ResistanceMagique { get; set; }
}

public class SuccesSauvegarde
{
    public string Id { get; set; } = "";
    public DateTime? DateDeblocage { get; set; }
}
