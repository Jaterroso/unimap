using System;
using Adrenak.UniMap;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class GeolookupExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            var request = new GeolookupRequest("KEY");
            try {
                var response = await request.Send(new Location(13.3525321, 74.79282239999999));
                Debug.Log(JsonUtility.ToJson(response));
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}