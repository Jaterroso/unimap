using Adrenak.UniMap;
using UnityEngine;

public class NearbySearchRequestExample : MonoBehaviour {
	void Start () {
		var request = new NearbySearchRequest();
		request.key = Config.key;
		request.location = new Location(48.8f, 2.35f);
		request.radius = 1000;
		request.type = PlaceType.Atm;

		// Callback search
		request.Send(
			result => Debug.Log(JsonUtility.ToJson(result)),
			exception => Debug.LogError(exception)
		);

		// Promise search
		request.Send()
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
