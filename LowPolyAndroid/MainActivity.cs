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

namespace LowPolyAndroid
{
    [Activity(Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
    public class MainActivity : Activity, View.IOnTouchListener, SeekBar.IOnSeekBarChangeListener
    {
        Button _button, _animSButton, _animGButton;
        LowPolyView _polyView;
        TextView _widthTB, _heightTB, _varTB, _sizeTB;

        Triangulation _currentTriangulation;

        LinearLayout _controlsContainer;

        SeekBar _freqSeek, _seedSeek;
        float _freqProgress, _seedProgress;

        int _numAnimFrames = 12;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            _button = FindViewById<Button>(Resource.Id.button1);
            _button.Click += UpdatePolyLib;
            _animSButton = FindViewById<Button>(Resource.Id.animSButton);
            _animSButton.Click += sweepAnimation;
            _animGButton = FindViewById<Button>(Resource.Id.animGButton);
            _animGButton.Click += growAnimation;

            _polyView = FindViewById<LowPolyView>(Resource.Id.imageView1);
            _polyView.SetOnTouchListener(this);

            _currentTriangulation = _polyView.CurrentTriangulation;

            _widthTB = FindViewById<TextView>(Resource.Id.widthTextBox);
            _heightTB = FindViewById<TextView>(Resource.Id.heightTextBox);
            _varTB = FindViewById<TextView>(Resource.Id.varTextBox);
            _sizeTB = FindViewById<TextView>(Resource.Id.sizeTextBox);
            _seedSeek = FindViewById<SeekBar>(Resource.Id.seedSeek);
            _freqSeek = FindViewById<SeekBar>(Resource.Id.frequencySeek);

            _controlsContainer = FindViewById<LinearLayout>(Resource.Id.controlsContainer);

            _seedSeek.SetOnSeekBarChangeListener(this);
            _freqSeek.SetOnSeekBarChangeListener(this);


            var metrics = Resources.DisplayMetrics;

            _widthTB.Text = metrics.WidthPixels.ToString();
            _heightTB.Text = metrics.HeightPixels.ToString();

            _varTB.Text = ".75";
            _sizeTB.Text = "150";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _polyView = null;
            _widthTB = null;
            _heightTB = null;
            _varTB = null;
            _sizeTB = null;
            _controlsContainer = null;
            _freqSeek = null;
            _freqSeek = null;
        }

        private void UpdatePolyLib(object sender, EventArgs e)
        {
            if (_currentTriangulation == null)
            {
                _currentTriangulation = _polyView.CurrentTriangulation;
            }

            var boundsWidth = Int32.Parse(_widthTB.Text);
            var boundsHeight = Int32.Parse(_heightTB.Text);

            var variance = float.Parse(_varTB.Text);
            var cellSize = int.Parse(_sizeTB.Text);

            if (!boundsWidth.Equals(_polyView.Width) || !boundsHeight.Equals(_polyView.Height))
            {
                _polyView = _polyView.ResizeView(boundsWidth, boundsHeight, this);
                _currentTriangulation = _polyView.CurrentTriangulation;
            }
            //set props on triangulation
            _currentTriangulation.Variance = variance;
            _currentTriangulation.CellSize = cellSize;
            _currentTriangulation.Frequency = _freqProgress;
            _currentTriangulation.Seed = _seedProgress;

            var colors = Triangulation.getRandomColorBruColors(6);
            var shader = Triangulation.GetRandomGradientShader(colors, _currentTriangulation.BoundsWidth,
                _currentTriangulation.BoundsHeight);

            _currentTriangulation.GradientColors = colors;
            _currentTriangulation.GradientShader = shader;
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
                    _controlsContainer.Visibility = ViewStates.Invisible;
                    break;
                case MotionEventActions.Move:
                    startAnim = true;
                    break;
                case MotionEventActions.Up:
                    _controlsContainer.Visibility = ViewStates.Visible;
                    break;
            }

            if (startAnim)
            {
                var touchAnimation = new RandomTouch(_polyView.CurrentTriangulation, 12, touch.X, touch.Y, 150);
                _polyView.AddAnimation(touchAnimation);
            }

            return true;
        }

        private void growAnimation(object sender, EventArgs e)
        {
            var growAnim = new Grow(_polyView.CurrentTriangulation, _numAnimFrames);
            _polyView.AddAnimation(growAnim);
        }

        private void sweepAnimation(object sender, EventArgs e)
        {
            var sweepAnim = new Sweep(_polyView.CurrentTriangulation, _numAnimFrames);
            _polyView.AddAnimation(sweepAnim);
        }

        #region seekbar change listener
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            switch (seekBar.Id)
            {
                case Resource.Id.seedSeek:
                    //keep seedprogress from 0-1000
                    _seedProgress = (progress / 100f) * 1000;
                    break;
                case Resource.Id.frequencySeek:
                    //keep freqProgress 0-1
                    _freqProgress = ((progress + 1) / 100f);
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


