namespace JeuDeRole.Domain.Interfaces;

public interface IObjetUtilisable
{
    string Nom { get; }
    string Description { get; }
    void Utiliser(ICombattant cible);
}
