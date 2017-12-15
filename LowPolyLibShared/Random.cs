using System;
using System.Collections.Generic;
using System.Text;
using Java.Util;

namespace LowPolyLibrary
{
    class Random
    {
        public static System.Random Rand = new System.Random(UUID.RandomUUID().GetHashCode());
    }
}
