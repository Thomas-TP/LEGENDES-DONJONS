using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Evenements;

/// <summary>
/// Implémentation du service d'événements.
/// Fournit une liste d'événements prédéfinis (Coffre, Piège...) avec leurs descriptions et ASCII Art.
/// </summary>
public class EvenementService : IEvenementService
{
    private readonly Random _random = new();

    // Catalogue statique des événements possibles
    private static readonly EvenementAleatoire[] Evenements =
    {
        new()
        {
            Type = TypeEvenement.Coffre,
            Nom = "Coffre au trésor",
            Description = "Un coffre mystérieux ! Vous trouvez des potions.",
            AsciiArt = @"
    ____
   |    |
   |    |
  _|____|_
 |  ____  |
 | |    | |
 | |____| |
 |________|"
        },
        new()
        {
            Type = TypeEvenement.Piege,
            Nom = "Piège à pointes !",
            Description = "Le sol s'effondre ! Chaque héros subit des dégâts.",
            AsciiArt = @"
  /\ /\ /\ /\
 /  V  V  V  \
|    PIEGE!    |
 \  ^  ^  ^  /
  \/ \/ \/ \/"
        },
        new()
        {
            Type = TypeEvenement.Marchand,
            Nom = "Marchand ambulant",
            Description = "Un marchand vous propose ses services.",
            AsciiArt = @"
     ___
    /   \
   | $ $ |
    \___/
   /|   |\
  / | _ | \
    |/ \|
    || ||"
        },
        new()
        {
            Type = TypeEvenement.Fontaine,
            Nom = "Fontaine magique",
            Description = "Une fontaine étincelante restaure vos forces.",
            AsciiArt = @"
      _.._
    .' ~  '.
   (  ~  ~  )
    '._ ~ _.'
   __|     |__
  /  |~~~~~|  \
 '---|     |---'
     |_____|"
        },
        new()
        {
            Type = TypeEvenement.Sanctuaire,
            Nom = "Sanctuaire ancien",
            Description = "Une aura divine renforce temporairement l'équipe.",
            AsciiArt = @"
      /\
     /  \
    / ** \
   /  **  \
  /   **   \
 /____**____\
      ||
      ||
  ====||===="
        },
        new()
        {
            Type = TypeEvenement.Embuscade,
            Nom = "Embuscade !",
            Description = "Des monstres vous attaquent par surprise !",
            AsciiArt = @"
  !!! DANGER !!!
    \  |  /
     \ | /
      \|/
   ----*----
      /|\
     / | \
    /  |  \"
        },
    };

    /// <summary>
    /// Sélectionne un événement aléatoire parmi la liste disponible.
    /// </summary>
    public EvenementAleatoire GenererEvenement()
    {
        return Evenements[_random.Next(Evenements.Length)];
    }
}
