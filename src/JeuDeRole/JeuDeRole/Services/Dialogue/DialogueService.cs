using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Dialogue;

/// <summary>
/// Implémentation du service Dialogue.
/// Centralise toutes les chaines de caractères narratives et les répliques des PNJ/Boss.
/// Permet de varier les descriptions grâce à des tirages aléatoires.
/// </summary>
public class DialogueService : IDialogueService
{
    private static readonly Random _random = new();

    // Dictionnaire associant un type de boss à une liste de répliques possibles
    private static readonly Dictionary<string, string[]> _dialoguesBoss = new()
    {
        ["Liche"] = new[]
        {
            "« Mortels imprudents... Vous osez pénétrer dans mon domaine ?\nLa mort elle-même est à mes ordres.\nVotre chair nourrira mes légions de morts-vivants ! »",
            "« Je suis la Liche Ancienne. J'ai vu naître et mourir des empires entiers.\nVous n'êtes que poussière face à l'éternité de la non-mort ! »",
            "« Ah... de la chair fraîche. Vos âmes viendront rejoindre ma collection.\nApprochez, et découvrez ce que signifie véritablement la TERREUR ! »"
        },
        ["DragonAncien"] = new[]
        {
            "« GRRROOOOAAAR ! Insectes !\nVous avez le culot de défier un Dragon Ancien ?\nJ'ai réduit des armées en cendres avant même votre naissance ! »",
            "« Je suis le feu et la tempête. La montagne tremble sous mes ailes.\nVotre courage est admirable... mais votre mort sera spectaculaire ! »",
            "« Depuis mille ans je sommeille dans cet antre.\nVous m'avez réveillé... et vous le regretterez amèrement ! »"
        },
        ["GolemCristal"] = new[]
        {
            "« ... CRRRK ... VRRRM ... Les cristaux vibrent. Le Golem s'éveille !\nSa surface reflète mille éclats aveuglants.\nVous n'êtes que des insectes face à la perfection du cristal ! »",
            "« Les anciens m'ont façonné pour garder ce lieu pour l'éternité.\nVous ne passerez pas. Nul ne passe. JAMAIS ! »",
            "« Chaque entaille que vous me faites... je me régénère.\nLe cristal est éternel. Vous ne l'êtes pas ! »"
        },
        ["Hydre"] = new[]
        {
            "« SSSHHHH ! Trois têtes se dressent, crachant un venin corrosif !\nCoupez-en une... deux autres repousseront !\nL'Hydre Venimeuse n'a jamais été vaincue ! »",
            "« Le marais empoisonné est mon domaine. Chaque souffle ici vous affaiblit.\nMes crocs sont imprégnés du poison le plus mortel !\nVenez, nourrir mes trois bouches affamées ! »",
            "« Les héros tombent, les empires s'effondrent, mais l'Hydre perdure.\nTrois esprits, une seule faim insatiable.\nVotre chair sera notre festin ! »"
        },
        ["SeigneurDemon"] = new[]
        {
            "« HAHAHAHA ! Enfin, des âmes dignes de mon attention !\nJe suis le Seigneur Démon, maître des ténèbres absolues !\nVotre monde sera plongé dans l'obscurité éternelle ! »",
            "« Les flammes de l'enfer coulent dans mes veines.\nJ'ai conquis des dimensions entières. Votre petit monde n'est qu'une bouchée.\nÀ genoux, mortels. Implorez ma clémence ! »",
            "« Depuis les abysses, j'ai observé votre progression.\nImpressionnant... pour des mortels. Mais cela s'arrête ICI !\nGoûtez au pouvoir des ténèbres ABSOLUES ! »"
        }
    };

    // Descriptions d'ambiance pour les salles normales (combat)
    private static readonly string[] _narrationsCombat =
    {
        "L'air se fait lourd... Des ombres bougent dans les ténèbres. Vous n'êtes pas seuls.",
        "Un grondement sourd résonne entre les murs de pierre. Des créatures approchent !",
        "Le sol est jonché d'ossements. Un piège ? Non... des habitants.",
        "Une puanteur nauséabonde envahit le couloir. Quelque chose vous attend au tournant.",
        "Des torches vacillent sur les murs. Entre les flammes, des yeux vous observent."
    };

    // Descriptions pour les salles de repos (fontaines, bivouacs)
    private static readonly string[] _narrationsRepos =
    {
        "Une salle paisible baignée d'une douce lumière. Une fontaine cristalline coule en son centre.",
        "Un ancien campement abandonné. Les braises sont encore tièdes.\nUn bon endroit pour reprendre des forces.",
        "Une alcôve cachée derrière une tapisserie. L'endroit semble sûr pour le moment."
    };

    // Descriptions pour les événements spéciaux
    private static readonly string[] _narrationsEvenement =
    {
        "Une salle étrange... Des runes brillent sur les murs. L'air vibre de magie ancienne.",
        "Un carrefour mystérieux. Trois portes s'offrent à vous, mais une seule mène plus loin.",
        "Des inscriptions anciennes couvrent les murs. Quelque chose d'inhabituel se prépare..."
    };

    // Descriptions avant un mini-boss
    private static readonly string[] _narrationsMiniBoss =
    {
        "L'atmosphère change brusquement. Une aura de puissance émane de la salle suivante.\nUn adversaire redoutable vous attend.",
        "Les murs sont marqués de griffures profondes. Quelque chose de GROS vit ici.",
        "Un rugissement fait trembler les murs !\nLe gardien de cet étage ne vous laissera pas passer facilement."
    };

    // Descriptions avant le boss final de l'étage
    private static readonly string[] _narrationsBoss =
    {
        "Les portes massives s'ouvrent dans un grondement.\nAu-delà, une immense caverne... et une présence terrifiante.",
        "Le sol tremble. L'air est chargé d'une énergie sombre.\nVous êtes arrivés au cœur du donjon.",
        "C'est la fin du chemin. Devant vous se dresse l'épreuve ultime.\nPréparez-vous au combat de votre vie !"
    };

    public string ObtenirDialogueBoss(string typeBoss)
    {
        if (_dialoguesBoss.TryGetValue(typeBoss, out var dialogues))
            return dialogues[_random.Next(dialogues.Length)];
        return "« Vous osez m'affronter ? Quelle folie ! »";
    }

    public string ObtenirNarrationEtage(int etage, TypeSalle type)
    {
        var narrations = type switch
        {
            TypeSalle.Combat => _narrationsCombat,
            TypeSalle.Repos => _narrationsRepos,
            TypeSalle.Evenement => _narrationsEvenement,
            TypeSalle.MiniBoss => _narrationsMiniBoss,
            TypeSalle.BossFinal => _narrationsBoss,
            _ => _narrationsCombat
        };
        return narrations[_random.Next(narrations.Length)];
    }

    public string ObtenirIntroArene()
    {
        var intros = new[]
        {
            "Les portes de l'arène s'ouvrent ! La foule rugit !\nCombattez vague après vague pour la gloire éternelle !",
            "Bienvenue dans l'Arène des Champions !\nSeuls les plus forts survivent. Montrez votre valeur !",
            "L'arène vous attend. Sang, sueur et gloire !\nCombien de vagues survivrez-vous ?"
        };
        return intros[_random.Next(intros.Length)];
    }

    public string ObtenirIntroDonjon(int profondeur)
    {
        return $"Vous pénétrez dans les profondeurs d'un donjon ancien.\n{profondeur} étages de dangers vous attendent.\nLes murs suintent d'humidité. L'obscurité est presque totale.\nSeule la lueur de votre torche éclaire le chemin. En avant, héros !";
    }
}
