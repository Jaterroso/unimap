using System;
using System.Collections.Generic;

namespace Adrenak.UniMap {
	[Serializable]
	public class Location {
		public double lat;
		public double lng;

		public Location(double _lat, double _lng) {
			lat = _lat;
			lng = _lng;
		}

		public static Location operator +(Location lhs, Location rhs) {
			return new Location(
				rhs.lat + lhs.lat,
				rhs.lng + lhs.lng
			);
		}

		public override string ToString() {
			return "Location (" + lat + ", " + lng + ")";
		}
	}

	[Serializable]
	public class Northeast {
		public double lat;
		public double lng;
	}

	[Serializable]
	public class Southwest {
		public double lat;
		public double lng;
	}

	[Serializable]
	public class Viewport {
		public Northeast northeast;
		public Southwest southwest;
	}

	[Serializable]
	public class Geometry {
		public Location location;
		public Viewport viewport;
	}

	[Serializable]
	public class OpeningHours {
		public bool open_now;
	}

	[Serializable]
	public class Photo {
		public int height;
		public List<string> html_attributions;
		public string photo_reference;
		public int width;
	}

	[Serializable]
	public class PlusCode {
		public string compound_code;
		public string global_code;
	}

	[Serializable]
	public class Nearby {
		public Geometry geometry;
		public string icon;
		public string id;
		public string name;
		public OpeningHours opening_hours;
		public List<Photo> photos;
		public string place_id;
		public PlusCode plus_code;
		public int price_level;
		public double rating;
		public string reference;
		public string scope;
		public List<string> types;
		public string vicinity;
	}

	[Serializable]
	public class Place {
		public string formatted_address;
		public Geometry geometry;
		public string name;
		public OpeningHours opening_hours;
		public List<Photo> photos;
		public double rating;
	}

	[Serializable]
	public class DebugLog {
		public List<object> line;
	}

	[Serializable]
	public class TextSearchResult {
		public string formatted_address;
		public Geometry geometry;
		public string icon;
		public string id;
		public string name;
		public OpeningHours opening_hours;
		public List<Photo> photos;
		public string place_id;
		public PlusCode plus_code;
		public double rating;
		public string reference;
		public List<string> types;
	}

	[Serializable]
	public class NearbySearchResponse {
		public List<object> html_attributions;
		public List<Nearby> results;
		public string status;
	}

	[Serializable]
	public class TextSearchResponse {
		public List<object> html_attributions;
		public List<TextSearchResult> results;
		public string status;
	}

	[Serializable]
	public class FindPlaceResponse {
		public List<Place> candidates;
		public DebugLog debug_log;
		public string status;
	}

	[Serializable]
	public class StreetViewMetaResponse {
		public string copyright;
		public string date;
		public Location location;
		public string pano_id;
		public string status;
	}

	[Serializable]
	public class AddressComponent {
		public string long_name;
		public string short_name;
		public List<string> types;
	}

	[Serializable]
	public class GeocodingResponse {
		public List<GeocodingResult> results;
		public string status;
	}

	[Serializable]
	public class GeocodingResult {
		public List<AddressComponent> address_components;
		public string formatted_address;
		public Geometry geometry;
		public string place_id;
		public PlusCode plus_code;
		public List<string> types;
	}

	[Serializable]
	public class GeolookUpResult {
		public List<AddressComponent> address_components;
		public string formatted_address;
		public Geometry geometry;
		public string place_id;
		public List<string> types;
	}

	[Serializable]
	public class GeolookupResponse {
		public List<GeolookUpResult> results;
	}
}