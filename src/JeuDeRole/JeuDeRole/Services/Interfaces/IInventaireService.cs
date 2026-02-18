using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant la logique d'utilisation des objets depuis l'inventaire.
/// Permet d'appliquer les effets des potions (soin, mana, buff) sur un combattant.
/// </summary>
public interface IInventaireService
{
    /// <summary>
    /// Utilise un objet consommable (Potion de Soin, Mana) sur un personnage ciblé.
    /// Applique l'effet et retire l'objet de l'inventaire.
    /// </summary>
    /// <param name="objet">L'objet à consommer.</param>
    /// <param name="cible">Le Héros ou Monstre qui reçoit l'effet.</param>
    /// <param name="inventaire">L'inventaire d'où l'objet est retiré.</param>
    void UtiliserObjet(ObjetConsommable objet, ICombattant cible, Domain.Entities.Inventaire inventaire);
}
