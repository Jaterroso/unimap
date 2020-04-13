using System;
using Adrenak.UniMap;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class StreetViewMetaExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            var request = new StreetViewMetaRequest();
            request.options.key = "ENTER_KEY_HERE";
            request.options.mode = StreetView.Mode.Location;
            request.options.place = "Taj Mahal";
            request.options.source = StreetView.Source.Outdoor;
            request.options.radius = 1000;

            request.Send(
                response => Debug.Log(JsonUtility.ToJson(response)),
                exception => Debug.LogError(exception)
            );

            try {
                var response = await request.Send();
                Debug.Log(JsonUtility.ToJson(response));
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}