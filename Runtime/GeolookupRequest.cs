using RestSharp;
using System.Text;
using System.Threading.Tasks;
using System;
using Adrenak.Unex;
using UnityEngine;

namespace Adrenak.UniMap {
	/// <summary>
	/// Used to get the address of a set of coordinates
	/// </summary>
	public class GeolookupRequest {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/geocode/json?";

		/// <summary>
		/// The Google Maps API key being used
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// The Location being looked up currently
		/// </summary>
		public Location Location { get; private set; }

		/// <summary>
		/// Creats an instance for lookup requests
		/// </summary>
		/// <param name="key"></param>
		public GeolookupRequest(string key) {
			Key = key;
		}

		/// <summary>
		/// Gets the URL for the given parameters
		/// </summary>
		/// <returns></returns>
		public string GetURL() {
			if (Key.IsNullOrEmpty())
				throw new Exception("No key provided");

			var builder = new StringBuilder(k_BaseURL);
			builder.Append("key=").Append(Key);
			builder.Append("&latlng=").Append(Location.lat).Append(",").Append(Location.lng);
			
			return builder.ToString();
		}

        /// <summary>
        /// Sends the request for a set of coordinates
        /// </summary>
        /// <param name="lat">The latitude of the coordinates to be looked up</param>
        /// <param name="lng">The longitude of the coordinates to be looked up</param>
        /// <param name="onSuccess">Callback for a successful response</param>
        /// <param name="onFailure">Callback for an unsuccessful response</param>
        public void Send(Location location, Action<GeolookupResponse> onSuccess, Action<Exception> onFailure) {
			Location = location;

            var client = new RestClient();
            var request = new RestRequest(GetURL(), Method.GET);

            client.ExecuteAsync(request, response => {
                Dispatcher.Enqueue(() => {
                    if (response.IsSuccessful()) {
                        var model = JsonUtility.FromJson<GeolookupResponse>(response.Content);
                        if (model != null)
                            onSuccess?.Invoke(model);
                        else {
                            var exception = new Exception("Could not deserialize", response.GetException());
                            onFailure?.Invoke(exception);
                        }
                    }
                    else {
                        var exception = new Exception("Unsuccessful response for Geolookup", response.GetException());
                        onFailure?.Invoke(exception);
                    }
                });
            });
        }

        /// <summary>
        /// Sends the request for a set of coordinates and returns a task for the response
        /// </summary>
        /// <param name="lat">The latitude of the coordinates to be looked up</param>
        /// <param name="lng">The longitude of the coordinates to be looked up</param>
        /// <returns></returns>
        public Task<GeolookupResponse> Send(Location location) {
            var source = new TaskCompletionSource<GeolookupResponse>();
            Send(
                location,
                response => source.SetResult(response),
                exception => source.SetException(exception)
            );
            return source.Task;
        }
	}
}