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
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace LowPoly
{
	[Activity (Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
	public class MainActivity : Activity
	{
        Button button, animSButton, animGButton;
	    CustomCanvasView imagePanel;
		TextView widthTB, heightTB, varTB, sizeTB;
	    

		

		protected override void OnCreate (Bundle savedInstanceState)
		{
            base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			button = FindViewById<Button> (Resource.Id.button1);
			button.Click += UpdatePolyLib;
			animSButton = FindViewById<Button>(Resource.Id.animSButton);
			animSButton.Click += sweepAnimation;
		    animGButton = FindViewById<Button>(Resource.Id.animGButton);
		    animGButton.Click += growAnimation;
		    
			imagePanel = FindViewById<CustomCanvasView> (Resource.Id.imageView1);
			//imagePanel.SetOnTouchListener(this);

			widthTB = FindViewById<TextView> (Resource.Id.widthTextBox);
			heightTB = FindViewById<TextView> (Resource.Id.heightTextBox);
			varTB = FindViewById<TextView> (Resource.Id.varTextBox);
			sizeTB = FindViewById<TextView> (Resource.Id.sizeTextBox);

            var metrics = Resources.DisplayMetrics;

			//widthTB.Text = metrics.WidthPixels.ToString();
			//heightTB.Text = metrics.HeightPixels.ToString();
			widthTB.Text = "1080";
			heightTB.Text = "1920";
		    varTB.Text = ".75";
		    sizeTB.Text = "150";
            
		}

        private void growAnimation(object sender, EventArgs e)
        {
            imagePanel.growAnimation();
        }

        private void sweepAnimation(object sender, EventArgs e)
        {
            imagePanel.sweepAnimation();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            imagePanel = null;
            widthTB = null;
            heightTB = null;
            varTB = null;
            sizeTB = null;
        }

		private void UpdatePolyLib(object sender, EventArgs e)
        {
            var boundsWidth = Int32.Parse(widthTB.Text);
			var boundsHeight = Int32.Parse(heightTB.Text);

            var variance = double.Parse(varTB.Text);
			var cellSize = double.Parse(sizeTB.Text);

		    imagePanel.Generate(boundsWidth, boundsHeight, variance, cellSize);
        }
	}
}


