namespace JeuDeRole.Domain.ValueObjects;

public class Stats
{
    public int PointsDeVieMax { get; init; }
    public int PointsDeManaMax { get; init; }
    public int Force { get; init; }
    public int Intelligence { get; init; }
    public int Agilite { get; init; }
    public int Defense { get; init; }
    public int ResistanceMagique { get; init; }

    public Stats(int pointsDeVieMax, int pointsDeManaMax, int force,
                 int intelligence, int agilite, int defense, int resistanceMagique)
    {
        PointsDeVieMax = pointsDeVieMax;
        PointsDeManaMax = pointsDeManaMax;
        Force = force;
        Intelligence = intelligence;
        Agilite = agilite;
        Defense = defense;
        ResistanceMagique = resistanceMagique;
    }

    public static Stats operator +(Stats a, Stats b)
    {
        return new Stats(
            a.PointsDeVieMax + b.PointsDeVieMax,
            a.PointsDeManaMax + b.PointsDeManaMax,
            a.Force + b.Force,
            a.Intelligence + b.Intelligence,
            a.Agilite + b.Agilite,
            a.Defense + b.Defense,
            a.ResistanceMagique + b.ResistanceMagique
        );
    }

    public static Stats Zero => new(0, 0, 0, 0, 0, 0, 0);
}
