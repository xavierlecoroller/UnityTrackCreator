using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum TypeOfSupport { none, simple, huge, turn, upsidedown }

public class SupportConfig{

    private int             _numTraverse;
    private Vector3         _fromPosition;
    private Vector3         _dirTraverseUp;
    private Vector3         _dirTraverseForward;
    private Vector3         _dirTraverseLeft;
    private TypeOfSupport   _typeOfSupport;

    public SupportConfig(int numTraverse, Vector3 fromPosition,
        Vector3 dirTraverseUp, Vector3 dirTraverseForward,
        TypeOfSupport typeOfSupport = TypeOfSupport.simple)
    {
        _numTraverse = numTraverse;
        _fromPosition = fromPosition;
        _dirTraverseUp = dirTraverseUp;
        _dirTraverseForward = dirTraverseForward;
        _dirTraverseLeft = Vector3.Cross(_dirTraverseUp, _dirTraverseForward);
        _typeOfSupport = typeOfSupport;
    }

    
    public void ToGizmos(){
        if (_typeOfSupport == TypeOfSupport.none) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_fromPosition, 0.2f);
        Vector3 toPosition;
        RaycastHit raycastHit;
        switch (_typeOfSupport){
            case TypeOfSupport.simple:
                if (Physics.Raycast(_fromPosition, -Vector3.up, out raycastHit)) toPosition = raycastHit.point;
                else toPosition = new Vector3(_fromPosition.x, 0, _fromPosition.z);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(_fromPosition, toPosition);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(toPosition, 0.1f);
                break;
            case TypeOfSupport.turn:
                if (Physics.Raycast(_fromPosition, -_dirTraverseUp, out raycastHit)) toPosition = raycastHit.point;
                else toPosition = new Vector3(_fromPosition.x, 0, _fromPosition.z);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(_fromPosition, toPosition);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(toPosition, 0.1f);
                break;
            case TypeOfSupport.huge:
                // Algo
                Vector3 fromPositionLeft = _fromPosition - _dirTraverseLeft - Vector3.up;
                Vector3 fromPositionRight = _fromPosition + _dirTraverseLeft - Vector3.up;
                Vector3 toPositionLeft, toPositionRight;
                if (Physics.Raycast(fromPositionLeft, -Vector3.up, out raycastHit)) toPositionLeft = raycastHit.point;
                else toPositionLeft = new Vector3(fromPositionLeft.x, 0, fromPositionLeft.z);
                if (Physics.Raycast(fromPositionRight, -Vector3.up, out raycastHit)) toPositionRight = raycastHit.point;
                else toPositionRight = new Vector3(fromPositionRight.x, 0, fromPositionRight.z);

                // Display
                Gizmos.color = Color.black;
                Gizmos.DrawLine(_fromPosition, fromPositionLeft);
                Gizmos.DrawLine(_fromPosition, fromPositionRight);
                if (fromPositionLeft.y > fromPositionRight.y)
                    Gizmos.DrawLine(fromPositionLeft, new Vector3(0, fromPositionRight.y - fromPositionLeft.y, 0) + fromPositionLeft);
                else Gizmos.DrawLine(fromPositionRight, new Vector3(0, fromPositionLeft.y - fromPositionRight.y, 0) + fromPositionRight);
                Gizmos.DrawLine(fromPositionLeft, toPositionLeft);
                Gizmos.DrawLine(fromPositionRight, toPositionRight);
                /*float distance = 0;
                bool pair = true;
                while (distance+2 < Vector3.Distance(fromPositionLeft, toPositionLeft)
                    && distance+2 < Vector3.Distance(fromPositionRight, toPositionRight)){
                    if (pair) Gizmos.DrawLine(fromPositionLeft - Vector3.up * distance, fromPositionRight - Vector3.up * (distance + 2));
                    else Gizmos.DrawLine(fromPositionLeft - Vector3.up * (distance + 2), fromPositionRight - Vector3.up * distance);
                    Gizmos.DrawLine(fromPositionLeft - Vector3.up * distance, fromPositionRight - Vector3.up * distance);
                    pair = !pair;
                    distance += 2;
                }*/
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(toPositionLeft, 0.1f);
                Gizmos.DrawWireSphere(toPositionRight, 0.1f);
                break;
            case TypeOfSupport.upsidedown:
                Vector3 intermediatePosition = _fromPosition - _dirTraverseUp;
                if (Physics.Raycast(intermediatePosition, -Vector3.up, out raycastHit)) toPosition = raycastHit.point;
                else toPosition = new Vector3(_fromPosition.x, 0, _fromPosition.z);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(_fromPosition, intermediatePosition);
                Gizmos.DrawLine(intermediatePosition, toPosition);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(toPosition, 0.1f);
                break;
        }
    }

    public TypeOfSupport SupportType{
        get { return _typeOfSupport; }
        set { _typeOfSupport = value; ; }
    }

    public int NumTraverse { get { return _numTraverse; } }
    public Vector3 PositionOnTraverse { get { return _fromPosition; } }
    public List<Track> GetPositionForExtrude(float offset){
        List<Track> supportExtrudePath = new List<Track>();
        RaycastHit raycastHit;
        Vector3 fromPosition = _fromPosition -_dirTraverseUp * offset;
        switch (_typeOfSupport){
            case TypeOfSupport.simple:
                supportExtrudePath.Add(new Track() { position = fromPosition, rotation = Quaternion.identity });
                if (Physics.Raycast(fromPosition, -Vector3.up, out raycastHit)) supportExtrudePath.Add(new Track() { position = raycastHit.point, rotation = Quaternion.identity });
                else supportExtrudePath.Add(new Track() { position = new Vector3(fromPosition.x, 0, fromPosition.z), rotation = Quaternion.identity });
                break;
            case TypeOfSupport.turn:
                if (Physics.Raycast(fromPosition, -_dirTraverseUp, out raycastHit)){
                    supportExtrudePath.Add(new Track() { position = fromPosition, rotation = Quaternion.FromToRotation(Vector3.up, fromPosition - raycastHit.point) });
                    supportExtrudePath.Add(new Track() { position = raycastHit.point, rotation = Quaternion.FromToRotation(Vector3.up, fromPosition - raycastHit.point) });
                }
                else{
                    supportExtrudePath.Add(new Track() { position = fromPosition, rotation = Quaternion.identity });
                    supportExtrudePath.Add(new Track() { position = new Vector3(fromPosition.x, 0, fromPosition.z), rotation = Quaternion.identity });
                }
                break;
            case TypeOfSupport.huge:
                // TODO
                Debug.LogError("TODO");
                // TODO
                Vector3 fromPositionLeft = fromPosition - _dirTraverseLeft - Vector3.up;
                Vector3 fromPositionRight = fromPosition + _dirTraverseLeft - Vector3.up;
                Vector3 toPositionLeft, toPositionRight;
                if (Physics.Raycast(fromPositionLeft, -Vector3.up, out raycastHit)) toPositionLeft = raycastHit.point;
                else toPositionLeft = new Vector3(fromPositionLeft.x, 0, fromPositionLeft.z);
                if (Physics.Raycast(fromPositionRight, -Vector3.up, out raycastHit)) toPositionRight = raycastHit.point;
                else toPositionRight = new Vector3(fromPositionRight.x, 0, fromPositionRight.z);
                supportExtrudePath.Add(new Track() { 
                    position = toPositionLeft,
                    rotation = Quaternion.FromToRotation(Vector3.up, -Vector3.up)
                    * Quaternion.Euler(0, -Vector3.Angle(Vector3.forward, new Vector3(_dirTraverseForward.x, 0, _dirTraverseForward.z)), 0)
                });
                supportExtrudePath.Add(new Track() { position = fromPositionLeft, rotation = supportExtrudePath[supportExtrudePath.Count - 1].rotation });
                supportExtrudePath.Add(new Track() { position = fromPosition, rotation = supportExtrudePath[supportExtrudePath.Count - 1].rotation * Quaternion.Euler(-90, 0, 0) });
                supportExtrudePath.Add(new Track() { position = fromPositionRight, rotation = supportExtrudePath[supportExtrudePath.Count - 1].rotation * Quaternion.Euler(-90, 0, 0) });
                if (fromPositionRight.y > 0) supportExtrudePath.Add(new Track() { position = toPositionRight, rotation = supportExtrudePath[supportExtrudePath.Count - 1].rotation });
                break;
            case TypeOfSupport.upsidedown:
                supportExtrudePath.Add(new Track() { position = fromPosition, rotation = Quaternion.FromToRotation(Vector3.up, _dirTraverseUp) });
                supportExtrudePath.Add(new Track() { position = fromPosition - _dirTraverseUp*0.7f, rotation = Quaternion.FromToRotation(Vector3.up, _dirTraverseUp) });
                if (Physics.Raycast(fromPosition - _dirTraverseUp, -Vector3.up, out raycastHit)){
                    supportExtrudePath.Add(new Track() { position = fromPosition - _dirTraverseUp - 0.3f * Vector3.up, rotation = Quaternion.identity });
                    supportExtrudePath.Add(new Track() { position = raycastHit.point, rotation = Quaternion.identity });
                }
                else{
                    supportExtrudePath.Add(new Track() { position = fromPosition, rotation = Quaternion.identity });
                    supportExtrudePath.Add(new Track() { position = new Vector3(fromPosition.x, 0, fromPosition.z), rotation = Quaternion.identity });
                }
                break;
        }
        // On ajoute un dernier extrude pour redresser l'ensemble et lui donner sa bonne orientation
        Track lastTrack = supportExtrudePath[supportExtrudePath.Count - 1];
        supportExtrudePath.Add(new Track() { position = lastTrack.position - Vector3.up, rotation = Quaternion.identity });
        return supportExtrudePath;
    }

}

