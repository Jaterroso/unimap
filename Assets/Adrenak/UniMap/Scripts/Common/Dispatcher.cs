using System;
using Adrenak.Unex;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.UniMap {
	public class Dispatcher : MonoBehaviour {
		static Dispatcher m_Instance;
		static Queue<Action> m_Queue = new Queue<Action>();

		public static void Create() {
			if (m_Instance != null) return;
			var go = new GameObject();
			go.hideFlags = HideFlags.DontSave;
			DontDestroyOnLoad(go);
			m_Instance = go.AddComponent<Dispatcher>();
		}

		public static void Add(Action action) {
			lock (m_Queue) {
				m_Queue.Enqueue(action);
			}
		}

		public static IPromise Add() {
			var promise = new Promise();
			Add(() => promise.Resolve());
			return promise;
		}

		private void Update() {
			lock (m_Queue) {
				while (m_Queue.Count > 0)
					m_Queue.Dequeue()();
			}
		}
	}
}
