namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	public static class MathUtils
	{
		public static int ClosestPowerOfTwo(int v)
		{
			//gets value of bit to the right of leading bit and moves it to left by 1
			int r = (v & (v >> 1)) << 1;
			//rs bit in same place as vs leading bit is 1 to round up or 0 to round down.
			v >>= 1;
			//replaces leading bit with a 1 if rounding up or leaves 0 if rounding down.
			v |= r;
			//Next power of 2 exclusive
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v++;
			return v;
		}
	}
}