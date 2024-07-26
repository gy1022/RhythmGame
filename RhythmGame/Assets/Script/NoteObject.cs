using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public Note note;
    public float speed;
    public float hitPosition;
    public float startTime;

    public void Initialize(Note note, float speed, float hitPosition, float startTime)
    {
        this.note = note;
        this.speed = speed;
        this.hitPosition = hitPosition;
        this.startTime = startTime;

        float initialDistance = speed * (note.startTime - (Time.time - startTime));
        transform.position = new Vector3(hitPosition + initialDistance, note.trackIndex * 2, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if(transform.position.x < hitPosition - 1)
        {
            Destroy(gameObject);
        }
    }
}
