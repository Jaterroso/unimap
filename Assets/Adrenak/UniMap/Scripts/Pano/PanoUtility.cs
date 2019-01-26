using UnityEngine;

namespace Adrenak.UniMap {
	public static class PanoUtility {
		public static Vector2 GetUntrimmedResolution(PanoSize level) {
			switch (level) {
				case PanoSize.VerySmall:
					return new Vector2(512, 512);
				case PanoSize.Small:
					return new Vector2(1024, 512);
				case PanoSize.Medium:
					return new Vector2(2048, 1024);
				case PanoSize.Large:
					return new Vector2(3584, 2048);
				case PanoSize.VeryLarge:
					return new Vector2(6656, 4096);
				default:
					return Vector2.zero;
			}
		}

		public static int GetUserPanoWidth(PanoSize size) {
			switch (size) {
				case PanoSize.VerySmall:
					return 512;
				case PanoSize.Small:
					return 1024;
				case PanoSize.Medium:
					return 2048;
				case PanoSize.Large:
					return 4096;
				case PanoSize.VeryLarge:
#if UNITY_ANDROID
					return 4096;
#elif UNITY_EDITOR || UNITY_STANDALONE
					return 8192;
#else
					return 4096;
#endif
				default:
					return 1;
			}
		}

		public static int GetZoomValue(PanoSize level) {
			switch (level) {
				case PanoSize.VerySmall:
					return 0;
				case PanoSize.Small:
					return 1;
				case PanoSize.Medium:
					return 2;
				case PanoSize.Large:
					return 3;
				case PanoSize.VeryLarge:
					return 4;
				default:
					return -1;
			}
		}

		public static Vector2 GetTileCount(PanoSize level) {
			switch (level) {
				case PanoSize.VerySmall:
					return new Vector2(1, 1);
				case PanoSize.Small:
					return new Vector2(2, 1);
				case PanoSize.Medium:
					return new Vector2(4, 2);
				case PanoSize.Large:
					return new Vector2(7, 4);
				case PanoSize.VeryLarge:
					return new Vector2(13, 8);
				default:
					return Vector2.zero;
			}
		}

		public static Vector2 GetTrimmedResolution(PanoSize level) {
			switch (level) {
				case PanoSize.VerySmall:
					return new Vector2(416, 208);
				case PanoSize.Small:
					return new Vector2(832, 416);
				case PanoSize.Medium:
					return new Vector2(1664, 832);
				case PanoSize.Large:
					return new Vector2(3328, 1664);
				case PanoSize.VeryLarge:
					return new Vector2(6656, 3328);
				default:
					return new Vector2(0, 0);
			}
		}

		public static string GetIDFromURL(string url) {
			return url.Split('!')[4].Substring(2);
		}
	}
}
