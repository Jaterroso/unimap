using UnityEngine;

namespace Adrenak.UniMap {
	public class CoroutineRunner : MonoBehaviour {
		static CoroutineRunner m_Instance;
		public static CoroutineRunner Instance {
			get {
				if (m_Instance == null)
					m_Instance = GameObject.FindObjectOfType<CoroutineRunner>();
				if (m_Instance == null) {
					var go = new GameObject("CoroutineRunner");
					go.hideFlags = HideFlags.HideAndDontSave;
					DontDestroyOnLoad(go);
					return go.AddComponent<CoroutineRunner>();
				}
				return m_Instance;
			}
		}
	}
}
