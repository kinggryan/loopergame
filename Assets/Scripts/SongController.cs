using UnityEngine;
using System.Collections;

public class SongController : MonoBehaviour {

    private static double bpm;
    private static int timeSignatureNumerator;

    public double startingBPM;
    public AudioSource songPlayer;

    private static double startDSPTime;

    void Start()
    {
        bpm = startingBPM;
        timeSignatureNumerator = 4;
        PlaySong();
    }

	void PlaySong()
    {
        startDSPTime = AudioSettings.dspTime + 0.25;
        songPlayer.PlayScheduled(startDSPTime);
    }

    static public double GetBPM()
    {
        return bpm;
    }

    static public double GetNearestBeatTimeFromTime(double unadjustedTime)
    {
        double returnTime = startDSPTime;
        double beatLength = GetBeatLength();
        while(returnTime < unadjustedTime)
        {
            returnTime += beatLength;
        }
        return returnTime; //  returnTime + 0.5 * beatLength > unadjustedTime ? returnTime - beatLength : returnTime;
    }

    static public double GetNextStartOfMeasure()
    {
        double currentTime = AudioSettings.dspTime;
        double returnTime = startDSPTime;
        double measureLength = GetMeasureLength();
        while (returnTime < currentTime)
        {
            returnTime += measureLength;
        }
        return returnTime;
    }

    static private double GetBeatLength()
    {
        return 60.0 / bpm;
    }

    /**
        Returns the length of a measure in seconds.
    */
    static private double GetMeasureLength()  
    {
        return timeSignatureNumerator / bpm * 60;
    }
}
