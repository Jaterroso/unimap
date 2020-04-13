using Adrenak.Unex;
using System;
using RestSharp;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Adrenak.UniMap {
	public class TextSearchRequest {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/place/textsearch/json?";

		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string key;

		/// <summary>
		/// The query for the search. Eg. "Restaurants in Sydney"
		/// </summary>
		public string query;

		/// <summary>
		/// Region from where the results are preferred. This is not a hard filter. Uses ccTLD naming.
		/// See https://www.ionos.com/digitalguide/domains/domain-extensions/cctlds-a-list-of-every-country-domain/
		/// </summary>
		public string region;

		/// <summary>
		/// The location around which the search is to be done. 
		/// </summary>
		public Location location;

		/// <summary>
		/// Search distance (in metres) from the given location. Maximum value is 50000
		/// </summary>
		public int radius = -1;

		/// <summary>
		/// The language in which the results are to be returned. Uses ISO naming.
		/// </summary>
		public string language;

		/// <summary>
		/// The minimum price level of the results returned
		/// </summary>
		public PriceLevel minPriceLevel = PriceLevel.Undefined;

		/// <summary>
		/// The maximum price level of the results return
		/// </summary>
		public PriceLevel maxPriceLevel = PriceLevel.Undefined;

		/// <summary>
		/// When set to true, makes sure that only the places that are currently opened are returned
		/// </summary>
		public bool isOpenNow;

		/// <summary>
		/// Restricts the results to a particular type of place
		/// </summary>
		public PlaceType type;

		// ================================================
		// PUBLIC METHODS
		// ================================================
		public TextSearchRequest(string key) {
			this.key = key;
		}

		/// <summary>
		/// Gets the request URL for the set parameters
		/// </summary>
		public string GetURL() {
			if (string.IsNullOrEmpty(key))
				throw new Exception("Key cannot be null or empty");
			if (string.IsNullOrEmpty(query))
				throw new Exception("Query cannot be null or empty");
			if (radius > 50000)
				throw new Exception("Radius cannot be larger than 50000");

			var sb = new StringBuilder(k_BaseURL)
				.Append("key=").Append(key)
				.Append("&query=").Append(query);

			if (!string.IsNullOrEmpty(region))
				sb.Append("&region=").Append(region);

			if (location != null)
				sb.Append("&location=").Append(location.lat).Append(",").Append(location.lng);

			if (radius >= 0)
				sb.Append("&radius=").Append(radius);

			if (!string.IsNullOrEmpty(language))
				sb.Append("&language=").Append(language);

			if (minPriceLevel != PriceLevel.Undefined)
				sb.Append("&minprice=").Append(minPriceLevel);

			if (maxPriceLevel != PriceLevel.Undefined)
				sb.Append("&maxprice=").Append(maxPriceLevel);

			if (isOpenNow)
				sb.Append("&opennow");

			if (type != PlaceType.Undefined)
				sb.Append("&type=").Append(EnumToString.From(type));

			return sb.ToString();
		}

		/// <summary>
		/// Send the API request and returns the response or exception
		/// </summary>
		/// <param name="query">The  query for the search</param>
		/// <param name="radius">The radius from the location within which the results should be returned</param>
		/// <param name="onResponse">Action that returns the response as a c# object</param>
		/// <param name="onException">Action that returns the exception encountered in case of an error</param>
		public void Send(string query, int radius, Action<TextSearchResponse> onResponse, Action<Exception> onException) {
			string url = string.Empty;
			try {
				url = GetURL();
			}
			catch (Exception e) {
				onException(e);
			}

            var client = new RestClient();
            var request = new RestRequest(url, Method.GET);
            client.ExecuteAsync(request, response => {
                Dispatcher.Enqueue(() => {
                    if (response.IsSuccessful()) {
                        try {
                            var result = JsonUtility.FromJson<TextSearchResponse>(response.Content);
                            onResponse?.Invoke(result);
                        }
                        catch (Exception e) {
                            onException?.Invoke(e);
                        }
                    }
                    else
                        onException?.Invoke(response.GetException());
                });
            });
        }

        /// <summary>
        /// Sends the API request and returns a task for the response
        /// </summary>
        /// <param name="query">The  query for the search</param>
        /// <param name="radius">The radius from the location within which the results should be returned</param>
        public Task<TextSearchResponse> Send(string query, int radius) {
            var promise = new TaskCompletionSource<TextSearchResponse>();
            Send(
                query, radius,
                result => promise.SetResult(result),
                exception => promise.SetException(exception)
            );
            return promise.Task;
        }
    }
}
