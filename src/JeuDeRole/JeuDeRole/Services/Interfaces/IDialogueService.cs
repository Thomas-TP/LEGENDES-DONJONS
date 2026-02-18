namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant tous les textes narratifs et dialogues du jeu.
/// Permet de centraliser les phrases d'ambiance, les intros de boss et d'étages.
/// </summary>
public interface IDialogueService
{
    /// <summary>
    /// Retourne la phrase d'intro spécifique à un type de boss (sa menace).
    /// </summary>
    string ObtenirDialogueBoss(string typeBoss);

    /// <summary>
    /// Génère un texte narratif décrivant l'ambiance d'un étage ou d'une salle spécifique.
    /// Varie selon le type de salle (Combat, Repos, Coffre, etc.) et l'étage actuel.
    /// </summary>
    string ObtenirNarrationEtage(int etage, TypeSalle type);

    /// <summary>
    /// Texte d'introduction du mode Arène.
    /// </summary>
    string ObtenirIntroArene();

    /// <summary>
    /// Texte d'introduction à l'entrée du Donjon, qui change selon la profondeur atteinte.
    /// Plus on descend, plus l'ambiance devient sombre.
    /// </summary>
    string ObtenirIntroDonjon(int profondeur);
}
