using System;
using Adrenak.UniMap;
using UnityEngine;

namespace Adrenak.UniMap.Examples {
    public class FindPlaceExample : MonoBehaviour {
        async void Start() {
            UniMapInitializer.Initialize();

            FindPlaceRequest search = new FindPlaceRequest("KEY"); ;
            search.fields.Add(FindPlaceRequest.Field.FormattedAddress);

            // Callback search
            search.Send(
                "Manhattan",
                onResult => Debug.Log(JsonUtility.ToJson(onResult)),
                onError => Debug.LogError(onError)
            );

            try {
                var response = await search.Send("Manhattan");
                Debug.Log(response);
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}