using UnityEngine;
using System.Collections;

[System.Serializable]
public class Traverse : Object{

    // Global data
	public int TraverseNum;
    public Vector3 TraversePosition;
    public Quaternion TraverseOrientation;

    // Local gizmos informations
    public Vector3 forward;
    public Vector3 up;
    public Vector3 right;
    public Vector3 scale;

    /// <summary>
    /// Global static value of the lenght of the traverse
    /// </summary>
    public static float DistanceRail = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Traverse"/> class.
    /// </summary>
    /// <param name='position'>
    /// Position of the traverse
    /// </param>
    /// <param name='orientation'>
    /// Orientation of the traverse
    /// </param>
    public Traverse(Vector3 position, Quaternion orientation, int traverseNum){
        TraversePosition = position;
        TraverseOrientation = orientation;
		TraverseNum = traverseNum;
        // Private representation
        forward = Vector3.Normalize(TraverseOrientation * Vector3.forward);
        up = Vector3.Normalize(TraverseOrientation * Vector3.up);
        right = Vector3.Normalize(TraverseOrientation * Vector3.right);
        scale = new Vector3(DistanceRail * 1.2f, 0.05f, 0.1f);
    }
	
	#region Gizmos
    public void ToGizmos(Traverse lastTraverse, bool fullDisplay = true, TypeOfTroncon type = TypeOfTroncon.normal){
        if (fullDisplay){
            //DrawGizmos();
			DrawTroncon(lastTraverse,type);
            DrawTraverse();
            if (lastTraverse.TraversePosition != TraversePosition)
                DrawRail(lastTraverse);
        }
        else DrawRailSimple(lastTraverse);
    }
	private void DrawTroncon(Traverse lastTraverse,TypeOfTroncon type){
		switch(type){
		case TypeOfTroncon.frein:
			Gizmos.color = Color.red;
			Gizmos.DrawLine(lastTraverse.TraversePosition + lastTraverse.right * DistanceRail / 1.5f
				,TraversePosition + right * DistanceRail / 1.5f);
			Gizmos.DrawLine(lastTraverse.TraversePosition - lastTraverse.right * DistanceRail / 1.5f
				,TraversePosition - right * DistanceRail / 1.5f);
			break;
		case TypeOfTroncon.remonteMecanique:
        	Gizmos.color = Color.blue;
			Gizmos.DrawLine(lastTraverse.TraversePosition+lastTraverse.right*0.1f,TraversePosition+right*0.1f);
			Gizmos.DrawLine(lastTraverse.TraversePosition-lastTraverse.right*0.1f,TraversePosition-right*0.1f);
			break;
		}
	}
    private void DrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawLine(TraversePosition, TraversePosition + forward / 4);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(TraversePosition, TraversePosition + up / 4);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(TraversePosition, TraversePosition + right / 4);
    }
    private void DrawTraverse(){
        Gizmos.color = new Color(0.5f, 0.3f, 0f, 1);
        // TopFace
        Gizmos.DrawLine(TraversePosition + forward * scale.z + right * scale.x / 2f
                       , TraversePosition + forward * scale.z - right * scale.x / 2f);
        Gizmos.DrawLine(TraversePosition - forward * scale.z + right * scale.x / 2f
                       , TraversePosition - forward * scale.z - right * scale.x / 2f);
        Gizmos.DrawLine(TraversePosition + forward * scale.z - right * scale.x / 2f
                       , TraversePosition - forward * scale.z - right * scale.x / 2f);
        Gizmos.DrawLine(TraversePosition + forward * scale.z + right * scale.x / 2f
                       , TraversePosition - forward * scale.z + right * scale.x / 2f);
        // BottomFace
        Gizmos.DrawLine(TraversePosition + forward * scale.z + right * scale.x / 2f - up * scale.y
                       , TraversePosition + forward * scale.z - right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition - forward * scale.z + right * scale.x / 2f - up * scale.y
                       , TraversePosition - forward * scale.z - right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition + forward * scale.z - right * scale.x / 2f - up * scale.y
                       , TraversePosition - forward * scale.z - right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition + forward * scale.z + right * scale.x / 2f - up * scale.y
                       , TraversePosition - forward * scale.z + right * scale.x / 2f - up * scale.y);
        // LinesBetween
        Gizmos.DrawLine(TraversePosition + forward * scale.z + right * scale.x / 2f
                       , TraversePosition + forward * scale.z + right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition - forward * scale.z + right * scale.x / 2f
                       , TraversePosition - forward * scale.z + right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition + forward * scale.z - right * scale.x / 2f
                       , TraversePosition + forward * scale.z - right * scale.x / 2f - up * scale.y);
        Gizmos.DrawLine(TraversePosition - forward * scale.z - right * scale.x / 2f
                       , TraversePosition - forward * scale.z - right * scale.x / 2f - up * scale.y);
		/*
		// Support
		Vector3 rightSupport = TraversePosition + right * scale.x/2f - up*0.5f;
		Vector3 leftSupport  = TraversePosition - right * scale.x/2f - up*0.5f;
		Gizmos.DrawLine (TraversePosition + right * scale.x/2f, rightSupport);
		Gizmos.DrawLine (TraversePosition - right * scale.x/2f, leftSupport);
		if (rightSupport.y > 0) Gizmos.DrawLine (rightSupport,new Vector3(rightSupport.x,0,rightSupport.z));
		if (leftSupport.y > 0) Gizmos.DrawLine (leftSupport,new Vector3(leftSupport.x,0,leftSupport.z));*/
    }
    private void DrawRail(Traverse lastTraverse){
        Gizmos.color = Color.gray;
        // Right
        Gizmos.DrawLine(TraversePosition + right * (DistanceRail + 0.05f) / 2f
         , lastTraverse.TraversePosition + lastTraverse.right * (DistanceRail + 0.05f) / 2f);
        Gizmos.DrawLine(TraversePosition + right * (DistanceRail - 0.05f) / 2f
         , lastTraverse.TraversePosition + lastTraverse.right * (DistanceRail - 0.05f) / 2f);
        Gizmos.DrawLine(TraversePosition + right * (DistanceRail + 0.05f) / 2f + up * 0.05f
         , lastTraverse.TraversePosition + lastTraverse.right * (DistanceRail + 0.05f) / 2f + lastTraverse.up * 0.05f);
        Gizmos.DrawLine(TraversePosition + right * (DistanceRail - 0.05f) / 2f + up * 0.05f
         , lastTraverse.TraversePosition + lastTraverse.right * (DistanceRail - 0.05f) / 2f + lastTraverse.up * 0.05f);
        // Left
        Gizmos.DrawLine(TraversePosition - right * (DistanceRail + 0.05f) / 2f
         , lastTraverse.TraversePosition - lastTraverse.right * (DistanceRail + 0.05f) / 2f);
        Gizmos.DrawLine(TraversePosition - right * (DistanceRail - 0.05f) / 2f
         , lastTraverse.TraversePosition - lastTraverse.right * (DistanceRail - 0.05f) / 2f);
        Gizmos.DrawLine(TraversePosition - right * (DistanceRail + 0.05f) / 2f + up * 0.05f
         , lastTraverse.TraversePosition - lastTraverse.right * (DistanceRail + 0.05f) / 2f + lastTraverse.up * 0.05f);
        Gizmos.DrawLine(TraversePosition - right * (DistanceRail - 0.05f) / 2f + up * 0.05f
         , lastTraverse.TraversePosition - lastTraverse.right * (DistanceRail - 0.05f) / 2f + lastTraverse.up * 0.05f);
    }
    private void DrawRailSimple(Traverse lastTraverse)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(TraversePosition, TraversePosition + up / 4);
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(TraversePosition, lastTraverse.TraversePosition);
    }
	#endregion
	
}
