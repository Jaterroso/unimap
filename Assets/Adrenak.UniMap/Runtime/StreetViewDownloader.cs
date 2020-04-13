using System;
using System.Text;
using UnityEngine;
using Adrenak.Unex;
using System.Net;
using System.Collections.Generic;
using RestSharp;

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
			var url = GetURL(face);
            var client = new RestClient();
            var request = new RestRequest(url);
            client.ExecuteAsync(request, response => { 
                Dispatcher.Enqueue(() => {
                    if (m_Disposed) return;
                    if (response.IsSuccessful()) {
                        var tex = new Texture2D(1, 1, TextureFormat.RGB565, false);
                        tex.LoadImage(response.RawBytes, false);
                        tex.Apply();
                        SetFaceTexture(face, tex);
                        OnFaceTextureDownloaded?.Invoke(face, tex);
                    }
                    else {
                        OnFaceTextureFailed?.Invoke(face, response.GetException().ToString());
                    }
                });            
            });
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
