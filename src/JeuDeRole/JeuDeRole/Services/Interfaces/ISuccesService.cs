namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant les succès (achievements) du jeu.
/// Similaire aux quêtes, mais pour des exploits globaux (ex: Tuer 1000 monstres).
/// </summary>
public interface ISuccesService
{
    /// <summary>
    /// Vérifie si de nouveaux succès ont été débloqués selon le contexte actuel.
    /// </summary>
    void Verifier(ContexteSucces contexte);

    List<Succes> ObtenirTous();
    List<Succes> ObtenirDebloques();

    /// <summary>
    /// Retourne les succès qui viennent d'être débloqués depuis la dernière vérification.
    /// </summary>
    List<Succes> NouveauxSucces();

    /// <summary>
    /// Restaure les succès déjà obtenus depuis une sauvegarde.
    /// </summary>
    void Restaurer(List<SuccesSauvegarde> sauvegardes);
}

/// <summary>
/// Définition d'un succès.
/// </summary>
public class Succes
{
    public string Id { get; init; } = "";
    public string Nom { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icone { get; init; } = "🏆";
    public bool Debloque { get; set; }
    public DateTime? DateDeblocage { get; set; }
}

/// <summary>
/// Contexte contenant toutes les métriques nécessaires pour valider les succès.
/// </summary>
public class ContexteSucces
{
    public int TotalKills { get; init; }
    public int TotalVictoires { get; init; }
    public int TotalDefaites { get; init; }
    public int BossVaincus { get; init; }
    public int NiveauMaxAtteint { get; init; }
    public int VaguesArene { get; init; }
    public int DonjonsProfondeur { get; init; }
    public bool VictoireSansMort { get; init; }
    public bool VictoireSoloHeros { get; init; }
}
