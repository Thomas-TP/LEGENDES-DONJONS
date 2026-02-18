using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un objet pouvant être porté par un héros (Arme, Armure, Accessoire).
/// Ajoute des bonus aux statistiques du héros lorsqu'il est équipé.
/// </summary>
public class Equipement : IEquipement
{
    public string Nom { get; }
    
    // Type détermine l'emplacement d'équipement (main, corps, etc.)
    public TypeEquipement Type { get; }
    
    // Les statistiques ajoutées au porteur
    public Stats BonusStats { get; }

    public Equipement(string nom, TypeEquipement type, Stats bonusStats)
    {
        Nom = nom;
        Type = type;
        BonusStats = bonusStats;
    }
}
