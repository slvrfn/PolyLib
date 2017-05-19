using System;
using System.Collections.Concurrent;
using Android.Graphics;

namespace LowPolyLibrary.BitmapPool
{
	/**
    * A pool of fixed-size Bitmaps. Leases a managed Bitmap object
    * which is tied to this pool. Bitmaps are put back to the pool
    * instead of actual recycling.
    *
    * WARNING: This class is NOT thread safe, intended for use
    *          from the main thread only.
    */
    public class BitmapPool
    {
		private readonly int _width;
		private readonly int _height;
		private readonly Bitmap.Config _config;
		private readonly ConcurrentStack<Bitmap> _bitmaps = new ConcurrentStack<Bitmap>();
		private bool _isRecycled;

		//private final Handler handler = new Handler();

		public BitmapPool(int bitmapWidth, int bitmapHeight, Bitmap.Config conf)
        {
            _width = bitmapWidth;
            _height = bitmapHeight;
            _config = conf;
        }

		/**
        * Destroy the pool. Any leased IManagedBitmap items remain valid
        * until they are recycled.
        * */
        public void recycle()
        {
            _isRecycled = true;
            foreach (var bitmap in _bitmaps)
            {
                bitmap.Recycle();
            }
            _bitmaps.Clear();
        }

		/**
        * Get a Bitmap from the pool or create a new one.
        * @return a managed Bitmap tied to this pool
        */
        public IManagedBitmap getBitmap()
        {
            Bitmap map;
            if (_bitmaps.Count == 0)
            {
                map = Bitmap.CreateBitmap(_width, _height, _config);
            }
            else
            {
                if (!_bitmaps.TryPop(out map))
                {
                    //should never reach here
                    //if this point is reached, trypop failed
                    var t = 0;
                }
            }

            return new LeasedBitmap(map, this);
		}

        public class LeasedBitmap : IManagedBitmap
        {
            private BitmapPool _pool;

            private int referenceCounter = 1;
            private readonly Bitmap _bitmap;

            internal LeasedBitmap(Bitmap bitmap, BitmapPool pool)
            {
                _bitmap = bitmap;
                _pool = pool;
            }

            public Bitmap GetBitmap()
            {
                return _bitmap;
            }

            public void recycle()
            {
				if (--referenceCounter == 0)
				{
					if (_pool._isRecycled)
					{
						_bitmap.Recycle();
					}
					else
					{
                        //set the image to black for reuse
                        using(var c = new Canvas(_bitmap))
                        {
                            c.DrawRGB(0,0,0);
                        }
						_pool._bitmaps.Push(_bitmap);
					}
				}
            }

            public IManagedBitmap retain()
            {
                ++referenceCounter;
                return this;
            }
        }
    }
}
