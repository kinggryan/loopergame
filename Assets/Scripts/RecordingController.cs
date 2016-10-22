using UnityEngine;
using System.Collections;

public class RecordingController : MonoBehaviour {

    private double startRecordingDSPTime;
    private bool isRecording;
    private bool startRecordingQueued;

    private double[] recordedNotes;
    public AudioSource recordedSource;
    public LoopPlayer loopPlayer;

	// Use this for initialization
	void Start () {
        recordedNotes = new double[0];
	}
	
	// Update is called once per frame
	void Update () {
        if(!isRecording)
        {
            if(startRecordingQueued)
            {
                if (AudioSettings.dspTime <= startRecordingDSPTime)
                {
                    isRecording = true;
                    startRecordingQueued = false;
                }
            }
            else
            {
                if (Input.GetKeyDown("space"))
                {
                    StartRecording();
                }
            }
        }
        else
        {
            if(AudioSettings.dspTime >= startRecordingDSPTime + SongController.GetMeasureLength() - SongController.GetBeatLength()/4)
            {
                StopRecording();
            }
        }	    
	}

    public void StartRecording()
    {
        startRecordingDSPTime = SongController.GetNextStartOfMeasure();
        startRecordingQueued = true;
        Debug.Log("start recording at " + startRecordingDSPTime);
    }

    public void StopRecording()
    {
        Debug.Log("Stop recording " + recordedNotes);
        if(recordedNotes.Length > 0)
            loopPlayer.PlayLoop(recordedNotes, recordedSource);
        isRecording = false;
        recordedNotes = new double[0];
    }

    public void RecordNoteAtTime(double noteDSPTime)
    {
        if(isRecording)
        {
            double noteBeat = (noteDSPTime - startRecordingDSPTime) / 60 * SongController.GetBPM();
            if(noteBeat >= 0 && noteBeat < 4)
            {
                double[] newNotes = new double[recordedNotes.Length + 1];
                for(int i = 0; i < recordedNotes.Length; i++)
                {
                    newNotes[i] = recordedNotes[i];
                }
                newNotes[newNotes.Length - 1] = noteBeat;
                recordedNotes = newNotes;
            }
        }
    }
}
