using Adrenak.Unex;
using System.Net;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Adrenak.UniMap {
	/// <summary>
	/// Used to download Street View images from URLs or Panorama IDs. This class is HEAVY as it uses a texture internally that is upto 4096x2048 pixels in size.
	/// </summary>
	public class PanoDownloader : IDisposable {
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

		bool m_Disposed;

		/// <summary>
		/// COnstructs a new downloader instance
		/// </summary>
		public PanoDownloader() {
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(MyRemoteCertificateValidationCallback);
		}

		// ================================================
		// BOTH PANOS
		// ================================================
		public IPromise<Texture2D> Download(string id) {
			var promise = new Promise<Texture2D>();
			Download(id,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads a pano from the Pano ID
		/// </summary>
		/// <param name="panoID">The ID of the pano image that needs to be downloaded</param>
		/// <param name="onResult">Callback with the texture in case of successful download. Else null.</param>
		public void Download(string panoID, Action<Texture2D> onResult = null, Action<Exception> onException = null) {
			// First try downloading assuming the pano is a user pano
			DownloadUserPano(panoID,
				userResult => {
					InvokeCallback(onResult, userResult);
					InvokeCallback(OnLoaded, userResult);
				},

				// If the panorama cannot be downloaded from User, then try the Tile download approach
				userException => {
					DownloadGooglePano(panoID,
						googleResult => {
							InvokeCallback(onResult, googleResult);
							InvokeCallback(OnLoaded, googleResult);
						},
						googleException => {
							InvokeCallback(onException, googleException);
						}
					);
				}
			);
		}

		// ================================================
		// USER PANOS
		// ================================================
		public IPromise<Texture2D> DownloadUserPano(string id) {
			var promise = new Promise<Texture2D>();
			DownloadUserPano(id,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads the panorama image using the known PanoID
		/// </summary>
		/// <param name="id">The PanoID to be downloaded</param>
		/// <param name="onResult">Callback containing the Texture2D of the pano image</param>
		public void DownloadUserPano(string id, Action<Texture2D> onResult = null, Action<Exception> onException = null) {
			string url = "https://lh5.googleusercontent.com/p/" + id + "=w4096";
			try {
				using (WebClient client = new WebClient()) {
					client.DownloadDataCompleted += delegate (object sender, DownloadDataCompletedEventArgs e) {
						if (m_Disposed) return;
						Dispatcher.Instance.Enqueue(() => {
							if (e.Error == null) {
								MonoBehaviour.Destroy(m_Texture);
								m_Texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
								m_Texture.LoadImage(e.Result);
								InvokeCallback(onResult, m_Texture);
								InvokeCallback(OnLoaded, m_Texture);
							}
							else {
								InvokeCallback(onException, e.Error);
							}
						});
					};
					client.DownloadDataAsync(new Uri(url));
				}
			}
			catch (Exception e) {
				InvokeCallback(onException, e);
			}
		}

		// ================================================
		// GOOGLE PANOS
		// ================================================
		/// <summary>
		/// Downloads a Google uploaded panorama using a promise
		/// </summary>
		/// <param name="id">The pano ID is to be downloaded</param>
		/// <returns>A Texture2D Promise</returns>
		public IPromise<Texture2D> DownloadGooglePano(string id) {
			var promise = new Promise<Texture2D>();
			DownloadGooglePano(id,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads a Google uploaded panorama image using the URL
		/// </summary>
		/// <param name="url">The URL of the pano</param>
		/// <param name="onResult">Callback for result</param>
		/// <param name="onException">Callback for exception on error</param>
		public void DownloadGooglePano(string panoID, Action<Texture2D> onResult, Action<Exception> onException) {
			MonoBehaviour.Destroy(m_Texture);
			m_Texture = new Texture2D(4096, 2048, TextureFormat.RGB24, false);

			int all = 0;
			int failed = 0;
			for (int i = 0; i < 8; ++i) {
				for (int j = 0; j < 4; ++j) {
					var x = i;
					var y = j;
					DownloadGooglePanoTile(panoID, i, j,
						tileResult => {
							Dispatcher.Instance.Enqueue(() => {
								all++;
								CopyTileToTexture(tileResult, x, 3 - y);
								if (all == 32) {
									if (NeedsTrimming())
										TrimTexture();
									InvokeCallback(onResult, m_Texture);
									InvokeCallback(OnLoaded, m_Texture);
								}
							});
						},

						// Exception sometimes occuer for some tiles. Usually 28 out of 32 tiles are downloaded.
						// But in the case that the pano cannot be downloaded at all, for example when the download
						// URL or the panoID is wrong, then we will only get exceptions
						tileException => {
							Dispatcher.Instance.Enqueue(() => {
								all++;
								failed++;

								if (all == 32) {
									if (NeedsTrimming())
										TrimTexture();
									InvokeCallback(onResult, m_Texture);
									InvokeCallback(OnLoaded, m_Texture);
								}

								// If the failed counter is 32, that means that the pano download has completely failed
								if (failed == 32)
									onException(new Exception("Could not download the pano image. ID or URL is incorrect."));
							});
						}
					);
				}
			}
		}

		// ================================================
		// GOOGLE PANO TILES
		// ================================================
		public IPromise<Texture2D> DownloadGooglePanoTile(string panoID, int x, int y) {
			var promise = new Promise<Texture2D>();
			DownloadGooglePanoTile(panoID, x, y,
				result => promise.Resolve(result),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads a 512x512 tile of the pano identified using the PanoID
		/// </summary>
		/// <param name="panoID">The Panorama ID from which the tiles have to be downloaded</param>
		/// <param name="x">The X index of the tile</param>
		/// <param name="y">The Y index of the tile</param>
		/// <param name="onResult">Callback with the tile texture</param>
		public void DownloadGooglePanoTile(string panoID, int x, int y, Action<Texture2D> onResult = null, Action<Exception> onException = null) {
			var url = "https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=" + panoID + "&output=tile&x=" + x + "&y=" + y + "&zoom=3&nbt&fover=2";

			try {
				using (WebClient client = new WebClient()) {
					client.DownloadDataCompleted += delegate (object sender, DownloadDataCompletedEventArgs e) {
						if (m_Disposed) return;
						Dispatcher.Instance.Enqueue(() => {
							if (e.Error == null) {
								var tile = new Texture2D(2, 2, TextureFormat.RGB24, false);
								tile.LoadImage(e.Result);
								InvokeCallback(onResult, tile);
							}
							else
								InvokeCallback(onException, e.Error);
						});
					};
					client.DownloadDataAsync(new Uri(url));
				}
			}
			catch (Exception e) {
				InvokeCallback(onException, e);
			}
		}

		// ================================================
		// OTHER METHODS
		// ================================================
		/// <summary>
		/// Extracts the Pano ID from a URL
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetIDFromURL(string url) {
			return url.Split('!')[4].Substring(2);
		}

		// Helper to invoke Texture2D Action
		void InvokeCallback<T>(Action<T> callback, T value = default(T)) {
			if (callback == null) return;

			if (value != null)
				callback(value);
			else
				callback(default(T));
		}

		// Copies the tile to the right place in the complete (large) texture
		void CopyTileToTexture(Texture2D tile, int x, int y) {
			Graphics.CopyTexture(tile, 0, 0, 0, 0, tile.width, tile.height, m_Texture, 0, 0, x * tile.width, y * tile.height);
			MonoBehaviour.Destroy(tile);
		}

		// Just checks the middle pixel in the bottom, if it is black, then the image (likely) requires
		// trimming. This will fail if the pixel happens to be pitch black in an image that
		// does not require trimming. However that in unlikely to happen and this hack works good enough.
		bool NeedsTrimming() {
			var p = m_Texture.GetPixel(2048, 1);
			return (p.r == 0 && p.g == 0 && p.b == 0);
		}

		// Removes the blank parts of the texture
		void TrimTexture() {
			// 3325x1664 are hardcoded. It has been found that any pano images that need to be trimmed
			// are of that resolution. We just block copy the texture contents into a new one and delete the 
			// untrimmed texture
			var trimmed = new Texture2D(3325, 1664, TextureFormat.RGB24, false);
			Graphics.CopyTexture(m_Texture, 0, 0, 0, 2048 - 1664, 3325, 1664, trimmed, 0, 0, 0, 0);
			MonoBehaviour.Destroy(m_Texture);
			m_Texture = trimmed;
		}

		bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			bool flag = true;
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int index = 0; index < chain.ChainStatus.Length; ++index) {
					if (chain.ChainStatus[index].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						if (!chain.Build((X509Certificate2)certificate))
							flag = false;
					}
				}
			}
			return flag;
		}

		public void Dispose() {
			m_Disposed = true;

			// Unsubscribe all listeners
			foreach (var listener in OnLoaded.GetInvocationList())
				OnLoaded -= (Action<Texture2D>)listener;

			if (m_Texture != null)
				MonoBehaviour.Destroy(m_Texture);
		}
	}
}
