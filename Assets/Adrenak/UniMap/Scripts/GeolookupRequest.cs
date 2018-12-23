using RestSharp;
using System.Text;
using System;
using Adrenak.Unex;
using UnityEngine;

namespace Adrenak.UniMap {
	public class GeolookupRequest {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/geocode/json?";
		public string Key { get; private set; }
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public GeolookupRequest(string key, double lat, double lng) {
			Core.Init();
			Key = key;
			Latitude = lat;
			Longitude = lng;
		}

		public string GetURL() {
			if (Key.IsNullOrEmpty())
				throw new Exception("No key provided");

			var builder = new StringBuilder(k_BaseURL);
			builder.Append("key=").Append(Key);
			builder.Append("&latlng=").Append(Latitude).Append(",").Append(Longitude);
			
			return builder.ToString();
		}

		public IPromise<GeolookupResponse> Send() {
			var promise = new Promise<GeolookupResponse>();
			Send(
				response => promise.Resolve(response),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		public void Send(Action<GeolookupResponse> onSuccess, Action<Exception> onFailure) {
			var client = new RestClient();
			var request = new RestRequest(GetURL(), Method.GET);

			client.ExecuteAsync(request, (response, handle) => {
				if (response.IsSuccess()) {
					var model = JsonUtility.FromJson<GeolookupResponse>(response.Content);
					if (model != null)
						onSuccess.TryInvoke(model);
					else {
						var exception = new Exception("Could not deserialize", response.GetException());
						onFailure.TryInvoke(exception);
					}
				}
				else {
					var exception = new Exception("Unsuccessful response for Geolookup", response.GetException());
					onFailure.TryInvoke(exception);
				}
			});
		}
	}
}