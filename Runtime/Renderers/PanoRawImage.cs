using UnityEngine;
using Adrenak.Unex;
using UnityEngine.UI;

namespace Adrenak.UniMap {
	public class PanoRawImage : MonoBehaviour {
		public PanoDownloader downloader = new PanoDownloader();
		public RawImage display;
		Texture2D m_Texture;

		void Awake() {
			downloader.OnStarted += () => {
				display.material.mainTexture = new Color(0, 0, 0, 1).ToPixel();
			};

			downloader.OnLoaded += t32 => {
				if (m_Texture != null)
					MonoBehaviour.Destroy(m_Texture);
				m_Texture = null;
				m_Texture = t32.GetTexture2D(TextureFormat.RGB565);
				display.material.mainTexture = m_Texture;
			};
		}

		private void OnDestroy() {
			downloader.Stop();
		}
	}
}
