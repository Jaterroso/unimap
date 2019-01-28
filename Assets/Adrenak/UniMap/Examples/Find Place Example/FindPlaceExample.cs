using Adrenak.UniMap;
using UnityEngine;

public class FindPlaceExample : MonoBehaviour {
	void Start () {
		FindPlaceRequest search = new FindPlaceRequest("KEY"); ;
		search.fields.Add(FindPlaceRequest.Field.FormattedAddress);

		// Callback search
		search.Send(
			"Manhattan",
			onResult => Debug.Log(JsonUtility.ToJson(onResult)),
			onError => Debug.LogError(onError)
		);

		// Promise search
		search.Send("Manhattan")
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
