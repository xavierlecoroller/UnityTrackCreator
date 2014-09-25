using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waggon : MonoBehaviour
{

    private WaggonController waggonController;
    public float positionFromController = 0;

    private Vector3 _requestPos = Vector3.zero;
    private Quaternion _requestRot = Quaternion.identity;
    private Vector3 _oldPos = Vector3.zero;
    private Quaternion _oldRot = Quaternion.identity;

    void Start() { }

    public void UpdatePosition(float lerp)
    {
        transform.position = Vector3.Lerp(_oldPos, _requestPos, lerp);
        transform.rotation = Quaternion.Lerp(_oldRot, _requestRot, lerp);
    }

    public void UpdatePhysics(float position)
    {
        waggonController.Tracks.GetTrack(position + positionFromController, ref _requestPos, ref _requestRot);
        _oldPos = transform.position;
        _oldRot = transform.rotation;
    }

    public void SetWaggonController(WaggonController controller)
    {
        waggonController = controller;
        _oldPos = transform.position;
        _oldRot = transform.rotation;
    }
}
