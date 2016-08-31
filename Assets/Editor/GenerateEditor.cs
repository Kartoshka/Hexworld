using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Generate))]
public class GenerateEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Generate generator = (Generate)target;
        /*
        if (GUILayout.Button("Generate new thingy mabober")) {
            generator.Populate();
        }
        */
        if (GUILayout.Button("Generate chunk at pos")) {
            generator.getChunk(generator.source.transform.position,generator.centerChunk, generator.size, generator.maxNumBlocks);
        }

		if (GUILayout.Button ("Clear")) {
			generator.Clear ();
		}
	}
}
