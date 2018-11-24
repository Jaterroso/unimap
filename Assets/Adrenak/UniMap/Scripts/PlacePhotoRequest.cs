#if UNIMAP_RSG_PROMISES
using RSG;
#endif

using System;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Adrenak.UniMap {
	public class PlacePhotoDownloader {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/place/photo?";

		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string key;

		/// <summary>
		/// Reference ID of the image that has to be downloaded
		/// </summary>
		public string reference;

		/// <summary>
		/// Maximum width of the image to be downloaded
		/// </summary>
		public int maxWidth;

		/// <summary>
		/// Maximum height of the image to be downloaded
		/// </summary>
		public int maxHeight;

		/// <summary>
		/// Gets the URL of the request given the current parameter values
		/// </summary>
		/// <returns></returns>
		public string GetURL() {
			var builder = new StringBuilder(k_BaseURL);
			builder.Append("&key=").Append(key)
				.Append("&photoreference=").Append(reference)
				.Append("&maxheight=").Append(maxHeight)
				.Append("&maxwidth=").Append(maxWidth);

			return builder.ToString();
		}

#if UNIMAP_RSG_PROMISES
		/// <summary>
		/// Send the API request and return a promise for the response
		/// </summary>
		public IPromise<Texture2D> Download() {
			var promise = new Promise<Texture2D>();
			Download(
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}
#endif

		/// <summary>
		/// Send the API request and returns the response or exception
		/// </summary>
		/// <param name="onResponse">Action that returns the response as a c# object</param>
		/// <param name="onException">Action that returns the exception encountered in case of an error</param>
		public void Download(Action<Texture2D> onResult, Action<Exception> onException) {
			CoroutineRunner.Instance.StartCoroutine(DownloadAsync(onResult, onException));
		}

		IEnumerator DownloadAsync(Action<Texture2D> onResult, Action<Exception> onException) {
			var url = GetURL();
			WWW www = new WWW(url);
			yield return www;

			if (string.IsNullOrEmpty(www.error)) {
				var tex = new Texture2D(1, 1);
				www.LoadImageIntoTexture(tex);
				onResult(www.texture);
			}
			else {
				// Try once more
				www = new WWW(url);
				yield return www;

				if (string.IsNullOrEmpty(www.error)) {
					try {
						var tex = new Texture2D(1, 1);
						www.LoadImageIntoTexture(tex);
						onResult(www.texture);
					}
					catch (Exception e) {
						onException(e);
					}
				}
				else {
					onException(new Exception(www.error));
				}
			}
		}
	}
}
