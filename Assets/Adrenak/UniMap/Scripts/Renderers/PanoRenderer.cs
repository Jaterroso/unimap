using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoRenderer : MonoBehaviour {
		public PanoDownloader pano = new PanoDownloader();
		public Renderer panoSurface;

		private void Awake() {
			pano.OnLoaded += delegate (Texture2D pano) {
				panoSurface.material.mainTexture = pano;
			};
		}
	}
}
