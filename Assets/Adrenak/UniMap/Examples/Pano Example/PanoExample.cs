using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoExample : MonoBehaviour {
		public PanoRenderer view;
		public List<string> urls;
		public float delay;

		IEnumerator Start() {
			Dispatcher.Instance.Init();

			while (true) {
				foreach (var url in urls) {
					view.pano.DownloadUsingURL(url);
					yield return new WaitForSeconds(delay);
				}
			}
		}
	}
}
