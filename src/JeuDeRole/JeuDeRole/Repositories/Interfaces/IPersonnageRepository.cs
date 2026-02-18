using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Repositories.Interfaces;

public interface IPersonnageRepository
{
    void Sauvegarder(Heros heros);
    Heros? Charger(string nom);
    List<Heros> ChargerTous();
}
