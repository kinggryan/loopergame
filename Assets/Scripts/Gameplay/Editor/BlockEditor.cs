using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FallingBlock))]
public class BlockEditor : Editor {

    static float bpm = 120;
    static float blockOffset = -0.1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        bpm = EditorGUILayout.FloatField("BPM", bpm);
        blockOffset = EditorGUILayout.FloatField("Block Offset", blockOffset);

        FallingBlock block = (FallingBlock)serializedObject.targetObject;

        float blockBeat = (block.transform.position.y - 0.5f*block.blockHeight - blockOffset) / block.fallSpeed / 60f * bpm;
        blockBeat = EditorGUILayout.FloatField("Block Beat", blockBeat);
        block.transform.position = new Vector3(block.transform.position.x, (0.5f*block.blockHeight + blockOffset) + (blockBeat / bpm * 60 * block.fallSpeed), block.transform.position.z);


    }
}
