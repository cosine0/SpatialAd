using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

abstract class LocationProvider
{
    public abstract bool IsEnabledByUser();
    public abstract void Start(float accuracy, float updateDistance);
    public abstract LocationServiceStatus Status();
    public abstract float GetLatitude();
    public abstract float GetLongitude();
    public abstract float GetAltitude();
    public abstract float GetTrueHeading();
    public abstract float GetHorizontalAccuracy();
}

class UnityLocationProvider : LocationProvider
{
    public static UnityLocationProvider Instance = new UnityLocationProvider();

    public override bool IsEnabledByUser()
    {
        return Input.location.isEnabledByUser;
    }

    public override void Start(float accuracy, float updateDistance)
    {
        Input.location.Start(accuracy, updateDistance);
        Input.compass.enabled = true;
    }

    public override LocationServiceStatus Status()
    {
        return Input.location.status;
    }

    public override float GetLatitude()
    {
        return Input.location.lastData.latitude;
    }

    public override float GetLongitude()
    {
        return Input.location.lastData.longitude;
    }

    public override float GetAltitude()
    {
        return Input.location.lastData.altitude;
    }

    public override float GetTrueHeading()
    {
        return Input.compass.trueHeading;
    }

    public override float GetHorizontalAccuracy()
    {
        return Input.location.lastData.horizontalAccuracy;
    }
}

class LocationPoint
{
    public float Latitude;
    public float Longitude;
    public float Altitude;
    public float TrueHeading;
    public float HorizontalAccuracy;
}

class LerpReplayLocationProvider : LocationProvider
{
    private readonly SortedDictionary<float, LocationPoint> _locationPoints;
    private float _startTime;
    private ClientInfo _clientInfo;

    public LerpReplayLocationProvider(SortedDictionary<float, LocationPoint> points)
    {
        _clientInfo = GameObject.FindGameObjectWithTag("ClientInfo").GetComponent<ClientInfo>();
        _startTime = Time.time;
        _locationPoints = points;
    }

    public override bool IsEnabledByUser()
    {
        return true;
    }

    public override void Start(float accuracy, float updateDistance)
    {
        // nothing to do!
    }

    public override LocationServiceStatus Status()
    {
        return LocationServiceStatus.Running;
    }

    public override float GetLatitude()
    {
        var currentTime = Time.time - _startTime;
        var timeList = _locationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return _locationPoints[timeList.First()].Latitude;
        }
        else if (upperBound >= _locationPoints.Count)
        {
            return _locationPoints[timeList.Last()].Latitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            var midPoint = Mathf.Lerp(_locationPoints[lowerTime].Latitude, _locationPoints[upperTime].Latitude,
                (currentTime - lowerTime) / (upperTime - lowerTime));
            return midPoint;
        }
    }

    public override float GetLongitude()
    {
        var currentTime = Time.time - _startTime;
        var timeList = _locationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return _locationPoints[timeList.First()].Longitude;
        }
        else if (upperBound >= _locationPoints.Count)
        {
            return _locationPoints[timeList.Last()].Longitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            var ratio = (currentTime - lowerTime) / (upperTime - lowerTime);
            return Mathf.Lerp(_locationPoints[lowerTime].Longitude, _locationPoints[upperTime].Longitude,
                ratio);
        }
    }

    public override float GetAltitude()
    {
        var currentTime = Time.time - _startTime;
        var timeList = _locationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return _locationPoints[timeList.First()].Altitude;
        }
        else if (upperBound >= _locationPoints.Count)
        {
            return _locationPoints[timeList.Last()].Altitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            return Mathf.Lerp(_locationPoints[lowerTime].Altitude, _locationPoints[upperTime].Altitude,
                (currentTime - lowerTime) / (upperTime - lowerTime));
        }
    }

    public override float GetTrueHeading()
    {
        //return Input.compass.trueHeading;
        return _clientInfo.MainCamera.transform.eulerAngles.y + 14;
    }

    public override float GetHorizontalAccuracy()
    {
        return 1;
    }
}

class NearestReplayLocationProvider : LocationProvider
{

    protected SortedDictionary<float, LocationPoint> LocationPoints;
    protected float StartTime;

    public NearestReplayLocationProvider(SortedDictionary<float, LocationPoint> points)
    {
        StartTime = Time.time;
        LocationPoints = points;
    }

    protected NearestReplayLocationProvider()
    {
    }

    public override bool IsEnabledByUser()
    {
        return true;
    }

    public override void Start(float accuracy, float updateDistance)
    {
    }

    public override LocationServiceStatus Status()
    {
        return LocationServiceStatus.Running;
    }



    public override float GetLatitude()
    {
        var currentTime = Time.time - StartTime;
        var timeList = LocationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return LocationPoints[timeList.First()].Latitude;
        }
        else if (upperBound >= LocationPoints.Count)
        {
            return LocationPoints[timeList.Last()].Latitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            if (Mathf.Abs(currentTime - lowerTime) < Mathf.Abs(currentTime - upperTime))
                return LocationPoints[lowerTime].Latitude;
            else
                return LocationPoints[upperTime].Latitude;
        }
    }

    public override float GetLongitude()
    {
        var currentTime = Time.time - StartTime;
        var timeList = LocationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return LocationPoints[timeList.First()].Longitude;
        }
        else if (upperBound >= LocationPoints.Count)
        {
            return LocationPoints[timeList.Last()].Longitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            if (Mathf.Abs(currentTime - lowerTime) < Mathf.Abs(currentTime - upperTime))
                return LocationPoints[lowerTime].Longitude;
            else
                return LocationPoints[upperTime].Longitude;
        }
    }

    public override float GetAltitude()
    {
        var currentTime = Time.time - StartTime;
        var timeList = LocationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return LocationPoints[timeList.First()].Altitude;
        }
        else if (upperBound >= LocationPoints.Count)
        {
            return LocationPoints[timeList.Last()].Altitude;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            if (Mathf.Abs(currentTime - lowerTime) < Mathf.Abs(currentTime - upperTime))
                return LocationPoints[lowerTime].Altitude;
            else
                return LocationPoints[upperTime].Altitude;
        }
    }

    public override float GetTrueHeading()
    {
        var currentTime = Time.time - StartTime;
        var timeList = LocationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return LocationPoints[timeList.First()].TrueHeading;
        }
        else if (upperBound >= LocationPoints.Count)
        {
            return LocationPoints[timeList.Last()].TrueHeading;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];

            if (Mathf.Abs(currentTime - lowerTime) < Mathf.Abs(currentTime - upperTime))
                return LocationPoints[lowerTime].TrueHeading;
            else
                return LocationPoints[upperTime].TrueHeading;
        }
    }

    public override float GetHorizontalAccuracy()
    {

        var currentTime = Time.time - StartTime;
        var timeList = LocationPoints.Keys.ToList();
        var index = timeList.BinarySearch(currentTime);
        int lowerBound, upperBound;
        if (index >= 0)
        {
            if (timeList[0] == currentTime)
            {
                lowerBound = 0;
                upperBound = 1;
            }
            else
            {
                lowerBound = -1;
                upperBound = 0;
            }
        }
        else
        {
            lowerBound = ~index - 1;
            upperBound = lowerBound + 1;
        }

        if (upperBound == 0)
        {
            return LocationPoints[timeList.First()].HorizontalAccuracy;
        }
        else if (upperBound >= LocationPoints.Count)
        {
            return LocationPoints[timeList.Last()].HorizontalAccuracy;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];

            if (Mathf.Abs(currentTime - lowerTime) < Mathf.Abs(currentTime - upperTime))
                return LocationPoints[lowerTime].HorizontalAccuracy;
            else
                return LocationPoints[upperTime].HorizontalAccuracy;
        }
    }
}

[Serializable]
class Location
{
    public float Time;
    public float Latitude;
    public float Longitude;
    public float Altitude;
    public float TrueHeading;
    public float HorizontalAccuracy;
    public Vector3 Gyro;
}

[Serializable]
class LocationWrapper
{
    public List<Location> Locations;
}

class ServerNearestReplayLocationProvider : NearestReplayLocationProvider
{
    private bool _gettingDataFinihed;

    public ServerNearestReplayLocationProvider(int id)
    {
        _gettingDataFinihed = false;
        StaticCoroutine.DoCoroutine(GetLocationData(id));
    }

    private IEnumerator GetLocationData(int id)
    {

        WWWForm form = new WWWForm();
        form.AddField("id", id);

        // 3D 오브젝트 목록 가져오기: GPS 정보를 서버에 POST
        using (UnityWebRequest www = UnityWebRequest.Post(
            "http://ec2-13-125-7-2.ap-northeast-2.compute.amazonaws.com:31337/capstone/get_record_location.php", form))
        {
            // POST 전송
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                // TODO: 필요시 재시도
            }
            else
            {
                string responseJsonString = www.downloadHandler.text;
                LocationWrapper jsonLocations = JsonUtility.FromJson<LocationWrapper>(responseJsonString);

                LocationPoints = new SortedDictionary<float, LocationPoint>();
                foreach (var location in jsonLocations.Locations)
                {
                    LocationPoints.Add(location.Time, new LocationPoint
                    {
                        Altitude = location.Altitude,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        TrueHeading = location.TrueHeading,
                        HorizontalAccuracy = location.HorizontalAccuracy
                    });
                }
                StartTime = Time.time;
                _gettingDataFinihed = true;
            }
        }
    }

    public override LocationServiceStatus Status()
    {
        if (!_gettingDataFinihed)
            return LocationServiceStatus.Initializing;
        else
            return LocationServiceStatus.Running;
    }
}