using System.Net;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Adrenak.UniMap {
	/// <summary>
	/// Used to download Street View images from URLs or Panorama IDs. This class is HEAVY as it uses a texture internally that is upto 4096x2048 pixels in size.
	/// </summary>
	public class PanoDownloader {
		/// <summary>
		/// Invoked everytime a request is finished. Null if the request was not successful
		/// </summary>
		public Action<Texture2D> OnLoaded;

		/// <summary>
		/// Returns the current Panorama Image texture
		/// </summary>
		public Texture2D PanoTexture {
			get { return m_Texture; }
		}
		
		Texture2D m_Texture;

		/// <summary>
		/// COnstructs a new downloader instance
		/// </summary>
		public PanoDownloader() {
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(MyRemoteCertificateValidationCallback);
		}

		/// <summary>
		/// Downloads a pano from a URL
		/// </summary>
		/// <param name="url">The URL of the pano that needs to be downloaded</param>
		/// <param name="callback">Callback with the texture in case of successful downoad. Else null.</param>
		public void DownloadUsingURL(string url, Action<Texture2D> callback = null) {
			try {
				var id = GetIDFromURL(url);
				DownloadUsingID(id, callback);
			}
			catch (Exception e) {
				Debug.LogError("Could not get Pano ID from the URL :" + e);
				if (callback != null) callback(null);
			}
		}

		/// <summary>
		/// Downloads a pano from the Pano ID
		/// </summary>
		/// <param name="panoID">The ID of the pano image that needs to be downloaded</param>
		/// <param name="callback">Callback with the texture in case of successful download. Else null.</param>
		public void DownloadUsingID(string panoID, Action<Texture2D> callback = null) {
			DownloadUserPanoWithID(panoID, 4096, pano => {
				if (pano != null) {
					InvokeCallback(callback, pano);
					InvokeCallback(OnLoaded, pano);
					return;
				}
				else {
					InvokeCallback(callback);
					InvokeCallback(OnLoaded);
				}

				// If the panorama cannot be downloaded from User, then try the Tile download approach

				if(m_Texture != null && m_Texture.width != 4096 && m_Texture.width != 2048)
					MonoBehaviour.Destroy(m_Texture);
				m_Texture = new Texture2D(4096, 2048, TextureFormat.RGB24, false);

				int count = 0;
				for (int i = 0; i < 8; ++i) {
					for (int j = 0; j < 4; ++j) {
						var x = i;
						var y = j;
						DownloadGooglePanoTileWithID(panoID, i, j, tile => {
							Dispatcher.Instance.Enqueue(() => {
								count++;
								if (tile != null)
									CopyTileToTexture(tile, x, 3 - y);
								if (count == 32) {
									TrimTexture();
									InvokeCallback(callback, m_Texture);
									InvokeCallback(OnLoaded, m_Texture);
								}
							});
						});
					}
				}
			});
		}

		/// <summary>
		/// Downloads the panorama image using the URL
		/// </summary>
		/// <param name="id">The PanoID to be downloaded</param>
		/// <param name="width">The width of the pano image</param>
		/// <param name="callback">Callback containing the Texture2D of the pano image</param>
		public void DownloadUserPanoWithURL(string url, int size = 4096, Action<Texture2D> callback = null) {
			try {
				var id = GetIDFromURL(url);
				DownloadUserPanoWithID(id, size, callback);
			}
			catch (Exception e) {
				Debug.LogError(e);
				InvokeCallback(callback);
				InvokeCallback(OnLoaded);
			}
		}

		/// <summary>
		/// Downloads the panorama image using the known PanoID
		/// </summary>
		/// <param name="id">The PanoID to be downloaded</param>
		/// <param name="width">The width of the pano image</param>
		/// <param name="callback">Callback containing the Texture2D of the pano image</param>
		public void DownloadUserPanoWithID(string id, int width = 4096, Action<Texture2D> callback = null) {
			string url = "https://lh5.googleusercontent.com/p/" + id + "=w" + width;
			try {
				using (WebClient client = new WebClient()) {
					client.DownloadDataCompleted += delegate (object sender, DownloadDataCompletedEventArgs e) {
						Dispatcher.Instance.Enqueue(() => {
							if (e.Error == null) {
								MonoBehaviour.Destroy(m_Texture);
								m_Texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
								m_Texture.LoadImage(e.Result);
								InvokeCallback(callback, m_Texture);
								InvokeCallback(OnLoaded, m_Texture);
							}
							else {
								Debug.LogError(e.Error);
								InvokeCallback(callback);
								InvokeCallback(OnLoaded);
							}
						});
					};
					client.DownloadDataAsync(new Uri(url));
				}
			}
			catch (Exception e) {
				Debug.LogError("Error downloading tile: " + e);
				InvokeCallback(callback, m_Texture);
				InvokeCallback(OnLoaded, m_Texture);
			}
		}

		/// <summary>
		/// Downloads a 512x512 tile of the pano identified using a URL
		/// </summary>
		/// <param name="panoID">The Panorama ID from which the tiles have to be downloaded</param>
		/// <param name="x">The X index of the tile</param>
		/// <param name="y">The Y index of the tile</param>
		/// <param name="callback">Callback with the tile texture</param>
		public void DownloadGooglePanoTileWithURL(string url, int x, int y, Action<Texture2D> callback = null) {
			try {
				var id = GetIDFromURL(url);
				DownloadGooglePanoTileWithID(id, x, y, callback);
			}
			catch (Exception e) {
				Debug.LogError(e);
				InvokeCallback(callback);
			}
		}

		/// <summary>
		/// Downloads a 512x512 tile of the pano identified using the PanoID
		/// </summary>
		/// <param name="panoID">The Panorama ID from which the tiles have to be downloaded</param>
		/// <param name="x">The X index of the tile</param>
		/// <param name="y">The Y index of the tile</param>
		/// <param name="callback">Callback with the tile texture</param>
		public void DownloadGooglePanoTileWithID(string panoID, int x, int y, Action<Texture2D> callback = null) {
			var url = "https://geo0.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=" + panoID + "&output=tile&x=" + x + "&y=" + y + "&zoom=3&nbt&fover=2";

			try {
				using (WebClient client = new WebClient()) {
					client.DownloadDataCompleted += delegate (object sender, DownloadDataCompletedEventArgs e) {
						Dispatcher.Instance.Enqueue(() => {
							if (e.Error == null) {
								var tile = new Texture2D(2, 2, TextureFormat.RGB24, false);
								tile.LoadImage(e.Result);
								InvokeCallback(callback, tile);
							}
							else {
								Debug.LogError(e.Error);
								InvokeCallback(callback);
							}
						});
					};
					client.DownloadDataAsync(new Uri(url));
				}
			}
			catch (Exception e) {
				Debug.LogError("Error downloading tile: " + e);
				InvokeCallback(callback);
			}
		}

		/// <summary>
		/// Extracts the Pano ID from a URL
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetIDFromURL(string url) {
			try {
				return url.Split('!')[4].Substring(2);
			}
			catch (Exception e) {
				Debug.LogError(e);
				return string.Empty;
			}
		}

		// Helper to invoke Texture2D Action
		void InvokeCallback(Action<Texture2D> callback, Texture2D tex = null) {
			if (callback == null) return;

			if (tex != null)
				callback(tex);
			else
				callback(null);
		}

		// Copies the tile to the right place in the complete (large) texture
		void CopyTileToTexture(Texture2D tile, int x, int y) {
			Graphics.CopyTexture(tile, 0, 0, 0, 0, tile.width, tile.height, m_Texture, 0, 0, x * tile.width, y * tile.height);
			MonoBehaviour.Destroy(tile);
		}

		// Removes the blank parts of the texture
		void TrimTexture() {
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
	}
}
