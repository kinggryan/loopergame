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
    public int numLoops = 4;
	private List<TrackLoopInfo> tracks;
    private List<Loop> loops;

	void Start() {
        tracks = new List<TrackLoopInfo>();
        for(int i = 0; i < numInstruments; i++)
        {
            TrackLoopInfo track = new TrackLoopInfo();
            track.audioSources = new List<AudioSource>();
            track.noteBeats = new List<double>();
            tracks.Add(track);
        }

        loops = new List<Loop>();
        for(int i = 0; i < numLoops; i++)
        {
            Loop newLoop = new Loop();
            newLoop.noteBeats = new List<List<double>>();
            for (int inNum = 0; inNum < numInstruments; inNum++)
                newLoop.noteBeats.Add(new List<double>());
            loops.Add(newLoop);
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

    public void PlayLoop(double[][] loopBeats,AudioSource[] originalAudioSources,int loopNumber)
    {
        RemoveLoop(loopNumber);

        Loop loopToAdd = new Loop();
        loopToAdd.noteBeats = new List<List<double>>();
        for (int i = 0; i < loopBeats.Length; i++)
        {
            List<double> loopBeatList = new List<double>(loopBeats[i]);
            loopToAdd.noteBeats.Add(loopBeatList);
        }

        loops[loopNumber] = loopToAdd;
        Loop fixedLoop = GetLoopWithRedundantNotesRemoved(loopToAdd);

		for (int trackNumber = 0; trackNumber < fixedLoop.noteBeats.Count; trackNumber++) {
			if (fixedLoop.noteBeats[trackNumber].Count > 0) {
                // Clean up old beats and loop sources
                // TODO: Recycle loop sources when possible


                // Setup next note time
                TrackLoopInfo track = AddNoteBeatsToTrack(fixedLoop.noteBeats[trackNumber].ToArray(), tracks [trackNumber], originalAudioSources [trackNumber]);
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
                bool addNote = true;
				for (int masterTrackNoteIndex = 0; masterTrackNoteIndex < tracks [trackIndex].noteBeats.Count; masterTrackNoteIndex++) {
					if (Mathf.Approximately((float)tracks [trackIndex].noteBeats [masterTrackNoteIndex],(float)loop.noteBeats[trackIndex][trackNoteIndex])) {
                        addNote = false;
					}
				}
                if(addNote)
                    newNoteBeats.Add(loop.noteBeats[trackIndex][trackNoteIndex]);
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

    private void RemoveLoop(int loopIndex)
    {
        Loop loopToRemove = loops[loopIndex];
        for (int i = 0; i < loops[loopIndex].noteBeats.Count; i++)
            loops[loopIndex].noteBeats[i].Clear();

        // determine which notes in this loop are not duplicated in other loops
        // TODO: Efficiency improvements
        // TODO: Fix all this code
        for (int instrumentNumber = 0; instrumentNumber < loopToRemove.noteBeats.Count; instrumentNumber++)
        {
            foreach (Loop loop in loops)
            {
                loopToRemove.noteBeats[instrumentNumber] = RemoveSharedNoteBeats(loopToRemove.noteBeats[instrumentNumber], loop.noteBeats[instrumentNumber]);
            }
        }

        // Now remove the non-overlapping notes from the note beats
        for(int instrumentNum = 0; instrumentNum < loopToRemove.noteBeats.Count; instrumentNum++)
        {
            List<double> noteBeats = loopToRemove.noteBeats[instrumentNum];
            tracks[instrumentNum] = RemoveNoteBeatsFromTrack(tracks[instrumentNum], noteBeats);
        }
    }

    List<double> RemoveSharedNoteBeats(List<double> noteBeatsToReturn,List<double> noteBeatsToNotOverlap)
    {
        for(int i = 0; i < noteBeatsToReturn.Count; i++)
        {
            foreach(double beat in noteBeatsToNotOverlap)
            {
                if(Mathf.Approximately((float)beat,(float)noteBeatsToReturn[i]))
                {
                    noteBeatsToReturn.RemoveAt(i);
                    i--;
                }
            }
        }

        return noteBeatsToReturn;
    }

    TrackLoopInfo RemoveNoteBeatsFromTrack(TrackLoopInfo track,List<double> noteBeats)
    {
        // Remove note beats
        // TODO: Efficiency improvements
        for(int i = 0; i < noteBeats.Count; i++)
        {
            for(int trackNoteBeatIndex = 0; trackNoteBeatIndex < track.noteBeats.Count; trackNoteBeatIndex++)
            {
                if(Mathf.Approximately((float)track.noteBeats[trackNoteBeatIndex],(float)noteBeats[i]))
                {
                    track.noteBeats.RemoveAt(trackNoteBeatIndex);
                    Object.Destroy(track.audioSources[trackNoteBeatIndex]);
                    track.audioSources.RemoveAt(trackNoteBeatIndex);
                    break;
                }
            }
        }

        // Recalculate next note stuff for track
        if(track.noteBeats.Count > 0)
        {

            track.currentNoteIndex = 0;
            double startOfMeasure = SongController.GetNextStartOfMeasure();
            track.nextNoteTime = startOfMeasure + track.noteBeats[0] / SongController.GetBPM() * 60.0;
        }

        return track;
    }
}
