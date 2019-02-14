using UnityEngine;
using System.Linq;
using System.Collections.Generic;
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

		public static Vector2 DetectBlankBands(Texture32 texture) {
			Color32 first;

			int height = texture.Height;
			for (int i = 0; i < texture.Height; i++) {
				first = texture.GetPixel(0, i);
				for (int j = 1; j < 10; j++) {
					var curr = texture.GetPixel(j, i);
					if (!first.SimilarTo(curr, 3)) {
						height = texture.Height - i;
						goto width;
					}
				}
			}

			width:

			int width = texture.Width;
			for (int i = texture.Width - 1; i > 0; i--) {
				first = texture.GetPixel(i, 0);
				for (int j = 1; j < 10; j++) {
					var curr = texture.GetPixel(i, texture.Height - j);
					if (!first.SimilarTo(curr, 3)) {
						width = i;
						goto done;
					}
				}
			}

			done:
			return new Vector2(width, height);
		}

		public static int DetectWidth(Texture32 texture) {
			int width = texture.Width;
			List<float> deltas = new List<float>();

			for (int i = texture.Width - 1; i > texture.Width / 2; i--) {
				float sum = 0;
				for (int j = 0; j < texture.Height; j += 8) {
					var reff = texture.GetPixel(0, texture.Height - 1 - j);
					var curr = texture.GetPixel(i, texture.Height - 1 - j);
					sum += reff.Minus(curr).Magnitude();
				}
				deltas.Add(sum);
			}

			float min = deltas[0];
			for(int i = 1; i < deltas.Count; i++) {
				if (min > deltas[i])
					min = deltas[i];
			}

			for (int i = 0; i < deltas.Count; i++) {
				var f = deltas[i];
				if (f.Approximately(min)) {
					return texture.Width - i;
				}
			}
			return width;
		}

		public static string GetIDFromURL(string url) {
			return url.Split('!')[4].Substring(2);
		}
	}
}
