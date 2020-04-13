using Adrenak.UniMap;
using System;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class GeocodingExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            var request = new GeocodingRequest("KEY") {
                Region = "us",
                Language = "en"
            };

            try {
                var response = await request.Send("MIT Manipal");
                Debug.Log(JsonUtility.ToJson(response));
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}