using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstrumentBlockBreaker : MonoBehaviour {

    public Bucket[] buckets;

    private int[][] instrumentBuckets;

	// Use this for initialization
	void Start () {
        instrumentBuckets = new int[][]{new int[] { 0 } };
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlayInstrumentInSeconds(int instrumentNumber,float seconds)
    {
        StartCoroutine(PlayInstrumentAndBreakBlocksInSeconds(instrumentNumber, seconds));
    }

    IEnumerator PlayInstrumentAndBreakBlocksInSeconds(int instrumentNumber,float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // Now break blocks
        bool brokeBlock = false;
        foreach(int bucketNum in instrumentBuckets[instrumentNumber])
        {
            brokeBlock = brokeBlock | buckets[bucketNum].BreakBlock();
        }

        // TODO: Care about if a block broke or not
    }
}
