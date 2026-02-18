namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant le système de quêtes et d'objectifs pour le joueur.
/// Permet de débloquer, suivre et terminer des quêtes pour obtenir des récompenses.
/// </summary>
public interface IQueteService
{
    List<Quete> ObtenirToutes();

    /// <summary>
    /// Sélection de quêtes non terminées.
    /// </summary>
    List<Quete> ObtenirActives();

    /// <summary>
    /// Sélection de quêtes validées et récompensées.
    /// </summary>
    List<Quete> ObtenirTerminees();

    /// <summary>
    /// Vérifie toutes les quêtes actives par rapport aux stats actuelles du joueur (kills, victoires...).
    /// Valide automatiquement celles dont les conditions sont remplies.
    /// </summary>
    void Verifier(ContexteQuete contexte);

    /// <summary>
    /// Retourne la liste des quêtes fraîchement validées pour afficher des notifications.
    /// </summary>
    List<Quete> NouvellesQuetesTerminees();

    /// <summary>
    /// Recharge l'état des quêtes depuis une sauvegarde.
    /// </summary>
    void Restaurer(List<QueteSauvegarde> sauvegardes);
}

/// <summary>
/// Représente une quête (mission) avec objectif et récompense.
/// </summary>
public class Quete
{
    public string Id { get; init; } = "";
    public string Nom { get; init; } = "";
    public string Description { get; init; } = "";
    public string Objectif { get; init; } = "";
    public string Icone { get; init; } = "📜";
    public int RecompenseOr { get; init; }
    public int RecompenseXp { get; init; }
    public bool Terminee { get; set; }
    public DateTime? DateCompletion { get; set; }
}

/// <summary>
/// Contient toutes les statistiques nécessaires pour vérifier les conditions des quêtes.
/// (Nombre de boss tués, profondeur atteinte, etc.)
/// </summary>
public class ContexteQuete
{
    public int TotalKills { get; init; }
    public int TotalVictoires { get; init; }
    public int BossVaincus { get; init; }
    public int NiveauMaxAtteint { get; init; }
    public int VaguesArene { get; init; }
    public int DonjonProfondeur { get; init; }
    public bool DragonAncienVaincu { get; init; }
    public bool LicheVaincue { get; init; }
    public bool GolemVaincu { get; init; }
    public bool HydreVaincue { get; init; }
    public bool DemonVaincu { get; init; }
    public int MonstresElementairesTues { get; init; }
}

/// <summary>
/// Pour la sauvegarde : stocke uniquement l'ID et la date de fin.
/// </summary>
public class QueteSauvegarde
{
    public string Id { get; set; } = "";
    public DateTime? DateCompletion { get; set; }
}
