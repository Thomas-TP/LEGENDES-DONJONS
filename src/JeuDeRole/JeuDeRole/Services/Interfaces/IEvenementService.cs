using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant la génération d'événements aléatoires dans le donjon.
/// Permet d'ajouter de la variété avec des coffres, des pièges ou des marchands errants.
/// </summary>
public interface IEvenementService
{
    /// <summary>
    /// Tire un événement aléatoire avec ses récompenses ou conséquences.
    /// Retourne un objet décrivant l'événement.
    /// </summary>
    EvenementAleatoire GenererEvenement();
}

public enum TypeEvenement
{
    Coffre,
    Piege,
    Marchand,
    Fontaine,
    Sanctuaire,
    Embuscade
}

/// <summary>
/// Description d'un événement aléatoire rencontré.
/// Contient le texte descriptif et potentiellement une illustration ASCII.
/// </summary>
public class EvenementAleatoire
{
    public TypeEvenement Type { get; init; }
    public string Nom { get; init; } = "";
    public string Description { get; init; } = "";
    public string AsciiArt { get; init; } = "";
}
