using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]


public class Note 
{
    public int trackIndex;
    public float startTime;
    public float duration;

    public Note(int trackIndex, float startTime, float duration)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
    }
}
