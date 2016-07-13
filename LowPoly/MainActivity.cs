using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Timers;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace LowPoly
{
	[Activity (Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		Button button, animButton;
		ImageView imagePanel;
		TextView widthTB, heightTB, varTB, sizeTB, timeElapsed;
		LowPolyLibrary.LowPolyLib _lowPoly = new LowPolyLibrary.LowPolyLib ();

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

			widthTB = FindViewById<TextView> (Resource.Id.widthTextBox);
			heightTB = FindViewById<TextView> (Resource.Id.heightTextBox);
			varTB = FindViewById<TextView> (Resource.Id.varTextBox);
			sizeTB = FindViewById<TextView> (Resource.Id.sizeTextBox);
            timeElapsed = FindViewById<TextView>(Resource.Id.timeElapsed);

            widthTB.Text = "1024";
			heightTB.Text = "768";
			varTB.Text = _lowPoly.setVariance.ToString ();
			sizeTB.Text = _lowPoly.cell_size.ToString ();



		}

		public void Generate (object sender, EventArgs e){
            var temp = new Stopwatch();
            temp.Start();
            _lowPoly.boundsWidth = Int32.Parse (widthTB.Text);
			_lowPoly.boundsHeight = Int32.Parse (heightTB.Text);

			_lowPoly.setVariance = double.Parse(varTB.Text);
			_lowPoly.cell_size = double.Parse(sizeTB.Text);

            temp.Start();
            var generatedBitmap = _lowPoly.GenerateNew();
            temp.Stop();

            imagePanel.SetImageDrawable (new BitmapDrawable (generatedBitmap));
            
		    timeElapsed.Text = temp.Elapsed.ToString();


		}

		public void stepAnimation(object sender, EventArgs e)
		{
			var temp = new Stopwatch();


			temp.Start();
			var generatedBitmap = _lowPoly.createAnimBitmap(1);
			temp.Stop();

			imagePanel.SetImageDrawable(new BitmapDrawable(generatedBitmap));

			timeElapsed.Text = temp.Elapsed.ToString();


		}
	}
}


