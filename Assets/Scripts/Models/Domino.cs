using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Domino 
{
    public int left;
    public int right;

    public string ownerId;

    public Domino(int l, int r, string owner = null)
    {
        left = l;
        right = r;
        ownerId = owner;
    }

    public Domino Flipped()
    {
        return new Domino(right, left);
    }

    public override string ToString()
    {
        return $"{left}|{right}";
    }

    public bool IsDouble()
    {
        return left == right;
    }

    public bool Contains(int value)
    {
        return left == value || right == value;
    }

}
