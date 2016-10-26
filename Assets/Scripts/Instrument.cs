using UnityEngine;
using System.Collections;

public class Instrument : MonoBehaviour {

    public RecordingController recordingController;
    public AudioSource instrumentSound;
    public int instrumentNumber;

    private string instrumentButtonName;

    // DSP Properties
    int instrumentClipFrameLength;
    int numFramesRead;

    bool playNoteNextUpdate;
    bool stopNextUpdate;

	// Use this for initialization
	void Start () {
        instrumentButtonName = "Instrument" + instrumentNumber;

        instrumentClipFrameLength = instrumentSound.clip.samples;
        numFramesRead = 0;
        playNoteNextUpdate = false;
        stopNextUpdate = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (playNoteNextUpdate || Input.GetButtonDown(instrumentButtonName))
        {
            PlayNote();
        }
	}

    void PlayNote()
    {
        Debug.Log("Playing note");
        if(!instrumentSound.isPlaying)
        {
            playNoteNextUpdate = false;
            double playTime = SongController.GetNearestPlayableNoteTimeFromTime(AudioSettings.dspTime);
            instrumentSound.PlayScheduled(playTime);
            recordingController.RecordNoteAtTime(playTime,instrumentNumber);
        }
    }

    //void OnAudioFilterRead(float[] data, int channels)
    //{
    //    numFramesRead += data.Length/2;
    //    if (numFramesRead >= instrumentClipFrameLength)
    //    {
    //        numFramesRead = 0;
    //        playNoteNextUpdate = true;
    //    }
    //}
}
