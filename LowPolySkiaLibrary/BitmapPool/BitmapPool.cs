using System;
using System.Collections.Concurrent;
using SkiaSharp;

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
		private readonly SKImageInfo _config;
		private readonly ConcurrentStack<SKSurface> _bitmaps = new ConcurrentStack<SKSurface>();
		private bool _isRecycled;

		//private final Handler handler = new Handler();

		public BitmapPool(int width, int height)
        {
            var conf = new SKImageInfo(width, height);
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
                //check which gives better performance
                //bitmap.Recycle();
                bitmap.Dispose();
            }
            _bitmaps.Clear();
        }

		/**
        * Get a Bitmap from the pool or create a new one.
        * @return a managed Bitmap tied to this pool
        */
        public IManagedBitmap getBitmap()
        {
            SKSurface map;
            if (_bitmaps.Count == 0)
            {
                map = SKSurface.Create(_config);
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
            private readonly SKSurface _bitmap;

            internal LeasedBitmap(SKSurface bitmap, BitmapPool pool)
            {
                _bitmap = bitmap;
                _pool = pool;
            }

            public SKSurface GetBitmap()
            {
                return _bitmap;
            }

            public void recycle()
            {
				if (--referenceCounter == 0)
				{
					if (_pool._isRecycled)
					{
					    //check which gives better performance
					    //_bitmap.Recycle();
                        _bitmap.Dispose();
					}
					else
					{
                        //set the image to black for reuse
                        using(var c = _bitmap.Canvas)
                        {
                            c.Clear();
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
