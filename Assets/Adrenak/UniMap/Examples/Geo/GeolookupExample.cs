using System.Collections;
using Adrenak.UniMap;
using UnityEngine;

public class GeolookupExample : MonoBehaviour {
	void Start () {
		var request = new GeolookupRequest("AIzaSyAzYNj5AyuB0e8KSmSdyzMLYnYtJRVnNho");
		request.Send(new Location(13.3525321, 74.79282239999999))
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
