using UnityEngine;
using System.Collections;

public class FallingBlock : MonoBehaviour {

	public float fallSpeed;
	public float blockHeight;

	private double previousDSPTime;

	void Start() {
		previousDSPTime = AudioSettings.dspTime;
	}

	// Update is called once per frame
	void Update () {
		double currentDSPTime = AudioSettings.dspTime;

		float dspTimeDelta = (float)(currentDSPTime - previousDSPTime);
		transform.position = new Vector3(transform.position.x,transform.position.y - fallSpeed * dspTimeDelta,transform.position.z);
		previousDSPTime = currentDSPTime;
	}
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        Bucket bucket = collider.GetComponent<Bucket>();
        if (bucket)
        {
            bucket.ConnectWithBlock(this);
        }
    }
}
