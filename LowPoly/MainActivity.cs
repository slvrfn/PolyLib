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

namespace LowPoly
{
	[Activity (Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
	public class MainActivity : Activity, View.IOnTouchListener
	{
		Button button, animButton;
		ImageView imagePanel;
		TextView widthTB, heightTB, varTB, sizeTB, timeElapsed;
		LowPolyLibrary.LowPolyLib _lowPoly = new LowPolyLibrary.LowPolyLib ();

		AnimationDrawable generatedAnimation;

		protected override void OnCreate (Bundle savedInstanceState)
		{
//			MotionEvent t;

			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			button = FindViewById<Button> (Resource.Id.button1);
			button.Click += Generate;
			animButton = FindViewById<Button>(Resource.Id.animButton);
			animButton.Click += stepAnimation;

			imagePanel = FindViewById<ImageView> (Resource.Id.imageView1);
			imagePanel.SetOnTouchListener(this);


			widthTB = FindViewById<TextView> (Resource.Id.widthTextBox);
			heightTB = FindViewById<TextView> (Resource.Id.heightTextBox);
			varTB = FindViewById<TextView> (Resource.Id.varTextBox);
			sizeTB = FindViewById<TextView> (Resource.Id.sizeTextBox);
            timeElapsed = FindViewById<TextView>(Resource.Id.timeElapsed);

			var metrics = Resources.DisplayMetrics;

			widthTB.Text = metrics.WidthPixels.ToString();
			heightTB.Text = metrics.HeightPixels.ToString();
			varTB.Text = _lowPoly.setVariance.ToString ();
			sizeTB.Text = _lowPoly.cell_size.ToString ();
		}

		private void updatePolyLib()
		{
			_lowPoly.boundsWidth = Int32.Parse(widthTB.Text);
			_lowPoly.boundsHeight = Int32.Parse(heightTB.Text);

			_lowPoly.setVariance = double.Parse(varTB.Text);
			_lowPoly.cell_size = double.Parse(sizeTB.Text);
		}

		public void Generate (object sender, EventArgs e){
            var temp = new Stopwatch();

			updatePolyLib();

            temp.Start();
            var generatedBitmap = _lowPoly.GenerateNew();
            temp.Stop();

            imagePanel.SetImageDrawable (new BitmapDrawable (generatedBitmap));
            
		    timeElapsed.Text = temp.Elapsed.ToString();
			generatedAnimation = null;
		}

		public void stepAnimation(object sender, EventArgs e)
		{
			var temp = new Stopwatch();

   //         temp.Start();
		 //   var generatedBitmap = _lowPoly.createSweepAnimBitmap(frameNum++);
			//temp.Stop();

            temp.Start();
		    if (generatedAnimation == null)
		    {
                generatedAnimation = _lowPoly.makeAnimation(12);
                imagePanel.SetImageDrawable(generatedAnimation);
            }
		    else
		    {
		        generatedAnimation.Stop();
		    }
            temp.Stop();
            //imagePanel.SetImageDrawable(new BitmapDrawable(generatedBitmap));
            //imagePanel.Background = generatedAnimation;
            generatedAnimation.Start();

            timeElapsed.Text = temp.Elapsed.ToString();
		}

		public bool OnTouch(View v, MotionEvent e)
		{
			if (e.Action == MotionEventActions.Down)
			{
				var touchX = e.GetX();
				var touchY = e.GetY();
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


