using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoExample : MonoBehaviour {
		public PanoRenderer view;
		public List<string> ids;
		public float delay;
		public PanoSize size;
		public TextureFormat format = TextureFormat.RGB24;

		IEnumerator Start() {
			UniMapInitializer.Setup();

			while (true) {
				foreach (var id in ids) {
					view.downloader.Download(id, size, format);
					yield return new WaitForSeconds(delay);
				}
			}
		}
	}
}