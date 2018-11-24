using System;
using System.Text;
using UnityEngine;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;

namespace Adrenak.UniMap {
	[Serializable]
	public class StreetViewDownloader : IDisposable {
		// ================================================
		// EVENTS
		// ================================================
		public delegate void FaceTextureDownloadedHandler(StreetView.Face face, Texture2D texture);
		public delegate void FaceTextureFailedHandler(StreetView.Face face, string error);

		/// <summary>
		/// Invoked when texture download for a <see cref="Face"/> is successful
		/// </summary>
		public event FaceTextureDownloadedHandler OnFaceTextureDownloaded;

		/// <summary>
		/// Invoked when texture download for a <see cref="Face"/> fails
		/// </summary>
		public event FaceTextureFailedHandler OnFaceTextureFailed;

		/// <summary>
		/// The base URL of the Street View API
		/// </summary>
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/streetview?";

		/// <summary>
		/// The <see cref="StreetViewDownloader.Options"/> used for the Street View API calls
		/// </summary>
		public StreetView.Options options = new StreetView.Options();

		/// <summary>
		/// A one to one map of <see cref="Face"/> and Texture2D corresponding to it
		/// </summary>
		Dictionary<StreetView.Face, Texture2D> textures = new Dictionary<StreetView.Face, Texture2D>();

		bool m_Disposed;

		public StreetViewDownloader() {
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(MyRemoteCertificateValidationCallback);
		}

		// ================================================
		// METHODS
		// ================================================
		/// <summary>
		/// Starts downloads of face textures
		/// </summary>
		public void Download() {
			DownloadFace(StreetView.Face.Up);
			DownloadFace(StreetView.Face.Down);
			DownloadFace(StreetView.Face.Front);
			DownloadFace(StreetView.Face.Back);
			DownloadFace(StreetView.Face.Left);
			DownloadFace(StreetView.Face.Right);
		}
		
		/// <summary>
		/// Downloads a single face texture
		/// </summary>
		/// <param name="face"></param>
		public void DownloadFace(StreetView.Face face) {
			using (WebClient client = new WebClient()) {
				client.DownloadDataCompleted += delegate (object sender, DownloadDataCompletedEventArgs e) {
					if (m_Disposed) return;
					Dispatcher.Instance.Enqueue(() => {
						if (e.Error == null) {
							var tex = new Texture2D(1, 1);
							tex.LoadImage(e.Result, false);
							tex.Apply();
							SetFaceTexture(face, tex);
							if (OnFaceTextureDownloaded != null) OnFaceTextureDownloaded(face, tex);
						}
						else {
							if (OnFaceTextureFailed != null) OnFaceTextureFailed(face, e.Error.ToString());
						}
					});
				};
				var url = GetURL(face);
				client.DownloadDataAsync(new Uri(url));
			}
		}
		
		/// <summary>
		/// Gets the request URL for the given face given the API parameters.
		/// </summary>
		/// <param name="face">The <see cref="Face"/> whose texture is to be downloaded.</param>
		/// <returns>The URL for downloading the texture of this <see cref="Face"/></returns>
		public string GetURL(StreetView.Face face) {
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

			sb.Append("&size=").Append(options.resolution).Append("x").Append(options.resolution)
			.Append("&fov=").Append(options.fov)
			.Append("&heading=").Append((options.heading % 360) + EnumUtility.HeadingFrom(face) + options.heading)
			.Append("&pitch=").Append(EnumUtility.PitchFrom(face) + options.pitch)
			.Append("&radius=").Append(options.radius)
			.Append("&source=").Append(EnumToString.From(options.source));

			return sb.ToString();
		}

		/// <summary>
		/// Gets the Texture2D corresponding to a <see cref="Face"/>
		/// </summary>
		/// <param name="face">The <see cref="Face"/> whose Texture2D is to be fetched.</param>
		/// <returns>The Texture2D corresponding to the passed <see cref="Face"/></returns>
		public Texture2D GetFaceTexture(StreetView.Face face) {
			if (textures.ContainsKey(face))
				return textures[face];
			else
				return null;
		}

		/// <summary>
		/// Sets a Texture2D to the given <see cref="Face"/>
		/// </summary>
		/// <param name="face">The <see cref="Face"/> whose texture is to be set.</param>
		/// <param name="texture">The texture to be set.</param>
		public void SetFaceTexture(StreetView.Face face, Texture2D texture) {
			if (textures.ContainsKey(face)) {
				MonoBehaviour.Destroy(textures[face]);
				textures[face] = texture;
			}
			else
				textures.Add(face, texture);
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
			foreach (var listener in OnFaceTextureFailed.GetInvocationList())
				OnFaceTextureFailed -= (FaceTextureFailedHandler)listener;

			foreach (var listener in OnFaceTextureDownloaded.GetInvocationList())
				OnFaceTextureDownloaded -= (FaceTextureDownloadedHandler)listener;

			foreach (var pair in textures)
				MonoBehaviour.Destroy(pair.Value);
		}
	}
}
