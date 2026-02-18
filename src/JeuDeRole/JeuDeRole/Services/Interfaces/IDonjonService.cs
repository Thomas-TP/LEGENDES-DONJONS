namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant la génération procédurale du donjon.
/// Crée les salles, détermine les rencontres (boss, monstres, trésors) et la structure du donjon.
/// </summary>
public interface IDonjonService
{
    /// <summary>
    /// Génère un donjon complet (un run) jusqu'au boss final.
    /// Contient des salles de combat, des salles de repos (fontaines), des événements aléatoires, etc.
    /// </summary>
    /// <param name="profondeurMax">La limite d'étages avant le boss final.</param>
    /// <returns>Une instance de Donjon contenant la liste des salles.</returns>
    Donjon GenererDonjon(int profondeurMax);
}

/// <summary>
/// Représente l'ensemble du donjon généré pour une session.
/// </summary>
public class Donjon
{
    /// <summary>
    /// Liste des salles générées, ordonnées par étage.
    /// </summary>
    public List<SalleDonjon> Salles { get; init; } = new();

    /// <summary>
    /// Profondeur maximale (le dernier étage où se trouve le BossFinal).
    /// </summary>
    public int ProfondeurMax { get; init; }
}

public enum TypeSalle
{
    Combat,
    Evenement,
    Repos,
    MiniBoss,
    BossFinal
}

/// <summary>
/// Représente une salle individuelle dans un étage du donjon.
/// </summary>
public class SalleDonjon
{
    public int Etage { get; init; }
    public TypeSalle Type { get; init; }
    public string Nom { get; init; } = "";
    public bool Visitee { get; set; }
}
