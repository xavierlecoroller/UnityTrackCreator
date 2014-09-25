using UnityEngine;
using System.Collections;

public class FollowTrain : MonoBehaviour {

    public GameObject TrainPosition;

    void Start ()
    {
    }

    void Update ()
    {
        transform.position = TrainPosition.transform.position;
        transform.rotation = Quaternion.Euler(0f,TrainPosition.transform.eulerAngles.y,0f);
    }
}
