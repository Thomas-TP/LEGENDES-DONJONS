using JeuDeRole.Domain.Entities;
using JeuDeRole.Repositories.Interfaces;

namespace JeuDeRole.Repositories.InMemory;

public class MemoirePersonnageRepository : IPersonnageRepository
{
    private readonly List<Heros> _heros = new();

    public void Sauvegarder(Heros heros)
    {
        var existant = _heros.FindIndex(h => h.Nom == heros.Nom);
        if (existant >= 0)
            _heros[existant] = heros;
        else
            _heros.Add(heros);
    }

    public Heros? Charger(string nom)
    {
        return _heros.FirstOrDefault(h => h.Nom == nom);
    }

    public List<Heros> ChargerTous()
    {
        return new List<Heros>(_heros);
    }
}
