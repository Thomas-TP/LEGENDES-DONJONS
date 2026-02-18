using System.Runtime.InteropServices;

namespace JeuDeRole.UI;

/// <summary>
/// Service de gestion des effets sonores du jeu (Beep).
/// Les sons sont exécutés de manière asynchrone pour ne pas bloquer l'interface.
/// </summary>
public static class SoundService
{
    // Détecte si le système d'exploitation est Windows pour activer les sons
    private static bool _actif = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Joue un son pour une attaque critique (aigu et rapide).
    /// </summary>
    public static void AttaqueCritique()
    {
        if (!_actif) return;
        Task.Run(() => { try { Console.Beep(800, 100); Console.Beep(1200, 150); } catch { } });
    }

    /// <summary>
    /// Joue un son simple lorsqu'une entité subit des dégâts.
    /// </summary>
    public static void Degats()
    {
        if (!_actif) return;
        Task.Run(() => { try { Console.Beep(300, 80); } catch { } });
    }

    /// <summary>
    /// Joue une séquence descendante pour la mort d'une entité.
    /// </summary>
    public static void Mort()
    {
        if (!_actif) return;
        Task.Run(() => { try { Console.Beep(400, 150); Console.Beep(300, 150); Console.Beep(200, 300); } catch { } });
    }

    /// <summary>
    /// Joue un arpège ascendant pour un soin ou une potion.
    /// </summary>
    public static void Soin()
    {
        if (!_actif) return;
        Task.Run(() => { try { Console.Beep(523, 100); Console.Beep(659, 100); Console.Beep(784, 150); } catch { } });
    }

    /// <summary>
    /// Joue une fanfare joyeuse pour la montée de niveau.
    /// </summary>
    public static void LevelUp()
    {
        if (!_actif) return;
        Task.Run(() =>
        {
            try
            {
                Console.Beep(523, 100); Console.Beep(659, 100);
                Console.Beep(784, 100); Console.Beep(1047, 200);
            }
            catch { }
        });
    }

    /// <summary>
    /// Joue une musique de victoire triomphante.
    /// </summary>
    public static void Victoire()
    {
        if (!_actif) return;
        Task.Run(() =>
        {
            try
            {
                Console.Beep(523, 150); Console.Beep(659, 150);
                Console.Beep(784, 150); Console.Beep(1047, 300);
            }
            catch { }
        });
    }

    /// <summary>
    /// Joue une mélodie triste pour la défaite (Game Over).
    /// </summary>
    public static void Defaite()
    {
        if (!_actif) return;
        Task.Run(() =>
        {
            try
            {
                Console.Beep(400, 200); Console.Beep(350, 200);
                Console.Beep(300, 200); Console.Beep(250, 400);
            }
            catch { }
        });
    }

    /// <summary>
    /// Signale un changement de phase de boss (son d'alerte).
    /// </summary>
    public static void PhaseChange()
    {
        if (!_actif) return;
        Task.Run(() => { try { Console.Beep(200, 100); Console.Beep(800, 200); } catch { } });
    }

    /// <summary>
    /// Joue un son court et brillant pour le déblocage d'un succès.
    /// </summary>
    public static void Succes()
    {
        if (!_actif) return;
        Task.Run(() =>
        {
            try { Console.Beep(880, 100); Console.Beep(1100, 100); Console.Beep(1320, 200); }
            catch { }
        });
    }
}
