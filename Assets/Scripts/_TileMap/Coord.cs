using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Coord
{
    public int x;
    public int y;

    public Coord(int x_, int y_)
    {
        x = x_;
        y = y_;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        Coord coord = (Coord)obj;
        return (x == coord.x) && (y == coord.y);
    }

    public bool Equals(Coord coord)
    {
        return Equals(coord as object);
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public static bool operator ==(Coord a, Coord b)
    {
        if (ReferenceEquals(a, b)) return true;
        if ((object)a == null || (object)b == null) return false;
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }

    public int Distance(Coord otherCoord)
    {
        return Mathf.Abs(x - otherCoord.x) + Mathf.Abs(y - otherCoord.y);
    }

    public static int Distance(Coord a, Coord b)
    {
        return a.Distance(b);
    }

    public Coord Add(Coord otherCoord)
    {
        return new Coord(x + otherCoord.x, y + otherCoord.y);
    }

    public static Coord Add(Coord a, Coord b)
    {
        return a.Add(b);
    }

    public Coord Multiply(int i)
    {
        return new Coord(x * i, y * i);
    }

    public static Coord Multiply(int i, Coord a)
    {
        return a.Multiply(i);
    }

    public Coord Subtract(Coord otherCoord)
    {
        return Add(otherCoord.Multiply(-1));
    }

    public static Coord Subtract(Coord a, Coord b)
    {
        return a.Subtract(b);
    }

    public Vector3 WorldPos()
    {
        return new Vector3(x, y, 0);
    }

    public static Coord[] Directions()
    {
        Coord[] directions = new Coord[4]
        {
            new Coord(1, 0),
            new Coord(0, 1),
            new Coord(-1, 0),
            new Coord(0, -1)
        };
        return directions;
    }

    public static float DirectionToAngle(Coord direction)
    {
        for (int i = 0; i < 4; i++)
        {
            if (direction == Directions()[i])
            {
                return i * 90;
            }
        }
        return 0;
    }

    public override string ToString()
    {
        return "Tile Coord| X: " + x + ", Y: " + y;
    }
}
