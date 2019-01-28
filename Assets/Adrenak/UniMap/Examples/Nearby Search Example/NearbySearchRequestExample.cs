using Adrenak.UniMap;
using UnityEngine;

public class NearbySearchRequestExample : MonoBehaviour {
	void Start () {
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

		// Promise search
		request.Send(new Location(48.8, 2.35))
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
