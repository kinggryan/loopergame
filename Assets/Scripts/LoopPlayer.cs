using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LoopPlayer : MonoBehaviour {

	private struct Loop {
		public List<List<double>> noteBeats;
	}

	private struct TrackLoopInfo {
		public List<AudioSource> audioSources;
		public List<double> noteBeats;
		public double nextNoteTime;
		public int currentNoteIndex;
	}

    // For now, always assuming 4 beat measures
	public double preScheduleNextNoteTime = -0.5;
    public InstrumentBlockBreaker blockBreaker;

    public int numInstruments;
	private List<TrackLoopInfo> tracks;

	void Start() {
        tracks = new List<TrackLoopInfo>();
        for(int i = 0; i < numInstruments; i++)
        {
            TrackLoopInfo track = new TrackLoopInfo();
            track.audioSources = new List<AudioSource>();
            track.noteBeats = new List<double>();
            tracks.Add(track);
        }
	}

	// Update is called once per frame
	void Update () {
		for(int i = 0; i < tracks.Count; i++) {
            TrackLoopInfo track = tracks[i];
			while (track.noteBeats.Count > 0 && AudioSettings.dspTime >= track.nextNoteTime + preScheduleNextNoteTime)
            {
                blockBreaker.PlayInstrumentInSeconds(i,(float)(track.nextNoteTime - AudioSettings.dspTime));
                track = ScheduleNextNoteForTrack(track);
                tracks[i] = track;
            }
        }
	}

	TrackLoopInfo ScheduleNextNoteForTrack(TrackLoopInfo track)
    {
        // Schedule the note to play
		track.audioSources[track.currentNoteIndex].PlayScheduled(track.nextNoteTime);
        
        // When the next note plays
		track.currentNoteIndex++;
		if(track.currentNoteIndex >= track.noteBeats.Count)
        {
			track.currentNoteIndex = 0;
			track.nextNoteTime += (4 - track.noteBeats[track.noteBeats.Count - 1] + track.noteBeats[0])/SongController.GetBPM()*60;
        }
        else
        {
			track.nextNoteTime += (track.noteBeats[track.currentNoteIndex] - track.noteBeats[track.currentNoteIndex - 1])/SongController.GetBPM()*60;
        }

		return track;
    }

    public void PlayLoop(double[][] loopBeats,AudioSource[] originalAudioSources)
    {
		for (int trackNumber = 0; trackNumber < loopBeats.Length; trackNumber++) {
			if (loopBeats [trackNumber].Length > 0) {
				// Clean up old beats and loop sources
				// TODO: Recycle loop sources when possible


				// Setup next note time
				TrackLoopInfo track = AddNoteBeatsToTrack (loopBeats [trackNumber], tracks [trackNumber], originalAudioSources [trackNumber]);
				track.currentNoteIndex = 0;
				double startOfMeasure = SongController.GetNextStartOfMeasure ();
				track.nextNoteTime = startOfMeasure + track.noteBeats [0] / SongController.GetBPM () * 60.0;
				tracks [trackNumber] = track;
			}
		}        
    }

	private Loop GetLoopWithRedundantNotesRemoved(Loop loop)
	{
		// Iterate through the tracks and ensure that there are no overlapping 
		Loop adjustedLoop = loop;
		for (int trackIndex = 0; trackIndex < loop.noteBeats.Count; trackIndex++) {
			// If the loop overlaps a beat from the master track, then remove that note beat
			List<double> newNoteBeats = new List<double>();

			// For all the beats in this track, determine if they're equal to a beat in the existing track
			for (int trackNoteIndex = 0; trackNoteIndex < loop.noteBeats[trackIndex].Count; trackNoteIndex++) {
				for (int masterTrackNoteIndex = 0; masterTrackNoteIndex < tracks [trackIndex].noteBeats.Count; masterTrackNoteIndex++) {
					
					if (!Mathf.Approximately((float)tracks [trackIndex].noteBeats [masterTrackNoteIndex],(float)loop.noteBeats[trackIndex][trackNoteIndex])) {
						newNoteBeats.Add (loop.noteBeats[trackIndex] [trackNoteIndex]);
					}
				}
			}

			adjustedLoop.noteBeats [trackIndex] = newNoteBeats;
		}

		return adjustedLoop;
	}

	private TrackLoopInfo AddNoteBeatsToTrack(double[] noteBeats,TrackLoopInfo track,AudioSource originalAudioSource)
	{
		int insertionIndex = 0;
		for (int i = 0; i < noteBeats.Length; i++) {
			double newNoteBeat = noteBeats [i];

			// Find the index to insert this note beat at
			while (insertionIndex < track.noteBeats.Count && track.noteBeats [insertionIndex] < newNoteBeat)
				insertionIndex++;

			// Now that it's been found, we need to insert it into the arrays
			track.noteBeats.Insert (insertionIndex, newNoteBeat);
			track.audioSources.Insert (insertionIndex, AddLoopPlayerWithOriginal (originalAudioSource));

			// For now, don't recalculate the current note, just make sure it points to the same note it was pointing to.
			if (track.currentNoteIndex > insertionIndex)
				track.currentNoteIndex++;
		}

		return track;
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
