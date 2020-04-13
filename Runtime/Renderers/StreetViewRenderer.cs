using System.IO;
using UnityEngine;

namespace Adrenak.UniMap {
	public class StreetViewRenderer : MonoBehaviour {
		public Renderer targetRenderer;
		public StreetViewDownloader downloader;

		Cubemap cubemap;
		Material material;

		public void Start() {
			material = new Material(Shader.Find("Adrenak/Unlit/Cubemap"));
			int c = 0;
			downloader.OnFaceTextureDownloaded += delegate (StreetView.Face face, Texture2D tex) {
				c++;
				File.WriteAllBytes(Application.dataPath.Replace("Assets", "") + "/" + c + ".jpg", tex.EncodeToJPG());
				AddToCubemap(face, tex);
			};

			downloader.OnFaceTextureFailed += delegate (StreetView.Face face, string message) {
				Debug.Log("Failed : " + face + ". " + message);
			};
		}

		void AddToCubemap(StreetView.Face face, Texture2D tex) {
			if (cubemap == null)
				cubemap = new Cubemap(downloader.options.resolution, TextureFormat.ARGB32, false);

			CubemapFace cubemapFace = CubemapFace.PositiveX;
			switch (face) {
				case StreetView.Face.Up:
					cubemapFace = CubemapFace.NegativeY;
					break;
				case StreetView.Face.Down:
					cubemapFace = CubemapFace.PositiveY;
					break;
				case StreetView.Face.Front:
					cubemapFace = CubemapFace.PositiveZ;
					break;
				case StreetView.Face.Back:
					cubemapFace = CubemapFace.NegativeZ;
					break;
				case StreetView.Face.Left:
					cubemapFace = CubemapFace.NegativeX;
					break;
				case StreetView.Face.Right:
					cubemapFace = CubemapFace.PositiveX;
					break;
			}

			cubemap.SetPixels(tex.GetPixels(), cubemapFace);
			cubemap.Apply();
			material.SetTexture("_Cubemap", cubemap);
			targetRenderer.material = material;
		}
	}
}
