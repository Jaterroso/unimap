namespace Adrenak.UniMap {
	public class EnumUtility {
		public static int HeadingFrom(StreetView.Face face) {
			switch (face) {
				case StreetView.Face.Right: return 90;
				case StreetView.Face.Back: return 180;
				case StreetView.Face.Left: return 270;
				default: return 0;
			}
		}

		public static int PitchFrom(StreetView.Face face) {
			switch (face) {
				case StreetView.Face.Up: return 90;
				case StreetView.Face.Down: return -90;
				default: return 0;
			}
		}
	}
}
