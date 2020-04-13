using Adrenak.Unex;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RestSharp;

namespace Adrenak.UniMap {
	public class PlacePhotoDownloader {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/place/photo?";

		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Reference ID of the image that has to be downloaded
		/// </summary>
		public string Reference { get; private set; }

        /// <summary>
        /// Maximum width of the image to be downloaded. Default: 512
        /// </summary>
        public int MaxWidth { get; set; } = 512;

        /// <summary>
        /// Maximum height of the image to be downloaded. Default: 512
        /// </summary>
        public int MaxHeight { get; set; } = 512;

        /// <summary>
        /// The TextureFormat of the texture that is downladed by the instance. Default: RGB565
        /// </summary>
        public TextureFormat Format { get; set; } = TextureFormat.RGB565;

		/// <summary>
		/// Whether mip maps of the download texture are generated
		/// </summary>
		public bool IsMipMapped { get; set; }

		/// <summary>
		/// Creates an instance to download the images
		/// </summary>
		/// <param name="key">The Google API key</param>
		public PlacePhotoDownloader(string key) {
			Key = key;
		}

		/// <summary>
		/// Gets the URL of the request given the current parameter values
		/// </summary>
		/// <returns></returns>
		public string GetURL() {
			var builder = new StringBuilder(k_BaseURL);
			builder.Append("&key=").Append(Key)
				.Append("&photoreference=").Append(Reference)
				.Append("&maxheight=").Append(MaxHeight)
				.Append("&maxwidth=").Append(MaxWidth);

			return builder.ToString();
		}

        /// <summary>
        /// Send the API request and returns the response or exception
        /// </summary>
        /// <param name="onResponse">Action that returns the response as a c# object</param>
        /// <param name="onException">Action that returns the exception encountered in case of an error</param>
        public void Download(string reference, Action<Texture2D> onResult, Action<Exception> onException) {
			Reference = reference;

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
						var tex = new Texture2D(1, 1, Format, IsMipMapped);
						tex.LoadImage(response.RawBytes);
						onResult?.Invoke(tex);
					}
					else
						onException?.Invoke(response.GetException());
				});            
            });
		}

        /// <summary>
        /// Send the API request and return a task for the response
        /// </summary>
        public Task<Texture2D> Download(string reference) {
            var source = new TaskCompletionSource<Texture2D>();
            Download(
                reference,
                result => source.SetResult(result),
                exception => source.SetException(exception)
            );
            return source.Task;
        }
	}
}
