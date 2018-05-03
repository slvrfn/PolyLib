using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Timers;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Format;
using Android.Views.Animations;
using Android.Views;
using LowPolyLibrary.Animation;
using LowPolyLibrary;
using LowPolyLibrary.Views.Android;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace LowPoly
{
    [Activity(Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
    public class MainActivity : Activity, View.IOnTouchListener, SeekBar.IOnSeekBarChangeListener
    {
        Button button, animSButton, animGButton;
        LowPolyView polyView;
        TextView widthTB, heightTB, varTB, sizeTB;

        LinearLayout controlsContainer;

        SeekBar freqSeek, seedSeek;
        float freqProgress = .01f;
        float seedProgress = 0;

        int numAnimFrames = 12;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            button = FindViewById<Button>(Resource.Id.button1);
            button.Click += UpdatePolyLib;
            animSButton = FindViewById<Button>(Resource.Id.animSButton);
            animSButton.Click += sweepAnimation;
            animGButton = FindViewById<Button>(Resource.Id.animGButton);
            animGButton.Click += growAnimation;

            polyView = FindViewById<LowPolyView>(Resource.Id.imageView1);
            polyView.SetOnTouchListener(this);

            widthTB = FindViewById<TextView>(Resource.Id.widthTextBox);
            heightTB = FindViewById<TextView>(Resource.Id.heightTextBox);
            varTB = FindViewById<TextView>(Resource.Id.varTextBox);
            sizeTB = FindViewById<TextView>(Resource.Id.sizeTextBox);
            seedSeek = FindViewById<SeekBar>(Resource.Id.seedSeek);
            freqSeek = FindViewById<SeekBar>(Resource.Id.frequencySeek);

            controlsContainer = FindViewById<LinearLayout>(Resource.Id.controlsContainer);

            seedSeek.SetOnSeekBarChangeListener(this);
            freqSeek.SetOnSeekBarChangeListener(this);


            var metrics = Resources.DisplayMetrics;

            widthTB.Text = metrics.WidthPixels.ToString();
            heightTB.Text = metrics.HeightPixels.ToString();

            varTB.Text = ".75";
            sizeTB.Text = "150";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            polyView = null;
            widthTB = null;
            heightTB = null;
            varTB = null;
            sizeTB = null;
            controlsContainer = null;
            freqSeek = null;
            freqSeek = null;
        }

        private void UpdatePolyLib(object sender, EventArgs e)
        {
            var boundsWidth = Int32.Parse(widthTB.Text);
            var boundsHeight = Int32.Parse(heightTB.Text);

            var variance = float.Parse(varTB.Text);
            var cellSize = int.Parse(sizeTB.Text);

            polyView = polyView.GenerateNewTriangulation(boundsWidth, boundsHeight, variance, cellSize, freqProgress, seedProgress, this);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            var touch = new SKPoint(e.GetX(), e.GetY());
            bool startAnim = false;
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    startAnim = true;
                    controlsContainer.Visibility = ViewStates.Invisible;
                    break;
                case MotionEventActions.Move:
                    startAnim = true;
                    break;
                case MotionEventActions.Up:
                    controlsContainer.Visibility = ViewStates.Visible;
                    break;
            }

            if (startAnim)
            {
                var touchAnimation = new RandomTouch(polyView.CurrentTriangulation, 12, touch.X, touch.Y, 150);
                polyView.AddAnimation(touchAnimation);
            }

            return true;
        }

        private void growAnimation(object sender, EventArgs e)
        {
            var growAnim = new Grow(polyView.CurrentTriangulation, numAnimFrames);
            polyView.AddAnimation(growAnim);
        }

        private void sweepAnimation(object sender, EventArgs e)
        {
            var sweepAnim = new Sweep(polyView.CurrentTriangulation, numAnimFrames);
            polyView.AddAnimation(sweepAnim);
        }

#region seekbar change listener
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            switch(seekBar.Id)
            {
                case Resource.Id.seedSeek:
                    //keep seedprogress from 0-1000
                    seedProgress = (progress/100f) * 1000;
                    break;
                case Resource.Id.frequencySeek:
                    //keep freqProgress 0-1
                    freqProgress = ((progress+1) / 100f);
                    break;
            }
            UpdatePolyLib(null, null);
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            
        }
#endregion
    }
}


