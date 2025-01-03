﻿using Puzzles.Base.Entites;

namespace Puzzles.Base;
public static class AOC
{
    private static readonly ulong[] digitsDividers;

    static AOC()
        => digitsDividers = InitDividers();

    public static int GetDigits(ulong number)
        => (int)(Math.Log10(number) + 1);

    public static (ulong left, ulong right) SplitUInt64(ulong number, int size)
        => (number / digitsDividers[size], number % digitsDividers[size]);

    public static int Mod(int n, int m)
        => ((n % m) + m) % m;

    public static int ModInv(int n, int m)
    {
        int t = 0, nt = 1;
        int r = m, nr = n;

        while (nr != 0)
        {
            int q = r / nr;
            (r, nr) = (nr, r - q * nr);
            (t, nt) = (nt, t - q * nt);
        }

        return Mod(t, m);
    }

    public static void Sort2(ref int a, ref int b)
    {
        if (a > b)
            (b, a) = (a, b);
    }

    public static void Sort3(ref int a, ref int b, ref int c)
    {
        Sort2(ref a, ref b);
        Sort2(ref a, ref c);
        Sort2(ref b, ref c);
    }

    public static int ManhattanDistance(int ax, int ay, int bx, int by)
        => Math.Abs(ax - bx) + Math.Abs(ay - by);

    public static int ManhattanDistance(Vec2 from, Vec2 to)
        => ManhattanDistance(from.X, from.Y, to.X, to.Y);

    #region Private methods

    private static ulong[] InitDividers()
    {
        var ulongDigits = GetDigits(ulong.MaxValue);
        var digitsDividers = new ulong[ulongDigits];

        digitsDividers[0] = 0UL;
        digitsDividers[1] = 10UL;

        for (int i = 2; i < ulongDigits; i++)
            digitsDividers[i] = digitsDividers[i - 1] * 10UL;

        return digitsDividers;
    }

    #endregion
}
