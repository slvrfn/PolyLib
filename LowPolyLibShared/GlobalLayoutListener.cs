using System;
using Android.Views;

namespace LowPolyLibrary
{
    public class GlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        readonly Action<GlobalLayoutListener> _onGlobalLayout;

        public GlobalLayoutListener(System.Action<GlobalLayoutListener> onGlobalLayout)
        {
            this._onGlobalLayout = onGlobalLayout;
        }

        public void OnGlobalLayout()
        {
            _onGlobalLayout(this);
        }
    }
}