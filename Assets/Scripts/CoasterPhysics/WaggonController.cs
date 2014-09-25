using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaggonController : MonoBehaviour
{


    /* Track & Physics data */
    public TrackController Tracks;
    private bool m_InitTrack = false;

    // Physics part
    public double Masse = 1;
    public double CoeffFrot = 0.03f;

    // Acceleration/Velocity/Position Equation
    private double _acceleration = 0;
    private double _velocity = 0;
    private double _lastVelocity = 10 / 3.6;
    private double _position = 0;
    private double _lastPosition = 0;

    private TypeOfTroncon _actualTypeOfTroncon = TypeOfTroncon.normal;

    private double _velocityRemonterMecanique = 10 / 3.6;
    private float _timeUpdate = 0;

    public float distanceOnTrack = 0;

    // Display part
    private Vector3 _requestPos;
    private Quaternion _requestRot;
    private Vector3 _oldPos;
    private Quaternion _oldRot;


    /* Waggon */
    private List<Waggon> _waggons;

    private IEnumerator Start()
    {
        yield return null;
        _requestPos = transform.position;
        _requestRot = transform.rotation;
        _oldPos = transform.position;
        _oldRot = transform.rotation;
        _waggons = new List<Waggon>(transform.parent.GetComponentsInChildren<Waggon>());
        foreach (Waggon waggon in _waggons) waggon.SetWaggonController(this);
        m_InitTrack = true;
    }

    private void Update()
    {
        if (!m_InitTrack)
        {
            return;
        }
        distanceOnTrack = (Time.time - _timeUpdate) / Time.fixedDeltaTime;
        // Update smooth position
        transform.position = Vector3.Lerp(_oldPos, _requestPos, distanceOnTrack);
        transform.rotation = Quaternion.Lerp(_oldRot, _requestRot, distanceOnTrack);
        foreach (Waggon waggon in _waggons) waggon.UpdatePosition(distanceOnTrack);
    }

    private void FixedUpdate()
    {
        if (!m_InitTrack)
        {
            return;
        }
        if (Tracks != null)
        {
            // Physics Engine
            _acceleration = Vector3.Dot(Physics.gravity, transform.forward) - _lastVelocity * CoeffFrot;
            switch (_actualTypeOfTroncon)
            {
                case TypeOfTroncon.remonteMecanique:
                    // La remonté mécanique contre la force de pesanteur et permet 
                    // de maintenir une vitesse constante.
                    if (_velocity <= _velocityRemonterMecanique) _acceleration += -_acceleration;
                    break;
                case TypeOfTroncon.frein:
                    // Le frein va augementer les forces de frottements jusqu'à ce 
                    // que le véhicule atteigne une certaine vitesse
                    if (_velocity > _velocityRemonterMecanique) _acceleration -= _lastVelocity * 1f;
                    break;
            }
            _velocity = _acceleration * Time.fixedDeltaTime + _lastVelocity;
            _position = _lastPosition + _lastVelocity * Time.fixedDeltaTime + _acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime / 2f;
            // Set the position
            SetNewPosition((float)_position);
            // Update the last data
            _lastVelocity = _velocity;
            _lastPosition = _position;
            _timeUpdate = Time.time;
        }
    }

    // Tools
    public float GetUpdateTime
    {
        get { return _timeUpdate; }
    }

    private void SetNewPosition(float distance)
    {
        Profiler.BeginSample("SetPositionTrack");
        _actualTypeOfTroncon = Tracks.GetTrack(distance, ref _requestPos, ref _requestRot);
        _oldPos = transform.position;
        _oldRot = transform.rotation;
        foreach (Waggon waggon in _waggons) waggon.UpdatePhysics(distance);
        Profiler.EndSample();
    }

    public void Init(double initVelocity = 10/3.6, double initPosition = 0)
    {
        _acceleration = 0;
        _velocity = 0;
        _lastVelocity = initVelocity;
        _position = 0f;
        _lastPosition = initPosition;
        _velocityRemonterMecanique = 10 / 3.6f;
    }
}
