using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using RestSharp;
using Adrenak.Unex;

namespace Adrenak.UniMap {
	public class GooglePanoDownloader {
		/// <summary>
		/// Invoked everytime a request is finished. Null if the request was not successful
		/// </summary>
		public event Action<Texture32> OnLoaded;


		/// <summary>
		/// Invoked everytime a request fails.
		/// </summary>
		public event Action<Exception> OnFailed;

		/// <summary>
		/// Invoked when a new download starts
		/// </summary>
		public event Action OnStarted;

		/// <summary>
		/// Returns the current Panorama Image texture
		/// </summary>
		Texture32 m_Texture;

		bool m_Running;
		List<RestRequestAsyncHandle> m_Handles = new List<RestRequestAsyncHandle>();

		/// <summary>
		/// Downloads a Google uploaded panorama image using the URL as a Promise
		/// </summary>
		/// <param name="url">The URL of the pano</param>
		/// <param name="size">The <see cref="PanoSize"/> of the pano image to be downloaded</param>
		public IPromise<Texture32> Download(string panoID, PanoSize size, TextureFormat format) {
			var promise = new Promise<Texture32>();
			Download(panoID, size, format,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads a Google uploaded panorama image using the URL
		/// </summary>
		/// <param name="url">The URL of the pano</param>
		/// <param name="size">The <see cref="PanoSize"/> of the pano image to be downloaded</param>
		/// <param name="onResult">Callback for result texture when the download is successful</param>
		/// <param name="onException">Callback for exception when the download fails</param>
		public void Download(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult, Action<Exception> onException) {
			Stop();
			Dispatcher.Add(() => {
				Runner.AutoRun(DownloadCo(panoID, size, format, onResult, onException));
			});
		}

		IEnumerator DownloadCo(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult, Action<Exception> onException) {
			OnStarted.TryInvoke();

			var uRes = PanoUtility.GetUntrimmedResolution(size);

			m_Texture = new Texture32((int)uRes.x, (int)uRes.y);
			var count = PanoUtility.GetTileCount(size);

			int xCount = (int)count.x;
			int yCount = (int)count.y;

			int req = xCount * yCount;
			int success = 0;
			int failed = 0;
			bool done = false;

			for (int i = 0; i < xCount; i++) {
				for (int j = 0; j < yCount; j++) {
					var x = i;
					var y = j;

					DownloadTile(panoID, x, y, size, format)
						.Then(tile => {
							success++;
							StitchTexture(tile, size, x, ((int)count.y - 1) - y);

							if (success == req) {
								done = true;
								CropTexture(size);

								onResult.TryInvoke(m_Texture);
								OnLoaded.TryInvoke(m_Texture);
							}
						})
						.Catch(exception => {
							if (x == 0 && yCount > y) {
								yCount = y;
								req = xCount * yCount;
							}
							if (y == 0 && xCount > x) {
								xCount = x;
								req = xCount * yCount;
							}

							failed++;
							if (failed == req) {
								var thrown = new Exception("Could not download the pano image. ID or URL is incorrect.");
								OnFailed.TryInvoke(thrown);
								onException.TryInvoke(thrown);
								return;
							}
							if (success == req && !done) {
								done = true;
								CropTexture(size);
								onResult.TryInvoke(m_Texture);
								OnLoaded.TryInvoke(m_Texture);
							}
						});
					yield return null;
				}
			}
		}

		IPromise<Texture2D> DownloadTile(string panoID, int x, int y, PanoSize size, TextureFormat format) {
			var promise = new Promise<Texture2D>();
			DownloadTile(panoID, x, y, size, format,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		void DownloadTile(string panoID, int x, int y, PanoSize size, TextureFormat format, Action<Texture2D> onResult = null, Action<Exception> onException = null) {
			m_Running = true;

			var url = "https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en"
				+ "&panoid=" + panoID
				+ "&output=tile"
				+ "&x=" + x
				+ "&y=" + y
				+ "&zoom=" + PanoUtility.GetZoomValue(size)
				+ "&nbt&fover=2";

			RestRequestAsyncHandle handle = null;
			m_Handles.Add(handle);
			var client = new RestClient();
			var request = new RestRequest(url, Method.GET);
			client.ExecuteAsync(request, ref handle)
				.Then(response => {
					if (!m_Running) return;
					Dispatcher.Add(() => {
						if (response.IsSuccess()) {
							var tile = new Texture2D(2, 2, format, true);
							tile.LoadImage(response.RawBytes);
							onResult.TryInvoke(tile);
						}
						else {
							onException.TryInvoke(response.GetException());
						}
					});
				})
				.Catch(exception => {
					if (!m_Running) return;
					onException.TryInvoke(exception);
				});
		}

		public IPromise<bool> IsAvailable(string id) {
			var promise = new Promise<bool>();
			IsAvailable(id, result => {
				promise.Resolve(result);
			});
			return promise;
		}

		/// <summary>
		/// Not all images are uploaded by Google, some panos are User uploads
		/// This fucniton returns whether the pano with the given ID can be downloaded
		/// </summary>
		/// <param name="panoID"></param>
		/// <param name="result"></param>
		public void IsAvailable(string panoID, Action<bool> result) {
			var url = "https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en"
				+ "&panoid=" + panoID
				+ "&output=tile"
				+ "&x=0"
				+ "&y=0"
				+ "&zoom=" + 0
				+ "&nbt&fover=2";

			new RestClient().ExecuteAsync(new RestRequest(url, Method.GET))
				.Then(response => {
					Dispatcher.Add(() => {
						result.TryInvoke(response.IsSuccess());
					});
				})
				.Catch(exception => {
					Debug.LogError(exception);
					Dispatcher.Add(() => {
						result.TryInvoke(false);
					});
				});
		}

		// Copies the tile to the right place in the complete (large) texture
		// On Android devices, Graphics.CopyTexture results in the editor freezing
		// when the PanoSize is set to VeryLast. For this reason we use an extension method
		// called Texture2D.Copy. However Graphics.Copytexture is faster so we use that
		// wherever possible
		void StitchTexture(Texture2D tile, PanoSize size, int x, int y) {
			m_Texture.ReplaceBlock(x * tile.width, y * tile.height, Texture32.FromTexture2D(tile));
			MonoBehaviour.Destroy(tile);
			tile = null;
		}

		// Removes the blank parts of the texture
		// Similar to StitchTexture, we try to use Graphics.CopyTexture wherever possible
		// however on VeryLarge PanoSize on Android devices, we use an extenion method called
		// Texture2D.Crop, which is slower than Graphics.CopyTexture and is a last resort
		void CropTexture(PanoSize level) {
			var uRes = PanoUtility.GetUntrimmedResolution(level);
			var tRes = PanoUtility.DetectBlankBands(m_Texture);
			tRes = new Vector2(
				PanoUtility.DetectWidth(m_Texture),
				tRes.y
			);

			// If the trimmed resolutionm is the same as untrimmed, we don't need to 
			if (tRes.x == uRes.x && tRes.y == uRes.y)
				return;

			m_Texture.Crop(0, (int)uRes.y - (int)tRes.y, (int)tRes.x, (int)tRes.y);
		}

		/// <summary>
		/// Destroys the internal Texture2D
		/// </summary>
		public void ClearTexture() {
			if (m_Texture != null)
				m_Texture.Clear();
			m_Texture = null;
		}

		public void Stop() {
			m_Running = false;
			ClearTexture();
			foreach (var handle in m_Handles) {
				if (handle != null) {
					handle.WebRequest.Abort();
					handle.Abort();
				}
			}
		}
	}
}
