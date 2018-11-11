using UnityEngine;
using Adrenak.UniMap;

public class TextSearchExample : MonoBehaviour {
	void Start () {
		TextSearchRequest request = new TextSearchRequest();
		request.key = Config.key;
		request.query = "MIT manipal";

		// Callback search
		request.Send(
			result => Debug.Log(JsonUtility.ToJson(result)),
			exception => Debug.LogError(exception)
		);

#if UNIMAP_RSG_PROMISES
		// Promise search
		request.SendAsync()
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
#endif
	}
}
