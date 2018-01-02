using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace LowPolyLibrary.Views
{
    class LowPolyView : FrameLayout
    {
        private TriangulationView triView;
        private AnimationUpdateView animView;

        protected LowPolyView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public LowPolyView(Context context) : base(context)
        {
            Init();
        }

        public LowPolyView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public LowPolyView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init();
        }

        private void Init()
        {
            
        }
    }
}
