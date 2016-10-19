using UnityEngine;
using System.Collections;

public class Instrument : MonoBehaviour {

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

        //// Read the audio data from the existing clip.
        //float[] samples = new float[instrumentSound.clip.samples * instrumentSound.clip.channels];
        //instrumentSound.clip.GetData(samples, 0);

        //// Create the new clp
        //AudioClip newClip = AudioClip.Create(instrumentSound.clip.name + "_wCallback", instrumentSound.clip.samples, instrumentSound.clip.channels, instrumentSound.clip.frequency, false, null, InstrumentClipPositionChanged);
        //newClip.SetData(samples, 0);
        //instrumentSound.clip = newClip;
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
        //instrumentSound.Stop();
        if(!instrumentSound.isPlaying)
        {
            playNoteNextUpdate = false;
            double playTime = SongController.GetNearestBeatTimeFromTime(AudioSettings.dspTime);
            instrumentSound.PlayScheduled(playTime);
        }
    }

    //// Audio Callbacks
    //void InstrumentClipPositionChanged(int frameNumber)
    //{
    //    Debug.Log(frameNumber);
    //}

    void OnAudioFilterRead(float[] data, int channels)
    {
        numFramesRead += data.Length/2;
        if (numFramesRead >= instrumentClipFrameLength)
        {
            numFramesRead = 0;
            playNoteNextUpdate = true;
           // Debug.Log("File finished!");
        }
    }
}
