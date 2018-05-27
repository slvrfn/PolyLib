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
using PolyLib.Animation;
using PolyLib;
using PolyLib.Views.Android;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace PolyLibAndroid
{
    [Activity(Label = "PolyLib", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
    public class MainActivity : Activity, View.IOnTouchListener, SeekBar.IOnSeekBarChangeListener
    {
        Button _button, _animSButton, _animGButton;
        PolyLibView _polyLibView;
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

            _polyLibView = FindViewById<PolyLibView>(Resource.Id.imageView1);
            _polyLibView.SetOnTouchListener(this);

            _currentTriangulation = _polyLibView.CurrentTriangulation;

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
            _polyLibView = null;
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
                _currentTriangulation = _polyLibView.CurrentTriangulation;
            }

            var boundsWidth = Int32.Parse(_widthTB.Text);
            var boundsHeight = Int32.Parse(_heightTB.Text);

            var variance = float.Parse(_varTB.Text);
            var cellSize = int.Parse(_sizeTB.Text);

            if (!boundsWidth.Equals(_polyLibView.Width) || !boundsHeight.Equals(_polyLibView.Height))
            {
                _polyLibView = _polyLibView.ResizeView(boundsWidth, boundsHeight, this);
                _currentTriangulation = _polyLibView.CurrentTriangulation;
            }
            //set props on triangulation
            _currentTriangulation.Variance = variance;
            _currentTriangulation.CellSize = cellSize;
            _currentTriangulation.Frequency = _freqProgress;
            _currentTriangulation.Seed = _seedProgress;

            var colors = Triangulation.getRandomColorBruColors(6);
            var shader = Triangulation.GetRandomGradientShader(colors, _currentTriangulation.BoundsWidth,
                _currentTriangulation.BoundsHeight);

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
                var touchAnimation = new RandomTouch(_polyLibView.CurrentTriangulation, 12, touch.X, touch.Y, 150);
                _polyLibView.AddAnimation(touchAnimation);
            }

            return true;
        }

        private void growAnimation(object sender, EventArgs e)
        {
            var growAnim = new Grow(_polyLibView.CurrentTriangulation, _numAnimFrames);
            _polyLibView.AddAnimation(growAnim);
        }

        private void sweepAnimation(object sender, EventArgs e)
        {
            var sweepAnim = new Sweep(_polyLibView.CurrentTriangulation, _numAnimFrames);
            _polyLibView.AddAnimation(sweepAnim);
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


