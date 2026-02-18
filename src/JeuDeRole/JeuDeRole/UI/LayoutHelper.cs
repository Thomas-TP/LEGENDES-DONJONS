namespace JeuDeRole.UI;

/// <summary>
/// Adapte les éléments visuels à la taille du terminal.
/// </summary>
public static class LayoutHelper
{
    public static int Largeur => Math.Max(40, Console.WindowWidth);
    public static int Hauteur => Math.Max(20, Console.WindowHeight);

    public static bool EstLarge => Largeur >= 120;
    public static bool EstMoyen => Largeur >= 80;

    /// <summary>Largeur de barre de vie adaptée au terminal.</summary>
    public static int LargeurBarre => EstLarge ? 30 : EstMoyen ? 20 : 12;

    /// <summary>Largeur de barre de vie courte (recap combat).</summary>
    public static int LargeurBarreCourte => EstLarge ? 20 : EstMoyen ? 15 : 8;

    /// <summary>Largeur du graphique bar chart.</summary>
    public static int LargeurChart => EstLarge ? 60 : EstMoyen ? 45 : 30;

    /// <summary>
    /// Centre un texte multi-lignes dans la fenêtre du terminal.
    /// </summary>
    /// <param name="texte">Le texte à centrer.</param>
    /// <returns>Le texte formaté avec les espaces de padding nécessaires.</returns>
    public static string Centrer(string texte)
    {
        var lignes = texte.Split('\n');
        int maxLen = lignes.Max(l => l.Length);
        int padding = Math.Max(0, (Largeur - maxLen) / 2);
        return string.Join('\n', lignes.Select(l => new string(' ', padding) + l));
    }

    /// <summary>
    /// Crée une ligne de bordure décorative pleine largeur.
    /// </summary>
    /// <param name="c">Le caractère utilisé pour la bordure.</param>
    /// <returns>Une chaîne représentant la bordure.</returns>
    public static string Bordure(char c = '═') => new(c, Math.Min(Largeur - 2, 120));

    /// <summary>
    /// Entoure un texte d'un cadre décoratif.
    /// </summary>
    /// <param name="texte">Le texte à encadrer.</param>
    /// <param name="coin">Caractère pour les coins du haut.</param>
    /// <param name="bord">Caractère pour les bords horizontaux.</param>
    /// <param name="mur">Caractère pour les bords verticaux.</param>
    /// <returns>Le texte encadré sous forme de chaîne multi-lignes.</returns>
    public static string Cadre(string texte, char coin = '╔', char bord = '═', char mur = '║')
    {
        var lignes = texte.Split('\n');
        int maxLen = lignes.Max(l => l.Length);
        int largeur = Math.Min(maxLen + 4, Largeur - 2);
        string haut = coin + new string(bord, largeur - 2) + MirrorCoin(coin);
        string bas = MirrorCoinBas(coin) + new string(bord, largeur - 2) + MirrorCoinBasDroit(coin);
        var result = new List<string> { haut };
        foreach (var l in lignes)
            result.Add($"{mur} {l.PadRight(largeur - 4)} {mur}");
        result.Add(bas);
        return string.Join('\n', result);
    }

    private static char MirrorCoin(char c) => c switch { '╔' => '╗', _ => c };
    private static char MirrorCoinBas(char c) => c switch { '╔' => '╚', _ => c };
    private static char MirrorCoinBasDroit(char c) => c switch { '╔' => '╝', _ => c };
}
