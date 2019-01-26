using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Adrenak.UniMap {
	public class PanoSaver : MonoBehaviour {
		public InputField urlInput;
		public Text message;
		public PanoSize size;
		PanoDownloader downloader = new PanoDownloader();

		private void Start() {
			UniMapInitializer.Setup();
		}

		public void Save () {
			message.text = "downloading...";
			var id = PanoUtility.GetIDFromURL(urlInput.text);

			downloader.Download(id, size)
				.Then(texture => {
					try {
						var dir = Path.Combine(Application.dataPath.Replace("Assets", ""), "SavedPanos");
						Directory.CreateDirectory(dir);

						File.WriteAllBytes(Path.Combine(dir, id + ".png"), texture.EncodeToPNG());
						File.WriteAllBytes(Path.Combine(dir, id + ".jpg"), texture.EncodeToJPG());
						message.text = "Saved to " + dir;
					}
					catch(Exception e) {
						message.text = "Erorr saving the pano: " + e;
					}
				})
				.Catch(exception => {
					message.text = "Could not download that pano";
				});
		}

		private void OnApplicationQuit() {
			downloader.Stop();
		}
	}
}
