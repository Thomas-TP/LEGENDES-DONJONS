using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.UI;
using Spectre.Console;

namespace JeuDeRole.Logging;

public class ConsoleLogger : ICombatLogger
{
    public void LogAction(string message)
    {
        AnsiConsole.MarkupLine($"  [white]► {Markup.Escape(message)}[/]");
        Thread.Sleep(250);
    }

    public void LogDegats(ICombattant source, ICombattant cible, int degats)
    {
        SoundService.Degats();
        AnsiConsole.MarkupLine($"  [red]⚔ {Markup.Escape(source.Nom)} inflige {degats} dégâts à {Markup.Escape(cible.Nom)} ![/] [grey](PV: {cible.PointsDeVie})[/]");
        Thread.Sleep(350);
    }

    public void LogSoin(ICombattant source, ICombattant cible, int montant)
    {
        SoundService.Soin();
        AnsiConsole.MarkupLine($"  [green]✚ {Markup.Escape(source.Nom)} soigne {Markup.Escape(cible.Nom)} de {montant} PV ![/] [grey](PV: {cible.PointsDeVie})[/]");
        Thread.Sleep(300);
    }

    public void LogMort(ICombattant combattant)
    {
        SoundService.Mort();
        AnsiConsole.MarkupLine($"  [darkred]✝ {Markup.Escape(combattant.Nom)} est vaincu ![/]");
        Thread.Sleep(500);
    }

    public void LogStatut(ICombattant cible, StatutEffet statut)
    {
        string couleur = statut switch
        {
            StatutEffet.Poison => "green",
            StatutEffet.Brulure => "orangered1",
            StatutEffet.Gel => "aqua",
            StatutEffet.Paralysie => "yellow",
            StatutEffet.Sommeil => "mediumpurple2",
            StatutEffet.BuffAttaque => "gold1",
            StatutEffet.DebuffDefense => "grey",
            _ => "magenta"
        };
        AnsiConsole.MarkupLine($"  [{couleur}]☠ {Markup.Escape(cible.Nom)} est affecté par : {statut}[/]");
        Thread.Sleep(250);
    }

    public void LogDebutTour(int numeroTour)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[cyan]TOUR {numeroTour}[/]").RuleStyle("cyan"));
    }

    public void LogFinCombat(bool victoireHeros)
    {
        AnsiConsole.WriteLine();
        if (victoireHeros)
        {
            SoundService.Victoire();
            var panel = new Panel("[bold yellow]🏆 VICTOIRE DES HÉROS ![/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Yellow)
                .Padding(2, 1);
            AnsiConsole.Write(panel);
        }
        else
        {
            SoundService.Defaite();
            var panel = new Panel("[bold darkred]💀 DÉFAITE...[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.DarkRed)
                .Padding(2, 1);
            AnsiConsole.Write(panel);
        }
    }

    public void LogExperience(string nomHeros, int xp, int niveauActuel, bool levelUp)
    {
        AnsiConsole.MarkupLine($"  [gold1]★ {Markup.Escape(nomHeros)} gagne {xp} XP ![/]");
        if (levelUp)
        {
            SoundService.LevelUp();
            AnsiConsole.MarkupLine($"  [bold gold1]⬆ {Markup.Escape(nomHeros)} monte au niveau {niveauActuel} ![/]");
        }
    }

    public void LogPhaseChangement(string nomBoss, int phase, string nomPhase)
    {
        SoundService.PhaseChange();
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold red]⚠ {Markup.Escape(nomBoss)} entre en {Markup.Escape(nomPhase)} ![/]").RuleStyle("red"));
    }

    public void LogDebutActionMonstre(string nomMonstre)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[red bold]Tour de {Markup.Escape(nomMonstre)}[/]").RuleStyle("darkred"));
        Thread.Sleep(400);
    }

    public void LogActionMonstre(string nomMonstre, string nomCompetence, string nomCible, bool estAoE)
    {
        AnsiConsole.Markup($"  [darkred]⚔ {Markup.Escape(nomMonstre)} prépare...[/]");
        Thread.Sleep(500);

        if (estAoE)
            AnsiConsole.MarkupLine($" [bold red]{Markup.Escape(nomCompetence)}[/] [grey]→ sur toute l'équipe ![/]");
        else
            AnsiConsole.MarkupLine($" [bold red]{Markup.Escape(nomCompetence)}[/] [grey]→ sur[/] [bold white]{Markup.Escape(nomCible)}[/] [grey]![/]");

        Thread.Sleep(300);
    }

    public void LogDefense(string nomCombattant)
    {
        AnsiConsole.MarkupLine($"  [blue]🛡 {Markup.Escape(nomCombattant)} se met en défense.[/]");
        Thread.Sleep(200);
    }

    public void LogCritique(ICombattant source, ICombattant cible, int degats)
    {
        SoundService.AttaqueCritique();
        AnsiConsole.MarkupLine($"  [bold yellow]💥 COUP CRITIQUE ! {Markup.Escape(source.Nom)} inflige {degats} dégâts à {Markup.Escape(cible.Nom)} ![/] [grey](PV: {cible.PointsDeVie})[/]");
        Thread.Sleep(500);
    }

    public void LogEsquive(ICombattant cible)
    {
        AnsiConsole.MarkupLine($"  [italic aqua]💨 {Markup.Escape(cible.Nom)} esquive l'attaque ![/]");
        Thread.Sleep(350);
    }

    public void LogElement(Element element, double multiplicateur)
    {
        if (element == Element.Neutre || Math.Abs(multiplicateur - 1.0) < 0.01) return;

        string emoji = AsciiArt.ObtenirEmoji(element);
        string couleur = AsciiArt.ObtenirCouleur(element);

        if (multiplicateur > 1.0)
            AnsiConsole.MarkupLine($"  [{couleur}]{emoji} Super efficace ! (x{multiplicateur:F1})[/]");
        else
            AnsiConsole.MarkupLine($"  [{couleur}]{emoji} Peu efficace... (x{multiplicateur:F1})[/]");

        Thread.Sleep(200);
    }
}
