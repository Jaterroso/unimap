using System;
using System.Collections.Generic;
using UnityEngine;
using RestSharp;
using Adrenak.Unex;

namespace Adrenak.UniMap {
	public class GooglePanoDownloader {
		/// <summary>
		/// Invoked everytime a request is finished. Null if the request was not successful
		/// </summary>
		public event Action<Texture2D> OnLoaded;

		/// <summary>
		/// Returns the current Panorama Image texture
		/// </summary>
		public Texture2D PanoTexture {
			get { return m_Texture; }
		}
		Texture2D m_Texture;

		bool m_Running;
		List<RestRequestAsyncHandle> m_Handles = new List<RestRequestAsyncHandle>();

		/// <summary>
		/// Downloads a Google uploaded panorama image using the URL as a Promise
		/// </summary>
		/// <param name="url">The URL of the pano</param>
		/// <param name="size">The <see cref="PanoSize"/> of the pano image to be downloaded</param>
		public IPromise<Texture2D> Download(string panoID, PanoSize level) {
			var promise = new Promise<Texture2D>();
			Download(panoID, level,
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
		public void Download(string panoID, PanoSize size, Action<Texture2D> onResult, Action<Exception> onException) {
			var uRes = PanoUtility.GetUntrimmedResolution(size);

			if (m_Texture != null)
				MonoBehaviour.Destroy(m_Texture);
			m_Texture = new Texture2D((int)uRes.x, (int)uRes.y, TextureFormat.RGB24, false);

			m_Handles.Clear();

			var count = PanoUtility.GetTileCount(size);

			int total = (int)count.x * (int)count.y;
			int all = 0;
			int failed = 0;

			for (int i = 0; i < (int)count.x; i++) {
				for (int j = 0; j < (int)count.y; j++) {
					var x = i;
					var y = j;

					DownloadTile(panoID, x, y, size)
						.Then(tile => {
							all++;

							StitchTexture(tile, size, x, ((int)count.y - 1) - y);
							if (all == total) {
								Debug.Log("done");
								CropTexture(size);
								onResult.TryInvoke(m_Texture);
								OnLoaded.TryInvoke(m_Texture);
							}
						})
						.Catch(exception => {
							all++;
							failed++;

							if (failed == total) {
								onException(new Exception("Could not download the pano image. ID or URL is incorrect."));
								return;
							}

							if (all == total) {
								Debug.Log("done");
								CropTexture(size);
								onResult.TryInvoke(m_Texture);
								OnLoaded.TryInvoke(m_Texture);
							}
						});
				}
			}
		}

		/// <summary>
		/// Downloads a 512x512 tile of the pano identified using the PanoID as a Promise
		/// </summary>
		/// <param name="panoID">The Panorama ID from which the tiles have to be downloaded</param>
		/// <param name="x">The X index of the tile</param>
		/// <param name="y">The Y index of the tile</param>
		/// <param name="size">The <see cref="PanoSize"/> of the image of the panorama to be downloaded</param>
		public IPromise<Texture2D> DownloadTile(string panoID, int x, int y, PanoSize size) {
			var promise = new Promise<Texture2D>();
			DownloadTile(panoID, x, y, size,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads a 512x512 tile of the pano identified using the PanoID
		/// </summary>
		/// <param name="panoID">The Panorama ID from which the tiles have to be downloaded</param>
		/// <param name="size">The <see cref="PanoSize"/> of the image of the panorama to be downloaded</param>
		/// <param name="x">The X index of the tile</param>
		/// <param name="y">The Y index of the tile</param>
		/// <param name="onResult">Callback with the tile texture</param>
		/// <param name="onException">Callback for the exception when the download fails</param>
		public void DownloadTile(string panoID, int x, int y, PanoSize size, Action<Texture2D> onResult = null, Action<Exception> onException = null) {
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
			new RestClient().ExecuteAsync(new RestRequest(url, Method.GET), ref handle)
				.Then(response => {
					if (!m_Running) return;
					Dispatcher.Add(() => {
						if (response.IsSuccess()) {
							var tile = new Texture2D(2, 2, TextureFormat.RGB24, false);
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
			if (size != PanoSize.VeryLarge) {
				Graphics.CopyTexture(tile, 0, 0, 0, 0, tile.width, tile.height, m_Texture, 0, 0, x * tile.width, y * tile.height);
			}
			else {
#if UNITY_ANDROID
				m_Texture.Copy(tile, new Vector2(x, y) * 512, false);
#elif UNITY_EDITOR || UNITY_STANDALONE
				Graphics.CopyTexture(tile, 0, 0, 0, 0, tile.width, tile.height, m_Texture, 0, 0, x * tile.width, y * tile.height);
#else
				m_Texture.Copy(tile, new Vector2(x, y) * 512, false);
#endif
			}
			MonoBehaviour.Destroy(tile);
		}

		// Removes the blank parts of the texture
		// Similar to StitchTexture, we try to use Graphics.CopyTexture wherever possible
		// however on VeryLarge PanoSize on Android devices, we use an extenion method called
		// Texture2D.Crop, which is slower than Graphics.CopyTexture and is a last resort
		void CropTexture(PanoSize level) {
			var uRes = PanoUtility.GetUntrimmedResolution(level);
			var tRes = PanoUtility.DetectTrimmedResolution(m_Texture);

			if (level != PanoSize.VeryLarge) {
				var trimmed = new Texture2D((int)tRes.x, (int)tRes.y, TextureFormat.RGB24, false);
				Graphics.CopyTexture(m_Texture, 0, 0,
					0,
					(int)uRes.y - (int)tRes.y,
					(int)tRes.x,
					(int)tRes.y,
					trimmed, 0, 0, 0, 0
				);
				MonoBehaviour.Destroy(m_Texture);
				m_Texture = trimmed;
			}
			else {
#if UNITY_ANDROID
				var trimmed = m_Texture.Crop(new Rect(0, uRes.y - tRes.y, tRes.x, tRes.y));
				MonoBehaviour.Destroy(m_Texture);
				m_Texture = trimmed;
#else
				var trimmed = new Texture2D((int)tRes.x, (int)tRes.y, TextureFormat.RGB24, false);
				Graphics.CopyTexture(m_Texture, 0, 0,
					0,
					(int)(uRes.y - tRes.y),
					(int)tRes.x,
					(int)tRes.y,
					trimmed, 0, 0, 0, 0
				);
				MonoBehaviour.Destroy(m_Texture);
				m_Texture = trimmed;
#endif
			}
		}

		/// <summary>
		/// Destroys the internal Texture2D
		/// </summary>
		public void ClearTexture() {
			if (m_Texture != null)
				MonoBehaviour.Destroy(m_Texture);
		}

		public void Stop() {
			m_Running = false;
			foreach (var handle in m_Handles) {
				if (handle != null)
					handle.Abort();
			}
		}
	}
}
