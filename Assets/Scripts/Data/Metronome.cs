using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[System.Serializable]
public class Metronome
{
    public List<BPMStop> Stops = new List<BPMStop>();

    public Metronome(float startBPM) {
        Stops.Add(new BPMStop(startBPM, 0));
    }
    public Metronome(float startBPM, int startOffset) {
        Stops.Add(new BPMStop(startBPM, startOffset));
    }

    public float ToBeat(int ms) {
        float beat = 0;
        if (Stops.Count == 0) return beat;
        for (int a = 0; a < Stops.Count; a++) 
        {
            BPMStop stop = Stops[a];
            float b = (ms - stop.Offset) / (60000 / stop.BPM);
            if (a + 1 < Stops.Count) 
            {
                float c = (Stops[a+1].Offset - stop.Offset) / (60 / stop.BPM);
                if (b <= c) return beat + b;
                beat += c;
            }
            else 
            {
                return beat + b;
            }
        }
        return beat;
    }

    public int ToMilliseconds(float beat) {
        if (Stops.Count == 0) return 0;
        for (int a = 0; a < Stops.Count; a++) 
        {
            BPMStop stop = Stops[a];
            float b = beat * (60000 / stop.BPM) + stop.Offset;
            if (a + 1 < Stops.Count) 
            {
                float c = (Stops[a+1].Offset - stop.Offset) / (60 / stop.BPM);
                if (beat <= c) return (int)(b);
                beat -= c;
            }
            else 
            {
                return (int)(b);
            }
        }
        return 0;
    }

    public BPMStop GetStop(int ms) {
        int tag = 0;
        while (tag < Stops.Count - 1 && Stops[tag].Offset < ms) tag++;
        return Stops[tag];
    }
}

[System.Serializable]
public class BPMStop
{
    public int Offset;
    public float BPM;
    public int Signature = 4;

    public BPMStop(float bpm, int offset) {
        Offset = offset;
        BPM = bpm;
    }
}
