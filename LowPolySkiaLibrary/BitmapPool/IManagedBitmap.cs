using System;

namespace LowPolyLibrary.BitmapPool
{

	/**
    * A reference-counted Bitmap object. The Bitmap is not really recycled
    * until the reference counter drops to zero.
    */
    public interface IManagedBitmap
    {
		/*
        * Get the underlying Bitmap object.
        * NEVER call Bitmap.recycle() on this object.
        */
		Bitmap GetBitmap();

		/**
        * Decrease the reference counter and recycle the underlying Bitmap
        * if there are no more references.
        */
        void recycle();

		/**
        * Increase the reference counter.
        * @return self
        */
        IManagedBitmap retain();

    }
}
