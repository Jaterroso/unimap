using UnityEngine;
using Adrenak.UniMap;

public class TextSearchExample : MonoBehaviour {
	void Start () {
		TextSearchRequest request = new TextSearchRequest("KEY");

		// Callback search
		request.Send(
			"MIT Manipal", 
			5000,
			result => Debug.Log(JsonUtility.ToJson(result)),
			exception => Debug.LogError(exception)
		);

		// Promise search
		request.Send("MIT Manipal", 5000)
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
