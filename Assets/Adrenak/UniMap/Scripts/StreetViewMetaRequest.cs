using Adrenak.Unex;
using System.Text;
using System.Collections;
using System;
using UnityEngine;

namespace Adrenak.UniMap {
	public class StreetViewMetaRequest {
		/// <summary>
		/// The base URL of the Street View API
		/// </summary>
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/streetview/metadata?";

		/// <summary>
		/// The options for the meta data request
		/// </summary>
		public StreetView.Options options = new StreetView.Options();

		/// <summary>
		/// Gets the request URL for the given face given the API parameters.
		/// </summary>
		/// <param name="face">The <see cref="Face"/> whose texture is to be downloaded.</param>
		/// <returns>The URL for downloading the texture of this <see cref="Face"/></returns>
		public string GetURL() {
			var sb = new StringBuilder(k_BaseURL).Append("key=").Append(options.key);

			switch (options.mode) {
				case StreetView.Mode.Coordinates:
					sb.Append("&location=").Append(options.location.lat).Append(",").Append(options.location.lng);
					break;
				case StreetView.Mode.PanoID:
					sb.Append("&pano=").Append(options.panoID);
					break;
				case StreetView.Mode.Location:
					sb.Append("&location=").Append(options.place);
					break;
			}

			sb.Append("&radius=").Append(options.radius)
			.Append("&source=").Append(EnumToString.From(options.source));

			return sb.ToString();
		}

		/// <summary>
		/// Send the API request and returns the response or exception
		/// </summary>
		/// <param name="onResponse">Action that returns the response as a C# object</param>
		/// <param name="onException">Action that returns the exception encountered in case of an error</param>
		public void Send(Action<StreetViewMetaResponse> onResponse, Action<Exception> onException) {
			CoroutineRunner.Instance.StartCoroutine(SendAsync(onResponse, onException));
		}

		/// <summary>
		/// Send the API request and return a promise for the response
		/// </summary>
		public IPromise<StreetViewMetaResponse> Send() {
			var promise = new Promise<StreetViewMetaResponse>();
			Send(
				response => promise.Resolve(response),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		IEnumerator SendAsync(Action<StreetViewMetaResponse> onResponse, Action<Exception> onException) {
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
					onResponse(JsonUtility.FromJson<StreetViewMetaResponse>(request.text));
				}
				catch (Exception e) {
					onException(e);
				}
			}
		}
	}
}