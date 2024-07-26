using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public AudioClip audioClip;
    public List<Note> notes = new List<Note>();
    public float bpm = 120f;
    public float speed = 1f;
    public GameObject notePrefabs;

    public float audioLatency = 0.1f;
    public float hitPosition = -8.0f;
    public float noteSpeed = 10;

    private AudioSource audioSource;
    public float startTime;
    private List<Note> activeNotes = new List<Note>();
    private float spawnOffset;

    public bool debugMode = false;
    public GameObject hitPositionMarker;

    public float initialDelay = 3f;

    public void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        startTime = Time.time + initialDelay;
        activeNotes.Clear();
        activeNotes.AddRange(notes);
        spawnOffset = (10 - hitPosition) / noteSpeed;

        if (debugMode)
        {
            CreateHitPositionMarker();
        }

        StartCoroutine(StartAudioWithDelay());
    }

    private IEnumerator StartAudioWithDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        audioSource.Play();
    }

    void Update()
    {
        float currentTime = Time.time - startTime;

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            if (currentTime >= note.startTime - spawnOffset && currentTime < note.startTime + note.duration)
            {
                SpawnNoteObject(note);
                activeNotes.RemoveAt(i);
            }
            else if(currentTime >= note.startTime + note.duration)
            {
                activeNotes.RemoveAt(i);
            }
        }
    }

    public void AddNote(Note note)
    {
        notes.Add(note);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void SpawnNoteObject(Note note)
    {
        GameObject noteObject = Instantiate(notePrefabs, new Vector3(10, note.trackIndex * 2, 0), Quaternion.identity);
        noteObject.GetComponent<NoteObject>().Initialize(note, noteSpeed, hitPosition, startTime);
    }

    public void AdjustAudioLatency(float latency)
    {
        audioLatency = latency;
    }    

    private void CreateHitPositionMarker()
    {
        hitPositionMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hitPositionMarker.transform.position = new Vector3(hitPosition, 0, 0);
        hitPositionMarker.transform.localScale = new Vector3(0.1f, 10.0f, 1.0f);
    }
}
