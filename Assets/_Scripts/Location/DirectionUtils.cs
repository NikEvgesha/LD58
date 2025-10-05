using System;

[Flags]
public enum Openings
{
    None  = 0,
    North = 1 << 0,
    East  = 1 << 1,
    South = 1 << 2,
    West  = 1 << 3,
}

public static class DirectionUtils
{
    public static Openings Rotate(Openings mask, int quarterTurnsCW)
    {
        quarterTurnsCW = ((quarterTurnsCW % 4) + 4) % 4;
        for (int i = 0; i < quarterTurnsCW; i++)
        {
            Openings rotated = Openings.None;
            if (mask.HasFlag(Openings.North)) rotated |= Openings.East;
            if (mask.HasFlag(Openings.East))  rotated |= Openings.South;
            if (mask.HasFlag(Openings.South)) rotated |= Openings.West;
            if (mask.HasFlag(Openings.West))  rotated |= Openings.North;
            mask = rotated;
        }
        return mask;
    }

    public static Openings Opposite(Openings dir)
    {
        Openings opp = Openings.None;
        if (dir.HasFlag(Openings.North)) opp |= Openings.South;
        if (dir.HasFlag(Openings.East))  opp |= Openings.West;
        if (dir.HasFlag(Openings.South)) opp |= Openings.North;
        if (dir.HasFlag(Openings.West))  opp |= Openings.East;
        return opp;
    }
}
