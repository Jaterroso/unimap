using System;
using UnityEngine;
using System.Threading.Tasks;
using RestSharp;
using Adrenak.Unex;

namespace Adrenak.UniMap {
	public class UserPanoDownloader {
		/// <summary>
		/// Invoked everytime a request is finished. Null if the request was not successful
		/// </summary>
		public event Action<Texture32> OnLoaded;

		public event Action<Exception> OnFailed;

		public event Action OnStarted;

		Texture32 m_Texture;
		RestRequestAsyncHandle m_Handle;
		bool m_Running;

        /// <summary>
        /// Downloads the panorama image using the known PanoID as a Promise
        /// </summary>
        /// <param name="id">The Pano ID to be downloaded</param>
        /// <param name="size">The <see cref="PanoSize"/> of the image to be downloaded</param>
        /// <returns></returns>
        public Task<Texture32> Download(string id, PanoSize size, TextureFormat format) {
            var promise = new TaskCompletionSource<Texture32>();
            Download(id, size, format,
                result => promise.SetResult(result),
                exception => promise.SetException(exception)
            );
            return promise.Task;
        }

        /// <summary>
        /// Downloads the panorama image using the known PanoID
        /// </summary>
        /// <param name="panoID">The PanoID to be downloaded</param>
        /// <param name="size">The <see cref="PanoSize"/> of the image to be downloaded.</param>
        /// <param name="onResult">Callback containing the Texture2D of the pano image</param>
        /// <param name="onException">Callback containing the exception when the download fails</param>
        public void Download(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult = null, Action<Exception> onException = null) {
			var width = PanoUtility.GetUserPanoWidth(size);
			string url = "https://lh5.googleusercontent.com/p/" + panoID + "=w" + width;
			m_Running = true;

			OnStarted?.Invoke();
            var client = new RestClient();
            var request = new RestRequest(url, Method.GET);
            client.ExecuteAsync(request, response => {
                if (!m_Running) return;

                Dispatcher.Enqueue(() => {
                    if (response.IsSuccessful()) {
                        var texture = new Texture2D(1, 1, format, true);
                        texture.LoadImage(response.RawBytes);
                        var result = Texture32.FromTexture2D(texture);
                        MonoBehaviour.Destroy(texture);
                        texture = null;

                        onResult?.Invoke(result);
                        OnLoaded?.Invoke(result);
                    }
                    else {
                        OnFailed?.Invoke(response.GetException());
                        onException?.Invoke(response.GetException());
                    }
                });
            });
        }

        /// <summary>
        /// Not all panorama images are uploaded by Google users
        /// This function returns whether the panorama of the given ID can be downloaded as a User Pano as a Promise
        /// </summary>
        /// <param name="id">The Panorama ID to be checked</param>
        /// <returns>Whether the instance can download the pano</returns>
        public Task<bool> IsAvailable(string id) {
            var source = new TaskCompletionSource<bool>();
            IsAvailable(id, result => source.SetResult(result));
            return source.Task;
        }

        /// <summary>
        /// Not all panorama images are uploaded by Google users
        /// This function returns whether the panorama of the given ID can be downloaded as a User Pano
        /// </summary>
        /// <param name="panoID">The Panorama ID to be checked</param>
        /// <param name="result">Whether the pano can be downloaded</param>
        public void IsAvailable(string panoID, Action<bool> result) {
			string url = "https://lh5.googleusercontent.com/p/" + panoID + "=w" + 1;
            new RestClient().ExecuteAsync(new RestRequest(url, Method.GET), response => { 
				Dispatcher.Enqueue(() => {
					result?.Invoke(response.IsSuccessful());
				});            
            });
		}

		/// <summary>
		/// Destroys the internal Texture2D
		/// </summary>
		public void ClearTexture() {
			if (m_Texture != null) {
				m_Texture.Clear();
				m_Texture = null;
			}
		}

		public void Stop() {
			m_Running = false;
			ClearTexture();
			if (m_Handle != null)
				m_Handle.Abort();
		}
	}
}
