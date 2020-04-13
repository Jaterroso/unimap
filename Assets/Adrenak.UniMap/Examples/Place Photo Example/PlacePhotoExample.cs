/*
 * UPDATE THIS SCRIPTS TO WORK WITH TASK BASED METHODS
 * 
using Adrenak.UniMap;
using UnityEngine;
using System.Collections.Generic;

public class PlacePhotoExample : MonoBehaviour {
	public Renderer renderer1;
	public Renderer renderer2;

	void Start() {
		PromiseExample();
		CallbackExample();
	}

	// Using promises, we get a relatively flat flow of code
	void PromiseExample() {
        // Create and send a find place request
        new FindPlaceRequest("KEY") {
            inputType = FindPlaceRequest.InputType.TextQuery,
            fields = new List<FindPlaceRequest.Field>() {
                FindPlaceRequest.Field.Photos
            }
        }.Send("Manhattan")
        // Then find place response and return the first photo reference 
        .Then(response => {
            Debug.Log(JsonUtility.ToJson(response));
            if (response.candidates.Count > 0 &&
                response.candidates[0].photos.Count > 0)
                return response.candidates[0].photos[0].photo_reference;
            else
                throw new System.Exception("No photos or results");
        })
        // Use the reference and return the download request
        .Then(reference => {
            return new PlacePhotoDownloader("KEY") {
                MaxWidth = 512,
                MaxHeight = 512,
            }.Download(reference);
        })
        // Use the result of the download request, a Texture2D object, to show on the renderer
        .Then(texture => renderer1.material.mainTexture = texture)
        .Catch(exception => Debug.LogError(exception));
    }

	// Using callbacks, the code tends to indent towards the right
	void CallbackExample() {
		// Create a find place request
		new FindPlaceRequest("KEY") {
			inputType = FindPlaceRequest.InputType.TextQuery,
			fields = new List<FindPlaceRequest.Field>() {
				FindPlaceRequest.Field.Photos
			}
		}.Send(
			"Manhattan",
			// If the find place request is a success
			response => {
				// Create a photo download request
				var reference = response.candidates[0].photos[0].photo_reference;
				new PlacePhotoDownloader("KEY") {
					MaxWidth = 512,
					MaxHeight = 512,
				}.Download(
					reference,
					// If the photo download is a success, show on a renderer
					texture => renderer2.material.mainTexture = texture,
					// Else show the exception
					exception => Debug.LogError(exception)
				);
			},
			// If the find place request failed, show the exception
			exception => {
				Debug.LogError(exception);
			}
		);
	}
}
*/