using System;
using UnityEngine;

[Serializable]
public struct NodeCoord : IEquatable<NodeCoord>, IComparable<NodeCoord>
{
    public int r, c; // 行、列（0..rows-1 / 0..cols-1）
    public NodeCoord(int r, int c) { this.r = r; this.c = c; }
    public bool Equals(NodeCoord other) => r == other.r && c == other.c;
    public override bool Equals(object obj) => obj is NodeCoord o && Equals(o);
    public override int GetHashCode() => (r * 73856093) ^ (c * 19349663);
    public int CompareTo(NodeCoord other) => (r == other.r) ? c.CompareTo(other.c) : r.CompareTo(other.r);
    public static bool operator ==(NodeCoord a, NodeCoord b) => a.Equals(b);
    public static bool operator !=(NodeCoord a, NodeCoord b) => !a.Equals(b);
}

// 无向边（始终存成 a <= b 的有序对，方便 HashSet）
[Serializable]
public struct EdgeCoord : IEquatable<EdgeCoord>
{
    public NodeCoord a, b;
    public EdgeCoord(NodeCoord x, NodeCoord y)
    {
        if (x.CompareTo(y) <= 0) { a = x; b = y; }
        else { a = y; b = x; }
    }
    public bool Equals(EdgeCoord other) => a.Equals(other.a) && b.Equals(other.b);
    public override bool Equals(object obj) => obj is EdgeCoord o && Equals(o);
    public override int GetHashCode() => (a.GetHashCode() * 486187739) ^ b.GetHashCode();
}
