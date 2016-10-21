using UnityEngine;
using System.Collections;

public class LoopPlayer : MonoBehaviour {

    // For now, always assuming 4 beat measures
    public AudioSource[] loopSources;
    public double[] noteBeats;
    public double nextNoteTime;
    public double preScheduleNextNoteTime = -0.5;
    public int currentNoteIndex;

	// Update is called once per frame
	void Update () {
        while(loopSources.Length > 0 && AudioSettings.dspTime >= nextNoteTime - preScheduleNextNoteTime)
            ScheduleNextNote();
	}

    void ScheduleNextNote()
    {
		Debug.Log ("Scheduling noote at time " + nextNoteTime + " for index " + currentNoteIndex);
        // Schedule the note to play
        loopSources[currentNoteIndex].PlayScheduled(nextNoteTime);
        
        // When the next note plays
        currentNoteIndex++;
        if(currentNoteIndex >= noteBeats.Length)
        {
            currentNoteIndex = 0;
			nextNoteTime += (4 - noteBeats[noteBeats.Length - 1] + noteBeats[0])/SongController.GetBPM()*60;
        }
        else
        {
			nextNoteTime += (noteBeats[currentNoteIndex] - noteBeats[currentNoteIndex - 1])/SongController.GetBPM()*60;
        }
    }

    public void PlayLoop(double[] loopBeats,AudioSource originalAudioSource)
    {
        // Clean up old beats and loop sources
        // TODO: Recycle loop sources when possible
        noteBeats = null;
        foreach(AudioSource source in loopSources)
        {
            GameObject.Destroy(source);
        }
        loopSources = null;

        // Setup new players and loop times array.
        noteBeats = loopBeats;
        loopSources = new AudioSource[loopBeats.Length];
        for(int i = 0; i < loopBeats.Length; i++)
        {
            loopSources[i] = AddLoopPlayerWithOriginal(originalAudioSource);
        }

        // Setup next note time
        currentNoteIndex = 0;
        double startOfMeasure = SongController.GetNextStartOfMeasure();
        nextNoteTime = startOfMeasure + noteBeats[0] / SongController.GetBPM() * 60.0;
//        ScheduleNextNote();
    }

    private AudioSource AddLoopPlayerWithOriginal(AudioSource originalAudioSource)
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = originalAudioSource.clip;
        newSource.bypassListenerEffects = originalAudioSource.bypassListenerEffects;
        newSource.bypassReverbZones = originalAudioSource.bypassReverbZones;
        newSource.volume = originalAudioSource.volume;
        newSource.pitch = originalAudioSource.pitch;

        return newSource;
    }
}
