using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackController : MonoBehaviour
{

    public bool IsLoop = true;
    public float DistanceTotal;

    private List<Troncon> _troncons;
    private float _minHeight = float.PositiveInfinity;
    private float _maxHeight = float.NegativeInfinity;

    public void Start()
    {
        _troncons = new List<Troncon>(GetComponentsInChildren<Troncon>());
        _troncons.Sort(delegate(Troncon a, Troncon b) { return a.name.CompareTo(b.name); });
        DistanceTotal = 0;
        Vector3 lastPosition = Vector3.zero;

        foreach (Troncon troncon in _troncons)
        {
            troncon.GetAllTracks(ref DistanceTotal, ref lastPosition);
            float maxHeight;
            float minHeight;
            troncon.GetHeightMinMax(out maxHeight, out minHeight);
            if (maxHeight > _maxHeight) _maxHeight = maxHeight;
            if (minHeight < _minHeight) _minHeight = minHeight;
        }
    }

    public TypeOfTroncon GetTrack(float distance, ref Vector3 requestPos, ref Quaternion requestRot)
    {
        // Keep the distance between 0 and the distance total of the loop
        while (distance < 0) distance += DistanceTotal;
        while (distance > DistanceTotal) distance -= DistanceTotal;
        for (int i = 0; i < _troncons.Count; i++)
        {
            // Test every troncon to get were the distance request is
            if (distance < _troncons[i].distanceMax && distance > _troncons[i].distanceMin)
            {
                Track lastTrack;
                if (i == 0) lastTrack = _troncons[_troncons.Count - 1].GetLastTrack;
                else lastTrack = _troncons[i - 1].GetLastTrack;
                _troncons[i].GetTrack(lastTrack, distance, ref requestPos, ref requestRot);
                return _troncons[i].typeOfTroncon;
            }
        }
        return TypeOfTroncon.normal;
    }

    public float GetMaxHeight { get { return _maxHeight; } }
    public float GetMinHeight { get { return _minHeight; } }

}