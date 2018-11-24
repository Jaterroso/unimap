using Adrenak.UniMap;
using UnityEngine;

public class StreetViewMetaExample : MonoBehaviour {
	void Start () {
		var request = new StreetViewMetaRequest();
		request.options.key = Config.key;
		request.options.mode = StreetView.Mode.Location;
		request.options.place = "Taj Mahal";
		request.options.source = StreetView.Source.Outdoor;
		request.options.radius = 1000;

		request.Send(
			response => Debug.Log(JsonUtility.ToJson(response)),
			exception => Debug.LogError(exception)
		);

#if UNIMAP_RSG_PROMISES
		request.Send()
			.Then(response => Debug.Log(JsonUtility.ToJson(response)))
			.Catch(exception => Debug.LogError(exception));
#endif
	}
}
