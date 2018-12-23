using Adrenak.Unex;
using System.Text;
using System.Collections;
using System;
using UnityEngine;

namespace Adrenak.UniMap {
	public class NearbySearchRequest {
		/// <summary>
		/// Lists the different ways in which the result can be listed
		/// </summary>
		public enum RankBy {
			Prominence,
			Distance
		}

		public const string k_BaseURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";

		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string key;

		/// <summary>
		/// The <see cref="Location"/> object that represents the coordinates around which the nearby places should be searched for.
		/// </summary>
		public Location location;

		/// <summary>
		/// The radius (in metres) around the location that should be searched for nearby places
		/// </summary>
		public int radius = 50;

		/// <summary>
		/// The minimum price level of the results returned
		/// </summary>
		public PriceLevel minPriceLevel = PriceLevel.VeryLow;

		/// <summary>
		///  The maximum price level of the results returned
		/// </summary>
		public PriceLevel maxPriceLevel = PriceLevel.VeryHigh;

		/// <summary>
		/// Keyword that should be associated with the nearby places (Recommended over <see cref="name"/>
		/// </summary>
		public string keyword;

		/// <summary>
		/// The language in which the returls are returned
		/// </summary>
		public string language;

		/// <summary>
		/// A term to be matched against all content that Google has indexed for a place. 
		/// </summary>
		[Obsolete("Prefer using keyword instead")]
		public string name;

		/// <summary>
		/// If trye the results only include the places that are currently opened
		/// </summary>
		public bool isOpenNow;

		/// <summary>
		/// Specifies the oerder in which the results are listed. 
		/// </summary>
		public RankBy rankBy = RankBy.Prominence;

		/// <summary>
		/// The type of the places returned in the results. See <see cref="PlaceType"/>
		/// </summary>
		public PlaceType type = PlaceType.Undefined;

		// ================================================
		// PUBLIC METHODS
		// ================================================
		/// <summary>
		/// Gets the request URL for the set parameters
		/// </summary>
		/// <returns></returns>
		public string GetURL() {
			if (string.IsNullOrEmpty(key))
				throw new Exception("Key cannot be null or empty");
			if (location == null)
				throw new Exception("Location cannot be null");

			var sb = new StringBuilder(k_BaseURL);

			// Add the parameters that are gaurunteed a value
			sb.Append("key=").Append(key)
				.Append("&location=").Append(location.lat).Append(",").Append(location.lng)
				.Append("&radius=").Append(radius)
				.Append("&minprice=").Append(minPriceLevel)
				.Append("&maxprice=").Append(maxPriceLevel)
				.Append("&rankby=").Append(RankByToString(rankBy));
			
			// Check and add the other parameters
			if (!string.IsNullOrEmpty(keyword))
				sb.Append("&keyword=").Append(keyword);

			if (!string.IsNullOrEmpty(language))
				sb.Append("&language=").Append(language);

#pragma warning disable 0618
			if (!string.IsNullOrEmpty(name))
				sb.Append("&name=").Append(name);
#pragma warning restore 0618

			if (type != PlaceType.Undefined)
				sb.Append("&type=").Append(EnumToString.From(type));

			if (isOpenNow)
				sb.Append("&opennow");

			return sb.ToString();
		}

		/// <summary>
		/// Send the API request and returns the response or exception
		/// </summary>
		/// <param name="onResult"></param>
		public void Send(Action<NearbySearchResponse> onResult, Action<Exception> onException) {
			CoroutineRunner.Instance.StartCoroutine(SendAsync(onResult, onException));
		}

		/// <summary>
		/// Send the API request and return a promise for the response
		/// </summary>
		public IPromise<NearbySearchResponse> Send() {
			var promise = new Promise<NearbySearchResponse>();
			Send(
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		// ================================================
		// INNER METHODS
		// ================================================
		IEnumerator SendAsync(Action<NearbySearchResponse> onResult, Action<Exception> onException) {
			string url;
			try {
				url = GetURL();
			}
			catch (Exception e) {
				onException(e);
				yield break;
			}

			WWW request = new WWW(url);
			yield return request;

			if (!string.IsNullOrEmpty(request.error)) {
				onException(new Exception(request.error + request.text));
				yield break;
			}
			else {
				try {
					onResult(JsonUtility.FromJson<NearbySearchResponse>(request.text));
				}
				catch (Exception e) {
					onException(e);
				}
			}
		}

		string RankByToString(RankBy rankBy) {
			switch (rankBy) {
				case RankBy.Distance:
					return "distance";
				case RankBy.Prominence:
					return "prominence";
				default:
					return "prominence";
			}
		}
	}
}
