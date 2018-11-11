using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Adrenak.UniMap {
	public class PanoSaver : MonoBehaviour {
		public InputField urlInput;
		public Text message;
		PanoDownloader downloader = new PanoDownloader();

		private void Start() {
			Dispatcher.Instance.Init();
		}

		public void Save () {
			downloader.DownloadUsingURL(urlInput.text, texture => {
				if(texture == null) {
					message.text = "Could not download that pano";
				}
				else {
					try {
						var id = PanoDownloader.GetIDFromURL(urlInput.text);

						var dir = Path.Combine(Application.dataPath.Replace("Assets", ""), "SavedPanos");
						Directory.CreateDirectory(dir);

						File.WriteAllBytes(Path.Combine(dir, id + ".png"), texture.EncodeToPNG());
						File.WriteAllBytes(Path.Combine(dir, id + ".jpg"), texture.EncodeToJPG());
						message.text = "Saved to " + dir;
					}
					catch(Exception e) {
						message.text = "Erorr saving the pano: " + e;
					}
				}
			});
		}
	}
}
