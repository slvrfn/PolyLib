using System;
using DelaunayTriangulator;


namespace PolyLib
{
    public static class ExtensionMethods
    {
        #region DelaunayTriangulator.Triad

        /// <summary>
        /// Create clone method for Triad
        /// </summary>
        /// <param name="t">A Triad object t.</param>
        /// <returns>A copy of the Triad t</returns>
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

        #endregion

        #region DelaunayTriangulator.Vertex

        /// <summary>
        /// Create clone method for Vertex
        /// </summary>
        /// <param name="v">A Vertex object v.</param>
        /// <returns>A copy of the Vertex v</returns>
        public static Vertex Clone(this Vertex v)
        {
            return new Vertex(v.x, v.y);
        }

        #endregion
    }
}
