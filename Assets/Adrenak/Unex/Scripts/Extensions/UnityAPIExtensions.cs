using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Object = UnityEngine.Object;

namespace Adrenak.Unex {
    public static class UnityAPIExtensions {
        // RECT
        public static Rect SetX(this Rect rect, float val) {
            return new Rect(val, rect.y, rect.width, rect.height);
        }

        public static Rect SetY(this Rect rect, float val) {
            return new Rect(rect.x, val, rect.width, rect.height);
        }

        public static Rect SetW(this Rect rect, float val) {
            return new Rect(rect.x, rect.y, val, rect.height);
        }

        public static Rect SetH(this Rect rect, float val) {
            return new Rect(rect.x, rect.y, rect.width, val);
        }

        // VECTOR3
        public static bool Approximately(this Vector3 a, Vector3 b) {
            return
                Mathf.Approximately(a.x, b.x) &&
                Mathf.Approximately(a.y, b.y) &&
                Mathf.Approximately(a.z, b.z);
        }
        
        public static Vector3 SetX(this Vector3 v, float val) {
            return new Vector3(val, v.y, v.z);
        }

        public static Vector3 SetY(this Vector3 v, float val) {
            return new Vector3(v.x, val, v.z);
        }

        public static Vector3 SetZ(this Vector3 v, float val) {
            return new Vector3(v.x, v.y, val);
        }

        // VECTOR2
        public static bool Approximately(this Vector2 a, Vector2 b) {
            return
                Mathf.Approximately(a.x, b.x) &&
                Mathf.Approximately(a.y, b.y);
        }

        public static Vector2 SetX(this Vector2 v, float val) {
            return new Vector2(val, v.y);
        }

        public static Vector2 SetY(this Vector2 v, float val) {
            return new Vector2(v.x, val);
        }

        // GAME OBJECTS
        public static void Destroy(this GameObject gameObject) {
            MonoBehaviour.Destroy(gameObject);
        }

        public static void DestroyImmediate(this GameObject gameObject) {
            MonoBehaviour.DestroyImmediate(gameObject);
        }

		public static T EnsureComponent<T>(this GameObject go) where T : Component {
			var get = go.GetComponent<T>();
			if (get == null)
				return go.AddComponent<T>();
			return get;
		}
		// https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
		public static T Clone<T>(this Component comp, T other) where T : Component {
			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos) {
				if (pinfo.CanWrite) {
					try {
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
					catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
				}
			}
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos)
				finfo.SetValue(comp, finfo.GetValue(other));
			return comp as T;
		}


		// VIEW GROUP
		public static void Set(this CanvasGroup group, bool status) {
            group.interactable = group.blocksRaycasts = status;
            group.alpha = status ? 1 : 0;
        }

        // LAYER MASK
        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }

        public static int[] GetIncludedLayers(this LayerMask mask) {
            List<int> layers = new List<int>();
            for (int i = 0; i < 32; i++) {
                var contains = mask.Contains(i);
                if(contains)
                    layers.Add(i);
            }
            return layers.ToArray();
        }

        public static bool[] GetLayersAsBool(this LayerMask mask) {
            bool[] boolArray = new bool[32];
            for (int i = 0; i < 32; i++)
                boolArray[i] = mask.Contains(i);
            return boolArray;
        }
		
		public static void SetActiveRecursively(this GameObject go) {
			go.SetActive(true);
			foreach (Transform t in go.transform)
				t.gameObject.SetActiveRecursively();
		}

		public static Texture2D GetFrame(this Camera cam) {
			cam.Render();
			RenderTexture.active = cam.targetTexture;
			var tex = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false);
			tex.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
			RenderTexture.active = null;

			return tex;
		}

		public static Texture2D GetFrame(this WebCamTexture tex) {
			if (!tex.isPlaying) return null;
			var result = new Texture2D(tex.width, tex.height);
			result.SetPixels(tex.GetPixels());
			result.Apply();
			return result;
		}

		public static Object LoadObject(this AssetBundle bundle, string name, bool unload) {
			var temp = bundle.LoadObjects(new string[] { name }, unload);

			if (temp.Length != 0) return temp[0];
			else return null;
		}

		public static Object[] LoadObjects(this AssetBundle bundle, string[] names, bool unload) {
			if (bundle == null) return new List<Object>().ToArray();

			List<Object> result = new List<Object>();

			for (int i = 0; i < names.Length; i++) {
				var temp = bundle.LoadAsset<Object>(names[i]);
				if (temp != null)
					result.Add(temp);
			}

			if (unload)
				bundle.Unload(false);

			return result.ToArray();
		}

		public static Texture2D ToPixel(this Color color) {
			var texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, color);
			texture.Apply();
			return texture;
		}
		
		public static Color GetAverageColor(this Texture2D tex) {
			var pixels = tex.GetPixels();
			Vector3 avg = Vector3.zero;
			for (int i = 0; i < pixels.Length; i++)
				avg += new Vector3(
					pixels[i].r,
					pixels[i].g,
					pixels[i].b
				);
			avg /= pixels.Length;
			return new Color(avg.x, avg.y, avg.z, 1);
		}

		public static bool SameAs(this Color c1, Color c2) {
			return
				c1.r == c2.r && c1.g == c2.g && c1.b == c2.g && c1.a == c2.a;
		}

		public static Color Minus(this Color c1, Color c2) {
			return new Color(
				Mathf.Abs(c1.r - c2.r),
				Mathf.Abs(c1.g - c2.g),
				Mathf.Abs(c1.b - c2.b),
				Mathf.Abs(c1.a - c2.a)
			);
		}

		public static float Magnitude(this Color c) {
			return (c.r + c.g + c.b + c.a) / 4;
		}

		public static bool SimilarTo(this Color c1, Color c2, float margin) {
			return c1.Minus(c2).Magnitude() < margin;
		}

		public static Texture2D Crop(this Texture2D tex, Rect rect) {
			var w = tex.width;
			var h = tex.height;

			var colors = new Color32[(int)rect.width * (int)rect.height];

			var index = 0;
			var pixels = tex.GetPixels32();
			for(int i = 0; i < pixels.Length; i++) {
				var r = i / w;
				var c = i % w;
				if(r >= rect.y && r < rect.y + rect.height && c >= rect.x && c < rect.x + rect.width) {
					colors[index] = pixels[i];
					index++;
				}
			}

			var result = new Texture2D((int)rect.width, (int)rect.height, tex.format, true);
			result.SetPixels32(colors);
			result.Apply();
			return result;
		}

		public static void Copy(this Texture2D tex, Texture2D other, Vector2 position, bool apply = true) {
			var pixels = other.GetPixels32();
			var w = other.width;
			var h = other.height;

			tex.SetPixels32((int)position.x, (int)position.y, w, h, pixels);
			if(apply)
				tex.Apply();
			pixels = null;
		}

		public static float GetGreyscale(this Color32 color) {
			return (color.r + color.g + color.b) / 3;
		}

		public static Sprite ToSprite(this Texture2D tex) {
			if (tex == null) return null;
			return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5F, .5F));
		}
	}
}
