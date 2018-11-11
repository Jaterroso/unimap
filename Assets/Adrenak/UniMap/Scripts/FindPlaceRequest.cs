#if UNIMAP_RSG_PROMISES
using RSG;
#endif

using System.Text;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class FindPlaceRequest {
		// ================================================
		// INNER TYPES
		// ================================================
		/// <summary>
		/// The mode of input for the search query
		/// </summary>
		public enum InputType {
			TextQuery,
			PhoneNumber
		}

		/// <summary>
		/// represents the types of fields that the response can contain
		/// </summary>
		public enum Field {
			// BASIC
			FormattedAddress,
			Geometry,
			Icon,
			ID,
			Name,
			PermanentlyClosed,
			Photos,
			PlaceID,
			PlusCode,
			Scope,
			Types,

			// CONTACT
			OpenNow,

			//ATMOSPHERE
			PriceLevel,
			Rating
		}

		/// <summary>
		/// Used to prefer results from a specified area
		/// </summary>
		public enum LocationBias {
			IP,
			Point,
			Circular,
			Rectangular
		}

		const string k_BaseURL = "https://maps.googleapis.com/maps/api/place/findplacefromtext/json?";

		// ================================================
		// PARAMETERS
		// ================================================
		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string key;

		/// <summary>
		/// The query for the search. Eg. "Eiffel Tower" or "+1 1234567890"
		/// </summary>
		public string input = string.Empty;

		/// <summary>
		/// Input type for the query. Supported: Text and Phone number
		/// </summary>
		public InputType inputType = InputType.TextQuery;

		/// <summary>
		/// The language in which the results will be returned
		/// </summary>
		public string language;

		/// <summary>
		/// The types of places that can be returned by the response
		/// </summary>
		public List<Field> fields = new List<Field>();

		/// <summary>
		/// Prefer results in a specified area, by specifying either a radius plus lat/lng, 
		/// or two lat/lng pairs representing the points of a rectangle. 
		/// If this parameter is not specified, the API uses IP address biasing by default. 
		/// </summary>
		public LocationBias locationBias = LocationBias.IP;

		/// <summary>
		/// (Default:50) The radius of the search circle if the <see cref="locationBias"/> is set to <see cref="LocationBias.Circular"/>
		/// </summary>
		public int radius = 50;

		/// <summary>
		/// (Default:0,0) The co-ordinate of search if the <see cref="locationBias"/> is set to <see cref="LocationBias.Circular"/> or <see cref="LocationBias.Point"/>
		/// </summary>
		public Location coordinates = new Location(0, 0);

		/// <summary>
		/// (Default:0,0) The location representing South West of the rectangular search area if <see cref="locationBias"/> is set to <see cref="LocationBias.Rectangular"/>
		/// </summary>
		public Location southWest = new Location(0, 0);

		/// <summary>
		/// (Default:0,0) The location representing North East of the rectangular search area if <see cref="locationBias"/> is set to <see cref="LocationBias.Rectangular"/>
		/// </summary>
		public Location northEast = new Location(0, 0);

		// ================================================
		// PUBLIC METHODS
		// ================================================
		/// <summary>
		/// Gets the request URL for the set parameters
		/// </summary>
		public string GetURL() {
			// Ensure mandatory parameters
			if (string.IsNullOrEmpty(key))
				throw new Exception("Key is null or empty");
			if (string.IsNullOrEmpty(input))
				throw new Exception("Input is null or empty");

			var sb = new StringBuilder(k_BaseURL)
				.Append("key=").Append(key)
				.Append("&input=").Append(input)
				.Append("&inputtype=").Append(InputTypeToString(inputType));

			if(!string.IsNullOrEmpty(language))
				sb.Append("&language=").Append(language);
				
			// Add the fields separated by commas
			if(fields.Count > 0) {
				sb.Append("&fields=");
				for (int i = 0; i < fields.Count; i++) {
					sb.Append(FieldToString(fields[i]));
					if (i != fields.Count - 1)
						sb.Append(",");
				}
			}

			// Add the location bias based on the type selected
			switch (locationBias) {
				case LocationBias.IP:
					sb.Append("&locationbias=ipbias");
					break;
				case LocationBias.Point:
					sb.Append("&point:").Append(coordinates.lat).Append(",").Append(coordinates.lng);
					break;
				case LocationBias.Circular:
					sb.Append("&circle:").Append(radius).Append("@").Append(coordinates.lat).Append(",").Append(coordinates.lng);
					break;
				case LocationBias.Rectangular:
					sb.Append("&rectangular:")
						.Append(southWest.lat).Append(",").Append(southWest.lng)
						.Append(northEast.lat).Append(",").Append(northEast.lng);
					break;
			}
			return sb.ToString();
		}

		/// <summary>
		/// Send the API request and returns the response or exception
		/// </summary>
		/// <param name="onResult">Action that returns the response as a c# object</param>
		/// <param name="onException">Action that returns the exception encountered in case of an error</param>
		public void Send(Action<PlaceSearchResponse> onResult, Action<Exception> onException) {
			CoroutineRunner.Instance.StartCoroutine(SearchCo(onResult, onException));
		}

#if UNIMAP_RSG_PROMISES
		/// <summary>
		/// Send the API request and return a promise for the response
		/// </summary>
		public IPromise<PlaceSearchResponse> SendAsync() {
			var promise = new Promise<PlaceSearchResponse>();
			Send(
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}
#endif

		// ================================================
		// INNER METHODS
		// ================================================
		IEnumerator SearchCo(Action<PlaceSearchResponse> onResult, Action<Exception> onException) {
			// Try to get the url
			string url;
			try {
				url = GetURL();
			}
			catch(Exception e) {
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
					var result = JsonUtility.FromJson<PlaceSearchResponse>(request.text);
					onResult(result);
				}
				catch(Exception e) {
					onException(e);
				}
			}
		}

		string InputTypeToString(InputType type) {
			switch (type) {
				case InputType.PhoneNumber:
					return "phonenumber";
				case InputType.TextQuery:
					return "textquery";
				default:
					return "textquery";
			}
		}

		string FieldToString(Field field) {
			switch (field) {
				case Field.FormattedAddress:
					return "formatted_address";
				case Field.Geometry:
					return "geometry";
				case Field.Icon:
					return "icon";
				case Field.ID:
					return "id";
				case Field.Name:
					return "name";
				case Field.PermanentlyClosed:
					return "permanently_closed";
				case Field.Photos:
					return "photos";
				case Field.PlaceID:
					return "place_id";
				case Field.PlusCode:
					return "plus_code";
				case Field.Scope:
					return "scope";
				case Field.Types:
					return "types";

				case Field.OpenNow:
					return "open_now";

				case Field.PriceLevel:
					return "price_level";
				case Field.Rating:
					return "rating";
				default:
					return string.Empty;
			}
		}
	}
}
