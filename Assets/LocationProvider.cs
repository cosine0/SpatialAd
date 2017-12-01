using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

abstract class LocationProvider
{
    public abstract bool IsEnabledByUser();
    public abstract void Start(float accuracy, float updateDistance);
    public abstract LocationServiceStatus Status();
    public abstract float GetLatitude();
    public abstract float GetLongitude();
    public abstract float GetAltitude();
    public abstract float GetTrueHeading();
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
}

class LocationPoint
{
    public float Latitude;
    public float Longitude;
    public float Altitude;
    public float TrueHeading;
}

class LerpReplayLocationProvider : LocationProvider
{
    private readonly SortedDictionary<float, LocationPoint> _locationPoints;
    private float _startTime;

    public LerpReplayLocationProvider(SortedDictionary<float, LocationPoint> points)
    {
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
            return Mathf.Lerp(_locationPoints[lowerTime].Latitude, _locationPoints[upperTime].Latitude,
                (currentTime - lowerTime) / (upperTime - lowerTime));
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
            return _locationPoints[timeList.First()].TrueHeading;
        }
        else if (upperBound >= _locationPoints.Count)
        {
            return _locationPoints[timeList.Last()].TrueHeading;
        }
        else
        {
            var lowerTime = timeList[lowerBound];
            var upperTime = timeList[upperBound];
            var lowerHeading = _locationPoints[lowerTime].TrueHeading;
            var upperHeading = _locationPoints[upperTime].TrueHeading + 360f;
            // lowerHeading +/-180 값 안으로 들어오도록 조정
            if (upperHeading > lowerHeading + 180)
                upperHeading -= 360;
            else if (upperHeading < lowerHeading - 180)
                upperHeading += 360;
            return (Mathf.Lerp(lowerHeading, upperHeading, (currentTime - lowerTime) / (upperTime - lowerTime)) % 360f + 360f) % 360f;
        }
    }
}