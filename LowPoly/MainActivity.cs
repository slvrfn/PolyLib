using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Timers;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views.Animations;
using Android.Views;
using LowPolyLibrary;
using AnimationBase = LowPolyLibrary.AnimationBase;

namespace LowPoly
{
	[Activity (Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
	public class MainActivity : Activity, View.IOnTouchListener
	{
		Button button, animSButton;
		ImageView imagePanel;
		TextView widthTB, heightTB, varTB, sizeTB, timeElapsed;
	    private LowPolyLibrary.Triangulation _lowPoly;

		AnimationDrawable generatedAnimation;

		protected override void OnCreate (Bundle savedInstanceState)
		{
            base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			button = FindViewById<Button> (Resource.Id.button1);
			button.Click += Generate;
			animSButton = FindViewById<Button>(Resource.Id.animSButton);
			animSButton.Click += stepAnimation;


			imagePanel = FindViewById<ImageView> (Resource.Id.imageView1);
			imagePanel.SetOnTouchListener(this);


			widthTB = FindViewById<TextView> (Resource.Id.widthTextBox);
			heightTB = FindViewById<TextView> (Resource.Id.heightTextBox);
			varTB = FindViewById<TextView> (Resource.Id.varTextBox);
			sizeTB = FindViewById<TextView> (Resource.Id.sizeTextBox);
            timeElapsed = FindViewById<TextView>(Resource.Id.timeElapsed);

           

            var metrics = Resources.DisplayMetrics;

			//widthTB.Text = metrics.WidthPixels.ToString();
			//heightTB.Text = metrics.HeightPixels.ToString();
			widthTB.Text = "1080";
			heightTB.Text = "1920";
		    varTB.Text = ".75";
		    sizeTB.Text = "150";

		    //_lowPoly.test();
		}

		private void UpdatePolyLib()
		{
            var boundsWidth = Int32.Parse(widthTB.Text);
			var boundsHeight = Int32.Parse(heightTB.Text);

			var variance = double.Parse(varTB.Text);
			var cellSize = double.Parse(sizeTB.Text);

            _lowPoly = new LowPolyLibrary.Triangulation(boundsWidth,boundsHeight,variance, cellSize);
        }

		public void Generate (object sender, EventArgs e){
            var temp = new Stopwatch();

			UpdatePolyLib();
            
            temp.Start();
		    var generatedBitmap = _lowPoly.GeneratedBitmap;
            temp.Stop();

            imagePanel.SetImageDrawable (new BitmapDrawable (generatedBitmap));
            
		    timeElapsed.Text = temp.Elapsed.ToString();
			generatedAnimation = null;
		}

		public void uuuu(AnimationTypes.Type anim, System.Drawing.PointF touch)
		{
			var temp = new Stopwatch();

			temp.Start();
			if (generatedAnimation == null)
			{
			    switch (anim)
			    {
                    case AnimationTypes.Type.Grow:
                        //generatedAnimation = _lowPoly.makeAnimation(anim, 12, touch.X, touch.Y, 50);
                        var g = new Grow(_lowPoly);
			            generatedAnimation = g.Animation;
                        break;
                    case AnimationTypes.Type.Sweep:
                        var s = new Sweep(_lowPoly);
			            generatedAnimation = s.Animation;
			            break;
                    case AnimationTypes.Type.Touch:
                        var t = new Touch(_lowPoly,touch.X, touch.Y, 200);
			            generatedAnimation = t.Animation;
			            break;
			    }

                //generatedAnimation = _lowPoly.makeAnimation(anim, 12, touch.X, touch.Y, 50);
                imagePanel.SetImageDrawable(generatedAnimation);
			}
			else
			{
				generatedAnimation.Stop();
			}
			temp.Stop();
			generatedAnimation.Start();

			timeElapsed.Text = temp.Elapsed.ToString();
		}

		public void stepAnimation(object sender, EventArgs e)
		{
			uuuu(AnimationTypes.Type.Sweep, new System.Drawing.PointF(0, 0));
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			if (e.Action == MotionEventActions.Down)
			{
				var touch = new System.Drawing.PointF();
				touch.X = e.GetX();
				touch.Y = e.GetY();
                //COMMENTED TEMPORARILY 10/10
				//_lowPoly.setPointsaroundTouch(touch, 200);
				generatedAnimation = null;
				uuuu(AnimationTypes.Type.Touch, touch);
				return true;
			}
			if (e.Action == MotionEventActions.Up)
			{
				// do other stuff
				return true;
			}

			return false;
		}
	}
}


