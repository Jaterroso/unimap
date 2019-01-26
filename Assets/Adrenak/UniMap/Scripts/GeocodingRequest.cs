using RestSharp;
using System.Text;
using System;
using Adrenak.Unex;
using UnityEngine;

namespace Adrenak.UniMap {
	public class GeocodingRequest {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/geocode/json?";
		public string Key { get; private set; }
		public string Address { get; private set; }
		public string Language { get; set; }
		public string Region { get; set; }

		public GeocodingRequest(string key, string address) {
			UniMapInitializer.Setup();
			Key = key;
			Address = address;
		}

		public string GetURL() {
			if (Key.IsNullOrEmpty()) 
				throw new Exception("No key provided");

			if (Address.IsNullOrEmpty())
				throw new Exception("No address provided");

			var builder = new StringBuilder(k_BaseURL);
			builder.Append("key=").Append(Key);
			builder.Append("&address=").Append(Address);

			if (!Language.IsNullOrEmpty())
				builder.Append("&language=").Append(Language);

			if (!Region.IsNullOrEmpty())
				builder.Append("&region=").Append(Region);

			return builder.ToString();
		}

		public IPromise<GeocodingResponse> Send() {
			var promise = new Promise<GeocodingResponse>();
			Send(
				response => promise.Resolve(response),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		public void Send(Action<GeocodingResponse> onSuccess, Action<Exception> onFailure) {
			var client = new RestClient();
			var request = new RestRequest(GetURL(), Method.GET);

			client.ExecuteAsync(request, (response, handle) => {
				if (response.IsSuccess()) {
					var model = JsonUtility.FromJson<GeocodingResponse>(response.Content);
					
					if (model != null)
						onSuccess.TryInvoke(model);
					else {
						var exception = new Exception("Could not deserialize", response.GetException());
						onFailure.TryInvoke(exception);
					}
				}
				else {
					var exception = new Exception("Unsuccessful response for Geocoding", response.GetException());
					onFailure.TryInvoke(exception);
				}
			});
		}
	}
}