using System;
using System.Collections.Generic;
using Adrenak.UniMap;

namespace Adrenak.UniMap {
	public class EnumToString {
		public static string From(PlaceType type) {
			List<char> letters = new List<char>();
			var name = type.ToString();

			for (int i = 0; i < name.Length; i++) {
				char letter = name[i];

				if (char.IsUpper(letter)) {
					if (i != 0)
						letters.Add('_');
					letters.Add(Char.ToLower(letter));
				}
				else
					letters.Add(letter);
			}

			return new string(letters.ToArray());
		}

		public static string From(StreetView.Source source) {
			switch (source) {
				case StreetView.Source.Default: return "default";
				case StreetView.Source.Outdoor: return "outdoor";
				default: return "default";
			}
		}
	}
}