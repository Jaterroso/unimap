using Adrenak.UniMap;
using UnityEngine;

public class FindPlaceExample : MonoBehaviour {
	void Start () {
		FindPlaceRequest search = new FindPlaceRequest();
		search.key = Config.key;
		search.input = "Brighton Beach";
		search.fields.Add(FindPlaceRequest.Field.FormattedAddress);

		// Callback search
		search.Send(
			onResult => Debug.Log(JsonUtility.ToJson(onResult)),
			onError => Debug.LogError(onError)
		);

#if UNIMAP_RSG_PROMISES
		// Promise search
		search.Send()
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
#endif
	}
}
