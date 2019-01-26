using Adrenak.UniMap;
using UnityEngine;

public class FindPlaceExample : MonoBehaviour {
	void Start () {
		FindPlaceRequest search = new FindPlaceRequest();
		search.key = "ENTER_KEY_HERE";
		search.input = "Brighton Beach";
		search.fields.Add(FindPlaceRequest.Field.FormattedAddress);

		// Callback search
		search.Send(
			onResult => Debug.Log(JsonUtility.ToJson(onResult)),
			onError => Debug.LogError(onError)
		);

		// Promise search
		search.Send()
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
	}
}
