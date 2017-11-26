using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

static class StaticGoogleMap
{
    /// <summary>
    /// 스태틱 구글맵을 다운받아 UI.RawImage에 텍스처로 입혀줌
    /// </summary>
    /// <param name="image">UI.RawImage 오브젝트</param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="zoom">지도 줌. 작을수록 확대됨.</param>
    /// <param name="mapType"></param>
    /// <param name="scale"></param>
    public static void ApplyMapTexture(RawImage image, float latitude, float longitude, float zoom = 18, string mapType = "roadmap", int scale = 0)
    {
        StaticCoroutine.DoCoroutine(MapTextureCoroutine(image, latitude, longitude, zoom, mapType, scale));
    }

    private const string ApiKey = "AIzaSyDWD_HQYenCZyrcVs9v0qhf2sZWSqMneHE";

    private static IEnumerator MapTextureCoroutine(RawImage image, float latitude, float longitude, float zoom = 18, string mapType = "roadmap", int mapWidth = 700, int mapHeight = 640, int scale = 0)
    {
        var url = string.Format("https://maps.googleapis.com/maps/api/staticmap?" +
                            "center={0},{1}" +
                            "&zoom={2}" +
                            "&size={3:d}x{4:d}" +
                            "&scale={5}" +
                            "&maptype={6}" +
                            "&markers=color:blue%7C{0},{1}" +
                            "&key={7}", latitude, longitude, zoom, (int)(image.rectTransform.rect.width * 2 / 3), (int)(image.rectTransform.rect.height * 2 / 3), scale, mapType, ApiKey);
        Debug.Log(url);
        WWW www = new WWW(url);
        yield return www;
        image.texture = www.texture;
    }
}