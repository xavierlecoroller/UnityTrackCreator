using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PathCreator))]
public class PathCreatorEditor : Editor {
	
	private bool _displayPathNodes;
	
	public override void OnInspectorGUI(){
		PathCreator pathCreator = (PathCreator)target;
		bool loop = GUILayout.Toggle(pathCreator.LoopCircuit," Loop circuit");
		if (loop != pathCreator.LoopCircuit){
			pathCreator.LoopCircuit = loop;
			PathCreator.newGenerationRequest = true;
		}
		GUILayout.Label("Distance circuit : " + pathCreator.GetLenght().ToString("F2"));
		_displayPathNodes = EditorGUILayout.Foldout(_displayPathNodes,"Display all nodes");
		if (pathCreator.Traverses != null && _displayPathNodes){
			foreach(KeyValuePair<Transform, List<Traverse>> pair in  pathCreator.Traverses){
				GUILayout.Label(pair.Key.ToString() + " - " + pair.Value.Count.ToString());
			}
		}
	}
	
}
