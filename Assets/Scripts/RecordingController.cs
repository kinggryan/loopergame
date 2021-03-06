﻿using UnityEngine;
using System.Collections;

public class RecordingController : MonoBehaviour {

    private double startRecordingDSPTime;
    private bool isRecording;
    private bool startRecordingQueued;

    private double[][] recordedNotes;
    public AudioSource[] instrumentSources;
    public LoopPlayer loopPlayer;

    public int numLoops = 4;
    private int selectedLoop = 0;

	// Use this for initialization
	void Start () {
		ResetRecordedNotes ();
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

            if(Input.GetKeyDown("1"))
            {
                selectedLoop = 0;
            }
            if (Input.GetKeyDown("2"))
            {
                selectedLoop = 1;
            }
            if (Input.GetKeyDown("3"))
            {
                selectedLoop = 2;
            }
            if (Input.GetKeyDown("4"))
            {
                selectedLoop = 3;
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

	void ResetRecordedNotes()
	{
		recordedNotes = new double[instrumentSources.Length][];
		for (int i = 0; i < instrumentSources.Length; i++)
			recordedNotes [i] = new double[0];
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
        if (recordedNotes.Length > 0)
        {
            loopPlayer.PlayLoop(recordedNotes, instrumentSources, selectedLoop);
        }
        isRecording = false;
		ResetRecordedNotes ();
	}

	public void RecordNoteAtTime(double noteDSPTime,int instrumentNumber)
    {
        if(isRecording)
        {
            double noteBeat = (noteDSPTime - startRecordingDSPTime) / 60 * SongController.GetBPM();
            if(noteBeat >= 0 && noteBeat < 4)
            {
                double[] newNotes = new double[recordedNotes[instrumentNumber].Length + 1];
                for(int i = 0; i < recordedNotes[instrumentNumber].Length; i++)
                {
                    newNotes[i] = recordedNotes[instrumentNumber][i];
                }
                newNotes[newNotes.Length - 1] = noteBeat;
                recordedNotes[instrumentNumber] = newNotes;
            }
        }
    }
}
