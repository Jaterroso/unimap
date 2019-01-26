using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoExample : MonoBehaviour {
		public PanoRenderer view;
		public List<string> urls;
		public float delay;
		public PanoSize size;

		IEnumerator Start() {
			UniMapInitializer.Setup();

			while (true) {
				foreach (var url in urls) {
					var id = PanoUtility.GetIDFromURL(url);
					view.pano.Download(id, size);
					yield return new WaitForSeconds(delay);
				}
			}
		}
	}
}
