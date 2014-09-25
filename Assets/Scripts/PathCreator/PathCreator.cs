using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PathCreator : MonoBehaviour{
	

	// Gizmos attribut
    public bool Affichage = true;
    public bool AffichageLeger = false;
	
    /// Generation path
	public int NumberIteration = 10;
	public bool LoopCircuit = true;
    public static bool newGenerationRequest = true;
    public static float distanceEntreTraverses = 1f;
	public static Transform SelectedTransform;
    public Dictionary<Transform, List<Traverse>> Traverses; // Toute les traverses générer
	public Traverse LastTraverse;

    /// Generations objects
	// Troncon attribut
	public List<TronconConfig> TronconConfigs = new List<TronconConfig>();
    public GameObject TraverseObject;
    public GameObject RailObject;
    public GameObject SupportObject;
    // Support attribut
    public List<SupportConfig> supportConfigs;
    public Vector3 supportSelected = Vector3.zero;



	// Unity Function
	private void Start(){}
	private void OnDrawGizmos(){
		if (Affichage){
			if (newGenerationRequest) GenerationPath();
			if (Traverses != null && Traverses.Count > 0){
				Traverse lastTraverse = LastTraverse;
				foreach (KeyValuePair<Transform, List<Traverse>> pair in Traverses){
					foreach (Traverse travs in pair.Value){
						bool notInTroncon = true;
						foreach(TronconConfig conf in TronconConfigs){
							if (notInTroncon && travs.TraverseNum >= conf.BeginNum && travs.TraverseNum < conf.EndNum){
								travs.ToGizmos(lastTraverse, !AffichageLeger, conf.TypeOfTheTroncon);
								notInTroncon = false;
							}
						}
						if (notInTroncon) travs.ToGizmos(lastTraverse, !AffichageLeger);
						lastTraverse = travs;
					}
				}
                foreach (SupportConfig supp in supportConfigs) {
                    supp.ToGizmos();
                }
                if (supportSelected != Vector3.zero) {
                    Gizmos.DrawSphere(supportSelected, 1);
                }
			}
		}
	}
	
	// EditorWindow Function
	public float GetLenght(){
		if (Traverses == null || Traverses.Count <2) return 0;
		Traverse lastTraverseLoop = LastTraverse;
		float distance = 0;
		foreach (KeyValuePair<Transform, List<Traverse>> pair in Traverses){
            foreach (Traverse traverse in pair.Value){
				distance += Vector3.Distance(lastTraverseLoop.TraversePosition,traverse.TraversePosition);
                lastTraverseLoop = traverse;
            }
        }
		return distance;
	}
	public void AddPathPoint(){
		int lastName = 0;
		GameObject lastPoint = GetLastPathPoint();
		if (int.TryParse(lastPoint.transform.name,out lastName)){
			GameObject pathPoint = new GameObject(string.Format("{0:00000}",lastName+1),typeof(PathPoint));
			pathPoint.transform.position = lastPoint.transform.position;
			pathPoint.transform.eulerAngles = lastPoint.transform.eulerAngles;
			pathPoint.transform.parent = lastPoint.transform.parent;
		}
		else Debug.Log("Error, the pattern use for the name of the point " +
			"does not enable you to use the PathCreator tools. [Pattern expected \"000001\"]");
			
	}
	public void SubPathPoint(){
		GameObject.DestroyImmediate(GetLastPathPoint());
		GenerationPath();
	}
	public GameObject GetLastPathPoint(){
		List<Component> components = new List<Component>(transform.GetComponentsInChildren(typeof(Transform)));
		if (components.Count < 1) return null;
		List<Transform> transforms = components.ConvertAll(c => (Transform)c);
		components = null;
		transforms.Remove(transform);
		transforms.Sort(delegate(Transform a, Transform b) { return a.name.CompareTo(b.name); });
		return transforms[transforms.Count-1].gameObject;
	}
	
	// Generation Function
    public void GenerationPath(){
		// Initialisation de l'ancien Path
		Traverses = new Dictionary<Transform, List<Traverse>>();
        supportConfigs = new List<SupportConfig>();

        /// Récupération des transforms des enfants
        List<Component> components = new List<Component>(transform.GetComponentsInChildren(typeof(Transform)));
		if (components.Count < 2) return;
        List<Transform> pathTransform = components.ConvertAll(c => (Transform)c);
		components = null;
        pathTransform.Remove(transform); // Remove the parent object
		// On trie en fonction de leur nom les transform
        pathTransform.Sort(delegate(Transform a, Transform b) { return a.name.CompareTo(b.name); });
		if (pathTransform.Count < 2) return;
		// On rajoute le premier point pour permettre au circuit de faire une boucle si demander
		if (LoopCircuit) pathTransform.Add(pathTransform[0]); 
		
        /// Génération du path smoother
		// Génération du path
        IEnumerable<Vector3> pathNodes = Interpolate.NewCatmullRom(pathTransform.ToArray(), NumberIteration, LoopCircuit);
        List<Vector3> pathVector = new List<Vector3>();
		Vector3 vectorStart = Vector3.zero;
		// On convertie le path smoother en une entité plus facilement controlable
        foreach (Vector3 vect in pathNodes) {
			pathVector.Add(vect);
			// La variable pathnode peut etre une boucle infini, 
			// on s'assure donc que si on repasse par le meme point
			// on sort de la boucle.
			if (vectorStart == Vector3.zero) vectorStart = vect;
			else if (vect == vectorStart) break;
		}
        pathNodes = null;

		int numberTraverse = 0;
        // Si il y a plus de 2 points
        if (pathVector.Count >= 2){
            /// Génération de l'orientation
            List<Quaternion> orientation = new List<Quaternion>();
            for (int i = 0; i < pathTransform.Count - 1; i++){
                Traverses.Add(pathTransform[i], new List<Traverse>());
                Quaternion lastAngle = pathTransform[i].rotation;
                orientation.Add(lastAngle);
                for (int j = 0; j < NumberIteration; j++)
                    orientation.Add(Slerp(lastAngle, pathTransform[i + 1].rotation, ((float)j) / NumberIteration));
            }
            orientation.Add(pathTransform[0].rotation);
			
            /// Génération du chemin smoother
            int tempIterator = 0;
			// On rajoute la premiere traverse qui nous servira de base pour l'algorythme
            Traverses[pathTransform[0]].Add(new Traverse(pathVector[0],
                Quaternion.LookRotation(pathVector[1] - pathVector[0]),numberTraverse));
			// On garde toujours la dernière position en mémoire
            Vector3 LastTraversesPos = pathVector[0];
			
			// L'objectif de cette boucle est de générer les positions des traverses 
			// à des distances régulières tout en  associant chaque traverse avec 
			// son objet d'origine.
            for (int i = 1; i < pathVector.Count-2; i++){
				// Le path vector est composé du path transform avec des points 
				// interpoler pour pouvoir smoother les courbes. Chaque point situer a partir
				// du transform jusqu'au transform suivant sont inclue dans le transform de départ.
                if (pathTransform[tempIterator + 1].position == pathVector[i]) tempIterator++;
				// Nous permet de savoir si la distance entre les points smoother et la dernière traverse
                float distance = Vector3.Distance(LastTraversesPos, pathVector[i]);
				// Si cette distance est supèrieur a la distance entre traverse, alors 
				// on rajoute une traverse a cette endroit.
                if (distance >= distanceEntreTraverses){
					// On recupère la distance entre les deux derniers path Vector
                    float lenght = Vector3.Distance(pathVector[i - 1], pathVector[i]);
					// On calcule alors la position de la traverse
                    Vector3 NewTraversesPos = Vector3.Lerp(pathVector[i - 1], pathVector[i], (distanceEntreTraverses - (distance - lenght)) / lenght);
					// Et ensuite son orientation
                    Quaternion NewTraversesOri = Slerp(orientation[i - 1], orientation[i], (distanceEntreTraverses - (distance - lenght)) / lenght);
					// On augemente alors le numero de traverse
					numberTraverse++;
					// On créer la nouvelle traverse
                    Traverses[pathTransform[tempIterator]].Add(new Traverse(NewTraversesPos,
                            Quaternion.LookRotation(NewTraversesPos - LastTraversesPos)
                            * Quaternion.Euler(new Vector3(0, 0, -NewTraversesOri.eulerAngles.z)),numberTraverse));
					// Au cas ou la distance entre les deux derniers path vector est 
					// suffisante pour d'autre traverses, on re-test le dernier path vector
					// avec les données de la nouvelle traverse
                    i--;
                    LastTraversesPos = NewTraversesPos;
                }
            }
			if (LoopCircuit && pathTransform.Count > 3) LastTraverse = Traverses[pathTransform[tempIterator]][Traverses[pathTransform[tempIterator]].Count-1];
			else LastTraverse= Traverses[pathTransform[0]][0];
            newGenerationRequest = false;
        }
		else{
			Traverses.Add(pathTransform[0], new List<Traverse>());
			Traverses[pathTransform[0]].Add(new Traverse(pathTransform[0].position,pathTransform[0].rotation,numberTraverse+1));
			LastTraverse = Traverses[pathTransform[0]][0];
		}
        // Generation des supports
        // on ne fait pas le dernier point
        for (int i = 0; i < pathTransform.Count - 1; i++) {
            if (Traverses[pathTransform[i]].Count > 0){
                Vector3 posSupport = Traverses[pathTransform[i]][0].TraversePosition;
                Vector3 dirUp = Traverses[pathTransform[i]][0].up;
                Vector3 dirForward = Traverses[pathTransform[i]][0].forward;
                supportConfigs.Add(new SupportConfig(Traverses[pathTransform[i]][0].TraverseNum,posSupport,dirUp,dirForward));
            }
        }

    }
	public void GenerateTroncon(bool traverse, bool rail, bool support, float offsetSupport = 0){
        try{
            float generationProgress = 0;
            EditorUtility.DisplayProgressBar("Generation process : TrackRoot Creator", "Please wait until the end of the progress", generationProgress);
            // Controle if the object TrackRoot already exist
            if (GameObject.Find("TrackRoot") != null){
                EditorUtility.ClearProgressBar();
                return;
            }
            // Génération de la base
            GameObject trackRoot = new GameObject("TrackRoot");
            trackRoot.AddComponent<TrackController>();
            // Rail Datas
            GameObject baseRail = Instantiate(RailObject) as GameObject;
            baseRail.name = "_RailObject";
            baseRail.GetComponent<MeshFilter>().mesh = new Mesh();
            baseRail.transform.parent = trackRoot.transform;
            List<ExtrudedTrailSection> sectionsRail = new List<ExtrudedTrailSection>();
            // Support Object
            GameObject baseSupport = new GameObject("_SupportObject");
            baseSupport.transform.parent = trackRoot.transform;
            // Traverses Datas
            int nbTraverses = Traverses.Count;
            int nbTroncon = 0;
            GameObject actualTroncon = trackRoot;
            bool isConfFromStart = false;
            bool isFirstTrack = true;
            Transform firstTrack = transform;
            foreach (TronconConfig conf in TronconConfigs){
                if (conf.BeginNum == 0) isConfFromStart = true;
            }
            if (!isConfFromStart){
                actualTroncon = new GameObject("section_" + string.Format("{0:00000}", (nbTroncon)));
                actualTroncon.transform.parent = trackRoot.transform;
                actualTroncon.AddComponent<Troncon>().typeOfTroncon = TypeOfTroncon.normal;
            }
            foreach (KeyValuePair<Transform, List<Traverse>> pair in Traverses)
            {
                foreach (Traverse travs in pair.Value){
                    foreach (TronconConfig conf in TronconConfigs){
                        if (conf.BeginNum == travs.TraverseNum){
                            nbTroncon++;
                            actualTroncon = new GameObject("section_" + string.Format("{0:00000}", (nbTroncon)));
                            actualTroncon.transform.parent = trackRoot.transform;
                            actualTroncon.AddComponent<Troncon>().typeOfTroncon = conf.TypeOfTheTroncon;
                        }
                        if (conf.EndNum == travs.TraverseNum){
                            nbTroncon++;
                            actualTroncon = new GameObject("section_" + string.Format("{0:00000}", (nbTroncon)));
                            actualTroncon.transform.parent = trackRoot.transform;
                            actualTroncon.AddComponent<Troncon>().typeOfTroncon = TypeOfTroncon.normal;
                        }
                    }

                    //// Traverses Creation ////
                    GameObject go;
                    if (traverse) go = GameObject.Instantiate(TraverseObject, travs.TraversePosition, travs.TraverseOrientation) as GameObject;
                    else{
                        go = new GameObject();
                        go.transform.position = travs.TraversePosition;
                        go.transform.rotation = travs.TraverseOrientation;
                    }
                    go.transform.name = "track_" + string.Format("{0:00000}", (travs.TraverseNum));
                    go.transform.parent = actualTroncon.transform;
                    if (isFirstTrack){
                        firstTrack = go.transform;
                        isFirstTrack = false;
                    }

                    //// Rail Creation ////
                    if (rail) Extrude(go.transform, baseRail, RailObject, sectionsRail);

                    //// Support Creation /////
                    if (support)
                    {
                        SupportConfig configOfTraverse = supportConfigs.Find(delegate(SupportConfig conf) { return conf.NumTraverse == travs.TraverseNum; });
                        if (configOfTraverse != null && configOfTraverse.SupportType != TypeOfSupport.none){
                            List<ExtrudedTrailSection> sectionsSupport = new List<ExtrudedTrailSection>();
                            List<Track> supportExtrudePath = configOfTraverse.GetPositionForExtrude(offsetSupport);
                            GameObject supportTroncon = Instantiate(SupportObject) as GameObject;
                            supportTroncon.name = "support_" + string.Format("{0:00000}", (travs.TraverseNum));
                            supportTroncon.GetComponent<MeshFilter>().mesh = new Mesh();
                            supportTroncon.transform.parent = baseSupport.transform;
                            supportTroncon.transform.position = supportExtrudePath[supportExtrudePath.Count - 1].position;
                            foreach (Track track in supportExtrudePath) Extrude(track.position, track.rotation, supportTroncon, SupportObject, sectionsSupport);
                        }
                    }
                }
                generationProgress += (1 / (float)nbTraverses);
                EditorUtility.DisplayProgressBar("Generation process ", "Please wait until the end of the progress", generationProgress);
            }
            // Fake track that enable soft loop
            GameObject firstLoopTrack = new GameObject("track_99999");
            firstLoopTrack.transform.parent = actualTroncon.transform;
            firstLoopTrack.transform.position = firstTrack.position;
            firstLoopTrack.transform.rotation = firstTrack.rotation;
            if (rail){
                Extrude(firstLoopTrack.transform, baseRail, RailObject, sectionsRail);
                baseRail.transform.position = firstTrack.position;
                baseRail.transform.rotation = firstTrack.rotation;
            }
            else GameObject.DestroyImmediate(baseRail);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
		EditorUtility.ClearProgressBar ();
	}

	// tools
    private void Extrude(Vector3 pos, Quaternion rot, GameObject rail, GameObject baseObject, List<ExtrudedTrailSection> sections){
        GameObject temp = new GameObject("__temp_Extrude__");
        temp.transform.position = pos;
        temp.transform.rotation = rot;
        Extrude(temp.transform, rail, baseObject, sections);
        GameObject.DestroyImmediate(temp);
    }
    private void Extrude(Transform trans, GameObject rail, GameObject baseObject, List<ExtrudedTrailSection> sections){
		Vector3 position = trans.position;
		float now = Time.time;
		// Recover Mesh
        Mesh baseMesh = baseObject.GetComponent<MeshFilter>().sharedMesh;
		MeshExtrusion.Edge[] precomputedEdges = MeshExtrusion.BuildManifoldEdges(baseMesh);
		// Add a new trail section to beginning of array
		/*if (sections.Count == 0 || (sections[0].point - position).sqrMagnitude >
		    PathCreator.distanceEntreTraverses * PathCreator.distanceEntreTraverses){*/
			ExtrudedTrailSection section = new ExtrudedTrailSection();
			section.point = position;
			section.matrix = trans.localToWorldMatrix;
			section.time = now;
			sections.Insert(0, section);
		//}
        // We need at least 2 sections to create the line
		if (sections.Count < 2) return;
		Matrix4x4 worldToLocal = trans.worldToLocalMatrix;
		Matrix4x4[] finalSections = new Matrix4x4[sections.Count];
		for (int i = 0; i < sections.Count; i++){
			if (i == 0) finalSections[i] = Matrix4x4.identity;
			else finalSections[i] = worldToLocal * sections[i].matrix;
		}
		// Rebuild the extrusion mesh	
		MeshExtrusion.ExtrudeMesh(baseMesh, rail.GetComponent<MeshFilter>().sharedMesh, finalSections, precomputedEdges, false);
	}
    private Quaternion Slerp(Quaternion p, Quaternion q, float t){
        Quaternion ret;

        float fCos = Quaternion.Dot(p, q);

        if ((1.0f + fCos) > 0.00001)
        {
            float fCoeff0, fCoeff1;

            if ((1.0f - fCos) > 0.00001)
            {
                float omega = Mathf.Acos(fCos);
                float invSin = 1.0f / Mathf.Sin(omega);
                fCoeff0 = Mathf.Sin((1.0f - t) * omega) * invSin;
                fCoeff1 = Mathf.Sin(t * omega) * invSin;
            }
            else
            {
                fCoeff0 = 1.0f - t;
                fCoeff1 = t;
            }

            ret.x = fCoeff0 * p.x + fCoeff1 * q.x;
            ret.y = fCoeff0 * p.y + fCoeff1 * q.y;
            ret.z = fCoeff0 * p.z + fCoeff1 * q.z;
            ret.w = fCoeff0 * p.w + fCoeff1 * q.w;
        }
        else
        {
            float fCoeff0 = Mathf.Sin((1.0f - t) * Mathf.PI * 0.5f);
            float fCoeff1 = Mathf.Sin(t * Mathf.PI * 0.5f);

            ret.x = fCoeff0 * p.x - fCoeff1 * p.y;
            ret.y = fCoeff0 * p.y + fCoeff1 * p.x;
            ret.z = fCoeff0 * p.z - fCoeff1 * p.w;
            ret.w = p.z;
        }

        return ret;
    
	}

}
#endif