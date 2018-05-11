using System;
using DelaunayTriangulator;


namespace LowPolyLibrary
{
    public static class ExtensionMethods
    {
        //Create clone method for Vertex
        public static Vertex Clone(this Vertex v)
        {
            return new Vertex(v.x, v.y);
        }

        //Create clone method for Vertex
        public static Triad Clone(this Triad t)
        {
            return new Triad(t.a, t.b, t.c)
            {
                ab = t.ab,
                bc = t.bc,
                ac = t.ac,
                circumcircleX = t.circumcircleX,
                circumcircleY = t.circumcircleY,
                circumcircleR2 = t.circumcircleR2
            };
        }
    }
}
