using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class PathPoint : MonoBehaviour {
	
	Vector3 lastPosition = Vector3.zero;
	Quaternion lastOrientation = Quaternion.identity;
	
	
	void OnDrawGizmosSelected(){
		if (lastPosition != transform.position ||
			lastOrientation != transform.rotation){
#if UNITY_EDITOR
            PathCreator.newGenerationRequest = true;
#endif
			lastPosition = transform.position;
			lastOrientation = transform.rotation;
		}
	}
	
}