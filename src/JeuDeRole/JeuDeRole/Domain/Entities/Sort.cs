using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un sortilège magique.
/// Extension de Competence souvent utilisée par les lanceurs de sorts (Mages, etc.).
/// Ajoute la notion d'École (ex: Incantation, Nécromancie, Élémentaire).
/// </summary>
public class Sort : Competence
{
    // Catégorie ou école de magie (pour affichage ou synergies futures)
    public string Ecole { get; }

    /// <summary>
    /// Crée un nouveau sort.
    /// </summary>
    /// <param name="nom">Nom du sort</param>
    /// <param name="coutMana">Coût en mana</param>
    /// <param name="puissance">Puissance magique</param>
    /// <param name="typeDegat">Souvent Magique, mais parfois Physique</param>
    /// <param name="cible">Cible</param>
    /// <param name="ecole">École de magie</param>
    /// <param name="effetSecondaire">Effet additionnel</param>
    /// <param name="dureeEffet">Durée de l'effet</param>
    /// <param name="niveauRequis">Niveau pour apprendre</param>
    /// <param name="element">Élément magique</param>
    public Sort(string nom, int coutMana, int puissance, TypeDegat typeDegat,
                CibleType cible, string ecole, StatutEffet effetSecondaire = StatutEffet.Aucun,
                int dureeEffet = 3, int niveauRequis = 1, Element element = Element.Neutre)
        : base(nom, coutMana, puissance, typeDegat, cible, effetSecondaire, dureeEffet, niveauRequis, element)
    {
        Ecole = ecole;
    }
}
