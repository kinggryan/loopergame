using UnityEngine;
using System.Collections;

public class Instrument : MonoBehaviour {

    public AudioSource instrumentSound;
    public int instrumentNumber;

    private string instrumentButtonName;

	// Use this for initialization
	void Start () {
        instrumentButtonName = "Instrument" + instrumentNumber;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown(instrumentButtonName))
        {
            double playTime = SongController.GetNearestBeatTimeFromTime(AudioSettings.dspTime);
            instrumentSound.PlayScheduled(playTime);
        }
	}
}
