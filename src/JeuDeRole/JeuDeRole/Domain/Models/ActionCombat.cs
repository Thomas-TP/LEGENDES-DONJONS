using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Domain.Models;

public class ActionCombat
{
    public ICombattant Source { get; }
    public ICompetence? Competence { get; }
    public List<ICombattant> Cibles { get; }
    public bool EstDefense { get; }
    public ObjetAction? Objet { get; }

    private ActionCombat(ICombattant source, ICompetence? competence,
                         List<ICombattant> cibles, bool estDefense, ObjetAction? objet)
    {
        Source = source;
        Competence = competence;
        Cibles = cibles;
        EstDefense = estDefense;
        Objet = objet;
    }

    public static ActionCombat Attaquer(ICombattant source, ICompetence competence, List<ICombattant> cibles)
        => new(source, competence, cibles, false, null);

    public static ActionCombat Defendre(ICombattant source)
        => new(source, null, new List<ICombattant>(), true, null);

    public static ActionCombat UtiliserObjet(ICombattant source, Entities.ObjetConsommable objet, ICombattant cible)
        => new(source, null, new List<ICombattant> { cible }, false, new ObjetAction(objet));
}

public class ObjetAction
{
    public Entities.ObjetConsommable Objet { get; }

    public ObjetAction(Entities.ObjetConsommable objet)
    {
        Objet = objet;
    }
}
