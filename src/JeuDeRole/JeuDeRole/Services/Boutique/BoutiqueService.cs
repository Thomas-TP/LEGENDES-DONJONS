using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Boutique;

/// <summary>
/// Implémentation du service Boutique.
/// Contient le catalogue hardcodé des articles disponibles à l'achat.
/// Gère la transaction d'achat (retrait de l'or) et de vente.
/// </summary>
public class BoutiqueService : IBoutiqueService
{
    private int _or;

    public int Or => _or;

    // Catalogue statique des équipements disponibles
    private static readonly List<ArticleBoutique> _equipements = new()
    {
        new() { Nom = "Épée en fer", Description = "FOR +5", Prix = 50, Categorie = "Arme" },
        new() { Nom = "Bâton magique", Description = "INT +6, PM +10, RES +2", Prix = 60, Categorie = "Arme" },
        new() { Nom = "Dague d'ombre", Description = "FOR +3, AGI +4", Prix = 45, Categorie = "Arme" },
        new() { Nom = "Masse sacrée", Description = "FOR +4, INT +3, PM +5", Prix = 55, Categorie = "Arme" },
        new() { Nom = "Épée de flammes", Description = "FOR +8, INT +2", Prix = 120, Categorie = "Arme" },
        new() { Nom = "Arc elfique", Description = "FOR +4, AGI +6, INT +2", Prix = 100, Categorie = "Arme" },
        new() { Nom = "Faux maudite", Description = "FOR +10, RES -2", Prix = 130, Categorie = "Arme" },
        new() { Nom = "Armure de plates", Description = "DEF +8, PV +10, AGI -2", Prix = 80, Categorie = "Armure" },
        new() { Nom = "Robe enchantée", Description = "DEF +2, INT +3, PM +15, RES +6", Prix = 75, Categorie = "Armure" },
        new() { Nom = "Armure de cuir", Description = "DEF +4, AGI +2, PV +5", Prix = 50, Categorie = "Armure" },
        new() { Nom = "Armure de mithril", Description = "DEF +12, PV +20, RES +4", Prix = 200, Categorie = "Armure" },
        new() { Nom = "Cape d'invisibilité", Description = "AGI +8, DEF +3", Prix = 150, Categorie = "Armure" },
        new() { Nom = "Anneau de force", Description = "FOR +3", Prix = 40, Categorie = "Accessoire" },
        new() { Nom = "Amulette de sagesse", Description = "INT +4, PM +10, RES +3", Prix = 50, Categorie = "Accessoire" },
        new() { Nom = "Bottes de vitesse", Description = "AGI +5", Prix = 45, Categorie = "Accessoire" },
        new() { Nom = "Collier de vie", Description = "PV +30, DEF +2", Prix = 100, Categorie = "Accessoire" },
        new() { Nom = "Talisman élémentaire", Description = "RES +8, INT +2", Prix = 110, Categorie = "Accessoire" },
    };

    // Catalogue statique des consommables
    private static readonly List<ArticleBoutique> _objets = new()
    {
        new() { Nom = "Potion de soin", Description = "Restaure 30 PV", Prix = 10, Categorie = "Objet" },
        new() { Nom = "Grande potion de soin", Description = "Restaure 60 PV", Prix = 25, Categorie = "Objet" },
        new() { Nom = "Potion de mana", Description = "Restaure 20 PM", Prix = 10, Categorie = "Objet" },
        new() { Nom = "Grande potion de mana", Description = "Restaure 40 PM", Prix = 25, Categorie = "Objet" },
        new() { Nom = "Antidote", Description = "Soigne le poison", Prix = 15, Categorie = "Objet" },
        new() { Nom = "Élixir de puissance", Description = "Buff ATK temporaire", Prix = 40, Categorie = "Objet" },
    };

    public List<ArticleBoutique> ObtenirEquipements() => new(_equipements);
    public List<ArticleBoutique> ObtenirObjets() => new(_objets);

    public bool Acheter(ArticleBoutique article, int quantite = 1)
    {
        int coutTotal = article.Prix * quantite;
        if (_or < coutTotal) return false;
        _or -= coutTotal;
        return true;
    }

    public int VendreEquipement(Equipement equipement)
    {
        int prixVente = 20; // prix plancher si non trouvé
        var article = _equipements.FirstOrDefault(a => a.Nom == equipement.Nom);
        if (article != null)
            prixVente = article.Prix / 2; // Rachat à 50% du prix neuf
        _or += prixVente;
        return prixVente;
    }

    public void AjouterOr(int montant)
    {
        if (montant > 0) _or += montant;
    }

    public void Restaurer(int or)
    {
        _or = Math.Max(0, or);
    }
}
