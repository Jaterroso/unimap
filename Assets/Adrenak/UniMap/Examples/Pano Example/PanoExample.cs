using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PanoExample : MonoBehaviour {
		public PanoRenderer view;
		public List<string> ids;
		public float delay;
		public PanoSize size;

		IEnumerator Start() {
			UniMapInitializer.Setup();

			while (true) {
				foreach (var id in ids) {
					view.pano.Download(id, size);
					yield return new WaitForSeconds(delay);
				}
			}
		}
	}
}