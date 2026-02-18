using JeuDeRole.Domain.Entities;
using JeuDeRole.Factories;
using JeuDeRole.Repositories.Interfaces;

namespace JeuDeRole.Repositories.InMemory;

public class MemoireMonstreRepository : IMonstreRepository
{
    private readonly List<Monstre> _monstres = new();

    public MemoireMonstreRepository(IMonstreFactory monstreFactory)
    {
        _monstres.Add(monstreFactory.CreerMonstre("Gobelin"));
        _monstres.Add(monstreFactory.CreerMonstre("Squelette"));
        _monstres.Add(monstreFactory.CreerMonstre("Loup"));
        _monstres.Add(monstreFactory.CreerMonstre("Orc"));
        _monstres.Add(monstreFactory.CreerMonstre("Dragon"));
    }

    public List<Monstre> ChargerTous()
    {
        return new List<Monstre>(_monstres);
    }

    public Monstre? ChargerParType(string type)
    {
        return _monstres.FirstOrDefault(m => m.Nom == type);
    }
}
