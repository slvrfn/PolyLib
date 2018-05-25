using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using SkiaSharp;

namespace PolyLib
{
    //interface used for animation update views
    //used by animationengine to ensure proper communication with various animation update view platform implementations
    public interface IAnimationUpdateView
    {
        void DrawOnMe(SKSurface surface);
        void SignalRedraw();
    }
}
