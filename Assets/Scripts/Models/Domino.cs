using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Domino 
{
    public int left;
    public int right;

    public Domino(int l, int r)
    {
        left = l;
        right = r;
    }

    public Domino Flipped()
    {
        return new Domino(right, left);
    }

    public override string ToString()
    {
        return $"{left}|{right}";
    }
}
