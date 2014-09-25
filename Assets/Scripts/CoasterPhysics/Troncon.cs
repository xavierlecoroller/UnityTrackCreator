using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TypeOfTroncon { normal = 0, remonteMecanique, frein }

[System.Serializable]
public class Troncon : MonoBehaviour
{

    public TypeOfTroncon typeOfTroncon;
    [HideInInspector]
    public float distanceMin;
    [HideInInspector]
    public float distanceMax;
    private List<Track> _track;

    void Start() { }

    public void GetAllTracks(ref float distance, ref Vector3 lastPosition)
    {
        List<Transform> transforms = new List<Transform>(GetComponentsInChildren<Transform>());
        transforms.Sort(delegate(Transform a, Transform b) { return a.name.CompareTo(b.name); });
        transforms.Remove(transform);
        _track = new List<Track>();
        float Distance = distance;
        Vector3 positionLast = lastPosition;
        foreach (Transform trans in transforms)
        {
            _track.Add(new Track() { position = trans.position, rotation = trans.rotation });
            if (positionLast != Vector3.zero)
                Distance += Vector3.Distance(positionLast, trans.position);
            positionLast = trans.position;
        }
        lastPosition = positionLast;
        distanceMin = distance;
        distanceMax = Distance;
        distance = distanceMax;
    }
    public void GetTrack(Track lastTrack, float distance, ref Vector3 position, ref Quaternion rotation)
    {
        float Distance = distanceMin;
        Track lastPosition = lastTrack;
        for (int i = 0; i < _track.Count; i++)
        {
            Distance += Vector3.Distance(lastPosition.position, _track[i].position);
            if (distance < Distance)
            {
                position = Vector3.Lerp(_track[i].position, lastPosition.position, (Distance - distance)
                                        / Vector3.Distance(lastPosition.position, _track[i].position));
                rotation = Quaternion.Lerp(_track[i].rotation, lastPosition.rotation, (Distance - distance)
                                           / Vector3.Distance(lastPosition.position, _track[i].position));
                return;
            }
            lastPosition = _track[i];
        }
    }
    public Track GetLastTrack
    {
        get { return _track[_track.Count - 1]; }
    }
    public void GetHeightMinMax(out float maxHeight, out float minHeight)
    {
        maxHeight = float.NegativeInfinity;
        minHeight = float.PositiveInfinity;
        foreach (Track track in _track)
        {
            if (maxHeight < track.position.y) maxHeight = track.position.y;
            if (minHeight > track.position.y) minHeight = track.position.y;
        }
    }

}


