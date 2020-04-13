using Adrenak.UniMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class StreetViewExample : MonoBehaviour {
        public StreetViewRenderer streetRenderer;
        public List<string> locations;
        public float delay = 10;

        IEnumerator Start() {
            UniMapInitializer.Initialize();

            while (true) {
                foreach (var location in locations) {
                    UpdateLocation(location);
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        void UpdateLocation(string newLocation) {
            streetRenderer.downloader.options.place = newLocation;
            streetRenderer.downloader.Download();
        }
    }
}