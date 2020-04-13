using System;
using UnityEngine;
using Adrenak.UniMap;

namespace Adrenak.UniMap.Examples {
    public class TextSearchExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            TextSearchRequest request = new TextSearchRequest("KEY");

            // Callback search
            request.Send(
                "MIT Manipal",
                5000,
                result => Debug.Log(JsonUtility.ToJson(result)),
                exception => Debug.LogError(exception)
            );

            // Promise search
            try {
                var response = await request.Send("MIT Manipal", 5000);
                Debug.LogError(JsonUtility.ToJson(response));
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}
