using UnityEngine;
using Adrenak.UniMap;

public class TextSearchExample : MonoBehaviour {
	void Start () {
		TextSearchRequest request = new TextSearchRequest();
		request.key = "ENTER_KEY_HERE";
		request.query = "MIT manipal";

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
