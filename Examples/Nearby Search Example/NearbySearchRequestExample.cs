using System;
using Adrenak.UniMap;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class NearbySearchRequestExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            var request = new NearbySearchRequest("KEY") {
                radius = 1000,
                type = PlaceType.Atm
            };

            // Callback search
            request.Send(
                new Location(48.8, 2.35),
                result => Debug.Log(JsonUtility.ToJson(result)),
                exception => Debug.LogError(exception)
            );

            // Task search
            try {
                var response = await request.Send(new Location(40.0, 2.35));
                Debug.Log(JsonUtility.ToJson(response));
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}