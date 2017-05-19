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
using LowPolyLibrary.Animation;
using System.Threading.Tasks.Dataflow;
using LowPolyLibrary.BitmapPool;

namespace LowPoly
{
	[Activity (Label = "LowPoly", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
	public class MainActivity : Activity, View.IOnTouchListener
	{
        Button button, animSButton, animGButton;
		ImageView imagePanel;
		TextView widthTB, heightTB, varTB, sizeTB, timeElapsed;
	    private LowPolyLibrary.Triangulation _lowPoly;

		private LowPolyLibrary.Animation.Animation animation;

	    private BitmapPool ReuseableBitmapPool;
        private IManagedBitmap LastBitmap;

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
			animSButton.Click += sweepAnimation;
		    animGButton = FindViewById<Button>(Resource.Id.animGButton);
		    animGButton.Click += growAnimation;

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



			animation = new LowPolyLibrary.Animation.Animation((arg) =>
			{
                //imagePanel.SetImageDrawable(new BitmapDrawable(arg));
                //RunOnUiThread(() => 
                //{
                if (LastBitmap != null)
                {
                    //recycle necessary?TODO
                    //((BitmapDrawable)imagePanel.Drawable).Bitmap.Recycle();
                    //((BitmapDrawable)imagePanel.Drawable).Bitmap.Dispose();
                    //imagePanel.Drawable.Dispose();
                    //imagePanel.SetImageBitmap(null);
                    LastBitmap.recycle();

                }
                imagePanel.SetImageBitmap(arg.GetBitmap());
                LastBitmap = arg;
			});
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LastBitmap.recycle();
            ReuseableBitmapPool.recycle();

        }

		private void UpdatePolyLib()
		{
            var boundsWidth = Int32.Parse(widthTB.Text);
			var boundsHeight = Int32.Parse(heightTB.Text);

		    //first occurence
		    if (ReuseableBitmapPool == null)
		    {
		        ReuseableBitmapPool = new BitmapPool(boundsWidth, boundsHeight, Bitmap.Config.Rgb565);
		    }

            var variance = double.Parse(varTB.Text);
			var cellSize = double.Parse(sizeTB.Text);

            _lowPoly = new LowPolyLibrary.Triangulation(boundsWidth,boundsHeight,variance, cellSize, ReuseableBitmapPool);
        }

		public void Generate (object sender, EventArgs e){
            var temp = new Stopwatch();

			UpdatePolyLib();
            
            temp.Start();
		    var generatedBitmap = _lowPoly.GeneratedBitmap;
            temp.Stop();
			if (LastBitmap != null)
			{
				//((BitmapDrawable)imagePanel.Drawable).Bitmap.Recycle();
				//((BitmapDrawable)imagePanel.Drawable).Bitmap.Dispose();
                //imagePanel.Drawable.Dispose();
                LastBitmap.recycle();

			}
            //imagePanel.SetImageDrawable (new BitmapDrawable (generatedBitmap));
            imagePanel.SetImageBitmap(generatedBitmap.GetBitmap());
            LastBitmap = generatedBitmap;

		    timeElapsed.Text = temp.Elapsed.ToString();
		}

		public void uuuu(AnimationTypes.Type anim, System.Drawing.PointF touch)
		{
			var temp = new Stopwatch();

			temp.Start();
			switch (anim)
			{
				case AnimationTypes.Type.Grow:
                    animation.AddEvent(_lowPoly, AnimationTypes.Type.Grow, 12);
                    break;
				case AnimationTypes.Type.Sweep:
					animation.AddEvent(_lowPoly, AnimationTypes.Type.Sweep, 12);
					break;
				case AnimationTypes.Type.Touch:
                    animation.AddEvent(_lowPoly, AnimationTypes.Type.Touch, 12, touch.X, touch.Y, 500);
                    break;
			}
			temp.Stop();

			timeElapsed.Text = temp.Elapsed.ToString();
		}

		public void sweepAnimation(object sender, EventArgs e)
		{
			uuuu(AnimationTypes.Type.Sweep, new System.Drawing.PointF(0, 0));
		}

        public void growAnimation(object sender, EventArgs e)
        {
            uuuu(AnimationTypes.Type.Grow, new System.Drawing.PointF(0, 0));
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


