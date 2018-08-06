using UnityEngine;

[System.Serializable]
public struct PlayerHandicap
{
    public float enegryProduction { get; private set; }
    public float pieceProduction { get; private set; }
    public float hammerProduction { get; private set; }


    public PlayerHandicap(float energy, float piece, float hammer)
    {
        enegryProduction = energy;
        pieceProduction = piece;
        hammerProduction = hammer;
    }

    public void SetEnergyHandicapLevel(float handicap)
    {
        enegryProduction = handicap;
    }

    public void SetPieceHandicapLevel(float handicap)
    {
        pieceProduction = handicap;
    }

    public void SetHammerHandicapLevel(float handicap)
    {
        hammerProduction = handicap;
    }

    public override string ToString()
    {
        return "Energy Production: " + enegryProduction + " | " +
                "Piece Producution: " + pieceProduction + " | " +
                "Hammer Production: " + hammerProduction;
    }
}
