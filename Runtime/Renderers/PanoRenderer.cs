using System;
using Adrenak.Unex;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoRenderer : MonoBehaviour {
		public PanoDownloader downloader = new PanoDownloader();
		public Renderer panoSurface;
		Texture2D m_Texture;

		void Awake() {
			downloader.OnStarted += () => {
				panoSurface.material.mainTexture = new Color(0, 0, 0, 1).ToPixel();
			};

			downloader.OnLoaded += t32 => {
				if (m_Texture != null) {
					MonoBehaviour.Destroy(m_Texture);
					m_Texture = null;
					GC.Collect();
				}
				m_Texture = t32.GetTexture2D(TextureFormat.RGB565);
				panoSurface.material.mainTexture = m_Texture;
			};
		}

		private void OnDestroy() {
			downloader.Stop();
		}
	}
}
