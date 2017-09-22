using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector2
{
    public static IntVector2 DOWN = new IntVector2(0, -1);
    public static IntVector2 LEFT = new IntVector2(-1, 0);
    public static IntVector2 ONE = new IntVector2(1, 1);
    public static IntVector2 RIGHT = new IntVector2(1, 0);
    public static IntVector2 UP = new IntVector2(0, 1);
    public static IntVector2 ZERO = new IntVector2(0, 0);
    public static IntVector2 NEGATIVE_INFINITY = new IntVector2(int.MinValue, int.MinValue);
    public static IntVector2 POSITIVE_INFINITY = new IntVector2(int.MaxValue, int.MaxValue);

	public int x;
	public int y;

    //public readonly int magnitude;
    //public readonly int normalized;

	public IntVector2 (int x_,int y_)
    {
		x = x_;
		y = y_;
	}

    public override bool Equals(object otherObj)
    {
        if (otherObj == null || GetType() != otherObj.GetType())
        {
            return false;
        }
        IntVector2 otherIntVector = (IntVector2)otherObj;
        return (x == otherIntVector.x) && (y == otherIntVector.y);
    }

    public bool Equals(IntVector2 otherVec)
    {
        return Equals(otherVec);
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public static bool operator ==(IntVector2 a, IntVector2 b)
    {
        if (ReferenceEquals(a, b)) return true;
        if ((object)a == null || (object)b == null) return false;
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(IntVector2 a, IntVector2 b)
    {
        return !(a == b);
    }

    public int Distance(IntVector2 otherVec)
    {
        return Mathf.Abs(x - otherVec.x) + Mathf.Abs(y - otherVec.y);
    }

    public static int Distance(IntVector2 a, IntVector2 b)
    {
        return a.Distance(b);
    }

    public IntVector2 Add(IntVector2 otherVec)
    {
        return new IntVector2(x + otherVec.x, y + otherVec.y);
    }

    public static IntVector2 Add(IntVector2 a, IntVector2 b)
    {
        return a.Add(b);
    }

    public IntVector2 Multiply(int i)
    {
        return new IntVector2(x * i, y * i);
    }

    public static IntVector2 Multiply(int i, IntVector2 a)
    {
        return a.Multiply(i);
    }

    public IntVector2 Subtract(IntVector2 otherVec)
    {
        return Add(otherVec.Multiply(-1));
    }

    public static IntVector2 Subtract(IntVector2 a, IntVector2 b)
    {
        return a.Subtract(b);
    }
}
