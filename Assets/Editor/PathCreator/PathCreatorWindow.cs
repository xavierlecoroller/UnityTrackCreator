using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PathCreatorWindow : EditorWindow {
	
	// Editor gui data
	private enum PathCreatorState {TrackPathTools, TrackGenerator, SupportTools}
	private PathCreatorState _state = PathCreatorState.TrackPathTools;
    private Vector2 _scrollPosition = Vector2.zero;

	// Track path data
	private PathCreator _trackPathCreator = null;
	private bool _generateTraverse = true;
	private bool _generateRail = true;
    private bool _generateSupport = true;
    private float _offsetSupport = 0;
	
	[MenuItem("SuperMineCart/PathCreatorTools")]
    static void ShowWindow(){
        EditorWindow.GetWindow(typeof(PathCreatorWindow));
	}
	
	private void OnGUI(){
		try{
			if (_trackPathCreator == null){
				_trackPathCreator = GameObject.FindObjectOfType(typeof(PathCreator)) as PathCreator;
				if (_trackPathCreator == null){
					// if the pathcreator object does not exist, create it
					GameObject gameObject = new GameObject("PathCreator");
					_trackPathCreator = gameObject.AddComponent<PathCreator>();
					GameObject pathFirstPoint = new GameObject("00000");
					pathFirstPoint.AddComponent<PathPoint>();
					pathFirstPoint.transform.parent = gameObject.transform;
				}
			}
			// Menu
            GuiButtonMenu(new Rect(5, 3, Screen.width / 3 - 10, 20), PathCreatorState.TrackPathTools);
            GuiButtonMenu(new Rect(5+Screen.width/3,3,Screen.width/3-10,20), PathCreatorState.SupportTools);
			GuiButtonMenu(new Rect(5 + 2 * Screen.width / 3, 3, Screen.width / 3 - 10, 20),PathCreatorState.TrackGenerator);

			// Data
			switch(_state){
			case PathCreatorState.TrackPathTools:
				// Modification first path
				GUI.Label(new Rect(5,30,Screen.width-10,20),"Path point modification :",EditorStyles.whiteLargeLabel);
				if (GUI.Button(new Rect(5,60,2*Screen.width/4-10,20),"Add Last PathPoint"))
					_trackPathCreator.AddPathPoint();
				if (GUI.Button(new Rect(5,80,2*Screen.width/4-10,20),"Sub Last PathPoint"))
					_trackPathCreator.SubPathPoint();
				if (GUI.Button(new Rect(5+3*Screen.width/4,60,Screen.width/4-10,40),"Refresh Path"))
					_trackPathCreator.GenerationPath();
				string titleLoopCircuit = "Loop circuit";
				if (_trackPathCreator.LoopCircuit) titleLoopCircuit = "No loop circuit";
				if (GUI.Button(new Rect(5+2*Screen.width/4,60,Screen.width/4-10,40),titleLoopCircuit))
					_trackPathCreator.LoopCircuit = !_trackPathCreator.LoopCircuit;
				_trackPathCreator.GetLastPathPoint().transform.position = 
					EditorGUI.Vector3Field(new Rect(5,105,Screen.width-10,35),"Last PathPoint Position",
					_trackPathCreator.GetLastPathPoint().transform.position);
				float angle = _trackPathCreator.GetLastPathPoint().transform.eulerAngles.z;
				angle = EditorGUI.FloatField(new Rect(5,140,Screen.width-10,18),"Last PathPoint Angle Turn",angle);
				if (angle > 180) angle -= 360;
				_trackPathCreator.GetLastPathPoint().transform.eulerAngles = new Vector3(0,0,angle);
				// Modification tronconConfig
				TronconConfig confToDelete = null;
				int farTroncon = 0;
				GUI.Label(new Rect(5,170,Screen.width-10,20),"Section modification :",EditorStyles.whiteLargeLabel);
				for(int i = 0; i < _trackPathCreator.TronconConfigs.Count ; i++){
					TronconConfig conf = _trackPathCreator.TronconConfigs[i];
					GUI.Box(new Rect(5,200+i*50,Screen.width-10,40),"");
					GUI.Label(new Rect(10,210+i*50,20,20),i.ToString());
					conf.TypeOfTheTroncon = (TypeOfTroncon)EditorGUI.EnumPopup(new Rect(27,200+i*50,Screen.width-54,16),conf.TypeOfTheTroncon);
					if (GUI.Button(new Rect(Screen.width-27,200+i*50,20,16),"X")) confToDelete = conf;
					EditorGUI.LabelField(new Rect(27,220+i*50,(Screen.width-34)/4,16),"First number");
					conf.BeginNum = EditorGUI.IntField(new Rect(27+(Screen.width-34)/4,220+i*50,(Screen.width-34)/4,16),conf.BeginNum);
					EditorGUI.LabelField(new Rect(27+2*(Screen.width-34)/4,220+i*50,(Screen.width-34)/4,16),"Last number");
					conf.EndNum = EditorGUI.IntField(new Rect(27+3*(Screen.width-34)/4,220+i*50,(Screen.width-34)/4,16),conf.EndNum);
					if (conf.EndNum <= conf.BeginNum) conf.EndNum = conf.BeginNum+1;
					if (farTroncon < conf.EndNum) farTroncon = conf.EndNum;
				}
				if (confToDelete!= null) _trackPathCreator.TronconConfigs.Remove(confToDelete);
				if (GUI.Button(new Rect(5,200+_trackPathCreator.TronconConfigs.Count*50,Screen.width-10,20),"Add new Troncon")){
					_trackPathCreator.TronconConfigs.Add(new TronconConfig(TypeOfTroncon.remonteMecanique,farTroncon,farTroncon+1));
				}
				break;
            case PathCreatorState.SupportTools:
                int nbSupport = _trackPathCreator.supportConfigs.Count;
                GUI.Label(new Rect(5, 30, Screen.width - 10, 20), "List of Support :", EditorStyles.whiteLargeLabel);

                GUI.Label(new Rect(5, 60, 40, 20), "Num");
                GUI.Label(new Rect(45, 60, Screen.width - 50, 20), "Type of support");
                _scrollPosition = GUI.BeginScrollView(new Rect(0, 80, Screen.width, Screen.height - 110)
                    , _scrollPosition, new Rect(0, 0, Screen.width - 15, 20 * nbSupport));
                for (int i = 0; i < nbSupport - 1; i++)
                {
                    SupportConfig supp = _trackPathCreator.supportConfigs[i];
                    GUI.Box(new Rect(5, 5 + i * 20, Screen.width - 25, 20), "");
                    GUI.Label(new Rect(5, 5 + i * 20, 40, 20), _trackPathCreator.supportConfigs[i].NumTraverse.ToString());
                    if (_trackPathCreator.supportSelected != _trackPathCreator.supportConfigs[i].PositionOnTraverse){
                        if (GUI.Button(new Rect(45 + Screen.width - 165, 7 + i * 20, 100, 15), "Show Support"))
                            _trackPathCreator.supportSelected = _trackPathCreator.supportConfigs[i].PositionOnTraverse;
                    }
                    else if (GUI.Button(new Rect(45 + Screen.width - 165, 7 + i * 20, 100, 15), "HIDE")) _trackPathCreator.supportSelected = Vector3.zero;
                    supp.SupportType = (TypeOfSupport)EditorGUI.EnumPopup(new Rect(45, 7 + i * 20, Screen.width - 165, 20), supp.SupportType);
                }
                GUI.EndScrollView();
                break;
			case PathCreatorState.TrackGenerator:
				GUI.Label(new Rect(5,30,Screen.width-10,20),"Traverse base :",EditorStyles.whiteLargeLabel);
				_trackPathCreator.TraverseObject = EditorGUI.ObjectField(new Rect(5,50,Screen.width-10,15),_trackPathCreator.TraverseObject,typeof(GameObject),false) as GameObject;
				GUI.Label(new Rect(5,80,Screen.width-10,20),"Rail base :",EditorStyles.whiteLargeLabel);
				_trackPathCreator.RailObject = EditorGUI.ObjectField(new Rect(5,100,Screen.width-10,15),_trackPathCreator.RailObject,typeof(GameObject),false) as GameObject;
				GUI.Label(new Rect(5,130,Screen.width-10,20),"Support base :",EditorStyles.whiteLargeLabel);
                _trackPathCreator.SupportObject = EditorGUI.ObjectField(new Rect(5, 150, Screen.width - 10, 15), _trackPathCreator.SupportObject, typeof(GameObject), false) as GameObject;
                GUI.Box(new Rect(5,190,Screen.width-10,120),"");
				GUI.Label(new Rect(7,190,Screen.width-14,20),"Generation option :",EditorStyles.whiteLargeLabel);
				_generateTraverse = GUI.Toggle(new Rect(7,210,Screen.width-14,20),_generateTraverse,"Generate traverse ?");
                _generateRail = GUI.Toggle(new Rect(7, 230, Screen.width - 14, 20), _generateRail, "Generate rail ?");
                _generateSupport = GUI.Toggle(new Rect(7, 250, Screen.width/2 - 14, 20), _generateSupport, "Generate support ?");
                _offsetSupport = EditorGUI.FloatField(new Rect(7 + Screen.width / 2, 250, Screen.width / 2 - 14, 20), "Offset support ?", _offsetSupport);
				if (   (_trackPathCreator.TraverseObject!=null  || !_generateTraverse)
                    && (_trackPathCreator.RailObject != null    || !_generateRail)
                    && (_trackPathCreator.SupportObject != null || !_generateSupport))
                {
					if (GUI.Button(new Rect(7,270,Screen.width-14,38),"Start Generation")){
                        _trackPathCreator.GenerateTroncon(_generateTraverse, _generateRail, _generateSupport, _offsetSupport);
					}
				}
				else GUI.Box(new Rect(7,270,Screen.width-14,38),"Start Generation");
				break;
			default:
				_state = PathCreatorState.TrackPathTools;
				break;
			}
		}catch(System.Exception ex){
			GUILayout.Label("Error : \n" + ex.ToString());
		}
	}

	#region GUIFunction

	private void GuiButtonMenu(Rect pos, PathCreatorState state){
		if (_state == state) GUI.Box(pos, state.ToString());
		else if (GUI.Button(pos, state.ToString())) _state = state;
	}

	#endregion

}
