using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Adrenak.UniMap.Examples {
	public class PanoSaver : MonoBehaviour {
		public InputField urlInput;
		public Text message;
		public PanoSize size;
		public TextureFormat format = TextureFormat.RGB24;
		PanoDownloader downloader = new PanoDownloader();

		private void Start() {
			UniMapInitializer.Initialize();
		}

		async public void Save() {
			message.text = "downloading...";
			var id = PanoUtility.GetIDFromURL(urlInput.text);

            try{
                var texture = await downloader.Download(id, size, format);

                var dir = Path.Combine(Application.dataPath.Replace("Assets", ""), "SavedPanos");
                Directory.CreateDirectory(dir);

                File.WriteAllBytes(Path.Combine(dir, id + ".png"), texture.GetTexture2D(TextureFormat.ARGB32).EncodeToPNG());
                File.WriteAllBytes(Path.Combine(dir, id + ".jpg"), texture.GetTexture2D(TextureFormat.ARGB32).EncodeToJPG());
                message.text = "Saved to " + dir;
                
            }
            catch(Exception e){
                message.text = "Could not download that pano";
            }
		}

		private void OnApplicationQuit() {
			downloader.Stop();
		}
	}
}
