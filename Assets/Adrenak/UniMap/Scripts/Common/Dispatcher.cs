using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.UniMap {
	public class Dispatcher : MonoBehaviour {
		static Dispatcher m_Instance;
		public static Dispatcher Instance {
			get {
				if (m_Instance == null)
					m_Instance = GameObject.FindObjectOfType<Dispatcher>();
				if(m_Instance == null) {
					var go = new GameObject();
					go.hideFlags = HideFlags.DontSave;
					DontDestroyOnLoad(go);
					m_Instance = go.AddComponent<Dispatcher>();
				}
				return m_Instance;
			}				
		}

		Queue<Action> m_Queue = new Queue<Action>();

		public void Init() { }

		public void Enqueue(Action action) {
			lock (m_Queue) {
				m_Queue.Enqueue(action);
			}
		}

		private void Update() {
			lock (m_Queue) {
				while (m_Queue.Count > 0)
					m_Queue.Dequeue()();
			}
		}
	}
}
