using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Generate))]
public class GenerateEditor : Editor {

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		Generate generator = (Generate)target;
		if(GUILayout.Button("Generate new thingy mabober")){
			generator.Populate();
		}
		if (GUILayout.Button ("Clear")) {
			generator.Clear ();
		}
	}
}
