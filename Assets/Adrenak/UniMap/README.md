## UniMap
Unity3D Google Maps SDK

UniMap is a C# SDK for using Google Maps APIs. The current features are:
- [Place Search](https://developers.google.com/places/web-service/search)
    - `Find Place`
    - `Nearby Search`
    - `Text Search`
- [Street View](https://developers.google.com/maps/documentation/streetview/intro)
- [Street View Meta Data](https://developers.google.com/maps/documentation/streetview/metadata)
- `Panorama` image download _(uses URL, not API)_

## Plug and Play features
UniMap has prefabs and scripts that provide high level implementation of the SDK classes. These include:
- `StreetViewRenderer` which uses `StreetViewDownloader` internally to show different street view images. See the `Street View Example` for a sample.
- `PanoRenderer` uses `PanoDownloader` internally to show different panorama images from a street view URL or panoID. See the `Pano Example` for a sample.
- `PanoSaver` tool lets you save Street View from the browser URL to your drive. These can then be imported as Cubemap and used as a skybox in Unity.

## Advanced Usage
UniMap exposes the Google APIs using the following low level classes:
- `FindPlaceRequest` 
- `NearbySearchRequest`
- `StreetViewMetaRequest`
- `TextSearchRequest`
- `PanoDownloader`
- `StreetViewDownloader`

The classes are `asynchronous` and can provide results in two ways:
- `Callbacks`
- `Promises`

The `Promises` library used is [Real Serious Games' C# Promise](https://github.com/Real-Serious-Games/C-Sharp-Promise) The library is used in a DLL form and can be (de)activated using the `UNIMAP_RSG_PROMISES` scripting define symbol.

## Contact
Vatsal Ambastha:  
[@github](https://www.github.com/adrenak)  
[@www](http://www.vatsalambastha.com)