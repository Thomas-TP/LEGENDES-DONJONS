using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Définition d'une compétence de combat (sorts, techniques physiques, etc.).
/// Utilisée pour infliger des dégâts, soigner ou appliquer des effets.
/// </summary>
public class Competence : ICompetence
{
    public string Nom { get; }
    public int CoutMana { get; }
    
    // Valeur de base des dégâts ou du soin
    public int Puissance { get; }
    
    // Physique ou Magique (influence la stat utilisée pour la défense)
    public TypeDegat TypeDegat { get; }
    
    // Cible valide (Ennemi unique, Groupe, Allié, Soi-même)
    public CibleType Cible { get; }
    
    // Effet de statut additionnel (ex: Poison, Brûlure)
    public StatutEffet EffetSecondaire { get; }
    public int DureeEffet { get; }
    
    // Condition de niveau pour débloquer/utiliser
    public int NiveauRequis { get; }
    
    // Élément associé (Feu, Glace, etc.) pour les faiblesses
    public Element Element { get; }

    /// <summary>
    /// Crée une nouvelle compétence.
    /// </summary>
    /// <param name="nom">Nom affiché</param>
    /// <param name="coutMana">Coût en PM</param>
    /// <param name="puissance">Puissance de base</param>
    /// <param name="typeDegat">Type de dégâts</param>
    /// <param name="cible">Type de ciblage</param>
    /// <param name="effetSecondaire">Effet optionnel appliqué</param>
    /// <param name="dureeEffet">Durée de l'effet en tours</param>
    /// <param name="niveauRequis">Niveau min pour utiliser</param>
    /// <param name="element">Élément de l'attaque</param>
    public Competence(string nom, int coutMana, int puissance, TypeDegat typeDegat,
                      CibleType cible, StatutEffet effetSecondaire = StatutEffet.Aucun,
                      int dureeEffet = 3, int niveauRequis = 1, Element element = Element.Neutre)
    {
        Nom = nom;
        CoutMana = coutMana;
        Puissance = puissance;
        TypeDegat = typeDegat;
        Cible = cible;
        EffetSecondaire = effetSecondaire;
        DureeEffet = dureeEffet;
        NiveauRequis = niveauRequis;
        Element = element;
    }
}
