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
    public class MainActivity : Activity, View.IOnTouchListener
    {
        LowPolyView _polyView;

        Triangulation _currentTriangulation;

        int _numAnimFrames = 12;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _polyView = FindViewById<LowPolyView>(Resource.Id.imageView1);
            _polyView.SetOnTouchListener(this);

            _currentTriangulation = _polyView.CurrentTriangulation;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _polyView = null;
        }

        //private void UpdatePolyLib(object sender, EventArgs e)
        //{
        //    if (_currentTriangulation == null)
        //    {
        //        _currentTriangulation = _polyView.CurrentTriangulation;
        //    }

        //    var colors = Triangulation.getRandomColorBruColors(6);
        //    var shader = Triangulation.GetRandomGradientShader(colors, _currentTriangulation.BoundsWidth,
        //        _currentTriangulation.BoundsHeight);

        //    _currentTriangulation.GradientColors = colors;
        //    _currentTriangulation.GradientShader = shader;
        //}

        public bool OnTouch(View v, MotionEvent e)
        {
            if (_currentTriangulation == null)
            {
                _currentTriangulation = _polyView.CurrentTriangulation;
            }

            var touch = new SKPoint(e.GetX(), e.GetY());
            bool startAnim = false;
            switch (e.Action)
            {
                case MotionEventActions.Cancel:
                    break;
                case MotionEventActions.Down:
                    startAnim = true;
                    break;
                case MotionEventActions.Move:
                    startAnim = true;
                    break;
                case MotionEventActions.Up:
                    //var colors = Triangulation.getRandomColorBruColors(6);

                    //foreach (var colCode in colors)
                    //{
                    //    Console.WriteLine(colCode.ToString());
                    //}

                    var col = new[]{
                        "#ffd53e4f",
                        "#fffc8d59",
                        "#fffee08b",
                        "#ffe6f598",
                        "#ff99d594",
                        "#ff3288bd",
                    };

                    var colors = new SKColor[col.Length];

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = SKColor.Parse(col[i]);
                    }

                    var gradientShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(_currentTriangulation.BoundsWidth, _currentTriangulation.BoundsHeight),
                        colors,
                        null, //spread colors evenly
                        SKShaderTileMode.Repeat
                    );

                    _currentTriangulation.GradientShader = gradientShader;

                    break;
            }

            if (startAnim)
            {
                var touchAnimation = new RandomTouch(_polyView.CurrentTriangulation, 8, touch.X, touch.Y, 150);
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
    }
}


