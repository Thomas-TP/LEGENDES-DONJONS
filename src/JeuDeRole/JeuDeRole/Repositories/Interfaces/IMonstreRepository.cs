using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Repositories.Interfaces;

public interface IMonstreRepository
{
    List<Monstre> ChargerTous();
    Monstre? ChargerParType(string type);
}
