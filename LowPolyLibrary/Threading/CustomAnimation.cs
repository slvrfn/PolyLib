using System;
using System.CodeDom.Compiler;

namespace LowPolyLibrary.Threading
{
	public class CustomAnimtion
	{
		public int CurrentFrame = 0;
		public string Type;
		public int TotalFrames;
		public int FrameDuration;

	    public int[] frames;

		public CustomAnimtion(string type, int totalFrames, int duration)
		{
			Type = type;
			TotalFrames = totalFrames;
			FrameDuration = duration;
            GenerateFrames(totalFrames);
		}

	    private void GenerateFrames(int numframes)
	    {
	        frames = new int[numframes];
            var rand = new Random();
	        for (int i = 0; i < frames.Length; i++)
	        {
                frames[i] = rand.Next(10);
	        }
	    }
	}
}
