using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{

    public SequenceData sequenceData;
    public NoteManager noteManager;
    public float playbackSpeed = 1.0f;
    private bool notesGenerated = false;

    // Start is called before the first frame update
    void Start()
    {
        if (sequenceData == null)
        {
            Debug.LogError("시퀀스 데이터 없음");
            return;
        }

        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            InitializeTrackANotes();
        }

        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        notesGenerated = false;
        GenerateNotes();
        noteManager.Initialize();
    }


    private void InitializeTrackANotes()
    {
        sequenceData.trackNotes = new List<List<int>>();
        for (int i = 0; i < sequenceData.trackNotes.Count; i++)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }
    }

    private void GenerateNotes()
    {
        if (notesGenerated) return;

        noteManager.notes.Clear();

        for (int trackIndex = 0; trackIndex < sequenceData.trackNotes.Count; trackIndex++)
        {
            for (int beatIndex = 0; beatIndex < sequenceData.trackNotes[trackIndex].Count; beatIndex++)
            {
                int noteValue = sequenceData.trackNotes[trackIndex][beatIndex];
                if (noteValue != 0)
                {
                    float startTime = beatIndex * 60f / sequenceData.bpm;
                    float duration = noteValue * 60f / sequenceData.bpm;
                    Note note = new Note(trackIndex, startTime, duration);
                    noteManager.AddNote(note);
                }
            }
        }
        notesGenerated = true;
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        noteManager.SetSpeed(speed);
    }

    public void LoadSequenceDataFromJson()
    {
        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        { 
            InitializeTrackANotes();
        }

        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        notesGenerated = false;
        GenerateNotes();
        noteManager.Initialize();
    }



}
