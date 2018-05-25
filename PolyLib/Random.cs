using System;
using System.Collections.Generic;
using System.Text;

namespace PolyLib
{
    class Random
    {
        public static System.Random Rand = new System.Random(System.Guid.NewGuid().GetHashCode());
    }
}
