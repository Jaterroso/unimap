using System;
using UnityEngine;

namespace Adrenak.UniMap {
	public class StreetView {
		/// <summary>
		/// The 6 sides that the street view returns results for.
		/// </summary>
		public enum Face {
			Front,
			Right,
			Back,
			Left,
			Up,
			Down,
		}

		/// <summary>
		/// The source of the Street View search results
		/// </summary>
		public enum Source {
			Outdoor,
			Default
		}

		/// <summary>
		/// The mode of settings the destination for the street view. 
		/// <see cref="Mode.Coordinates"/> calls the API with a latitude and longitude value where the Street View should be serached
		/// <see cref="Mode.Location"/> calls the API with a text query where the text contains the name of the place
		/// <see cref="Mode.PanoID"/> calls the API for a specific Panorama ID.
		/// </summary>
		public enum Mode {
			Coordinates,
			Location,
			PanoID
		}

		[Serializable]
		public class Options {
			[Header("Main Settings")]
			/// <summary>
			/// The Google Maps API key. Get this on the Google developer portal
			/// </summary>
			public string key;

			/// <summary>
			/// The mode used to call the API. <see cref="Mode"/> for more information
			/// </summary>
			public Mode mode;

			/// <summary>
			/// The Location object that represents the coordinates where the Street View search should take place
			/// Used only if <see cref="mode"/> is set to <see cref="Mode.Coordinates"/>
			/// </summary>
			[Header("If Mode is set to Coordinates")]
			public Location location;

			/// <summary>
			/// The PanoID that should be requested by the API. 
			/// Used only if <see cref="mode"/> is set to <see cref="Mode.PanoID"/>
			/// </summary>
			[Header("If Mode is set to PanoID")]
			public string panoID = string.Empty;

			/// <summary>
			/// The place that should be queried for a StreetView
			/// Used only if <see cref="StreetViewDownloader.mode"/> is set to <see cref="Mode.Location"/>
			/// </summary>
			[Header("If Mode is set to Location")]
			public string place = string.Empty;

			[Header("Other Settings")]
			/// <summary>
			/// The field of view of the panoramic images fetch. Default is 90.
			/// </summary>
			public float fov = 90;

			/// <summary>
			/// The direction of the camera in degrees. Like a compass.
			/// </summary>
			public int heading = 0;

			/// <summary>
			/// (default:50) Sets a radius, specified in meters, in which to search for a panorama, 
			/// centered on the given latitude and longitude. Valid values are non-negative integers.
			/// </summary>
			public int radius = 50;

			/// <summary>
			/// (Default:0) Specifies the up or down angle of the camera relative to the Street View vehicle.
			/// </summary>
			public int pitch = 0;

			/// <summary>
			/// (Default:<see cref="Source.Default"/>) The <see cref="Source"/> type to be used for the API call
			/// </summary>
			public Source source = Source.Default;

			/// <summary>
			/// (default:512) The resolution of the face textures downloaded. GOOGLE API LIMITS THIS TO 640px. IF YOU WANT HIGH RES USE PANODOWNLOADER
			/// </summary>
			public int resolution = 512;
		}
	}
}