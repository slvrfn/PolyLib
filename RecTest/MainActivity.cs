using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

namespace RecTest
{
    [Activity(Label = "RecTest", MainLauncher = true)]
    public class MainActivity : Activity, SeekBar.IOnSeekBarChangeListener, View.IOnTouchListener
    {
        RectangleView recView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            recView = (RectangleView)FindViewById(Resource.Id.recView);
            var seekbar = (SeekBar) FindViewById(Resource.Id.angleSeekBar);

            seekbar.SetOnSeekBarChangeListener(this);
            recView.SetOnTouchListener(this);
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            recView.SetAngle(progress);
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            recView.setTouchLocation(e.GetX(), e.GetY());

            return true;
        }
    }
}

