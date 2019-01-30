using UnityEngine;
using Adrenak.Unex;

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
					return new Vector2(4096, 2048);
				case PanoSize.VeryLarge:
					return new Vector2(8192, 4096);
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
					return 8192;
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
					return new Vector2(8, 4);
				case PanoSize.VeryLarge:
					return new Vector2(16, 8);
				default:
					return Vector2.zero;
			}
		}

		/// <summary>
		/// Calculates the dimensions to which the texture
		/// should be trimmed to make it seamless
		/// </summary>
		/// <param name="texture">The texture to be process</param>
		/// <returns>The dimensions to which the texture should be cropped</returns>
		public static Vector2 DetectTrimmedResolution(Texture32 texture) {
			int height = texture.Height;
			for (int i = texture.Height; i > 0; i--) {
				Color first = texture.GetPixel(0, texture.Height - i);
				for (int j = 1; j < 10; j++) {
					var curr = texture.GetPixel(j, texture.Height - i);
					if(!first.SimilarTo(curr, .001f)) {
						height = i;
						return new Vector2(height * 2, height);
					}
				}
			}
			return new Vector2(height * 2, height);
		}

		public static string GetIDFromURL(string url) {
			return url.Split('!')[4].Substring(2);
		}
	}
}
