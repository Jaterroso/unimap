using Adrenak.UniMap;
using System.Net;
using UnityEngine;

public class GeocodingExample : MonoBehaviour {
	void Start () {
		ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;

		var request = new GeocodingRequest("AIzaSyAzYNj5AyuB0e8KSmSdyzMLYnYtJRVnNho") {
			Region = "us",
			Language = "en"
		};

		Debug.Log(request.GetURL());

		request.Send("MIT Manipal")
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.Log(exception));
	}
}
