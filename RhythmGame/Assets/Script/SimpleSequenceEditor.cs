using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class SimpleSequenceEditor : EditorWindow
{
    private SequenceData sequenceData;
    private Vector2 scrolPos;
    private float beatHedight = 20;
    private float trackWidth = 50;
    private int totalBeats;
    private bool isPlaying = false;
    private int currentBeatTime = 0;
    private int playFromBeat = 0;
    private float startTime = 0;
    private AudioSource audioSource;

    [MenuItem("Tool/Simple Sequence Editor")]

    private static void ShowWindow()
    {
        var window = GetWindow<SimpleSequenceEditor>();
        window.titleContent = new GUIContent("Simple Sequencer");
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
        CreateAudioSource();
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
            audioSource = null;
        }
    }

    private void CreateAudioSource()
    {
        var audioSourceObject = new GameObject("EditorAudioSource");
        audioSourceObject.hideFlags = HideFlags.HideAndDontSave;
        audioSource = audioSourceObject.AddComponent<AudioSource>();
    }

    private void InitializeTracks()
    {
        if (sequenceData == null) return;

        if (sequenceData.trackNotes == null)
        {
            sequenceData.trackNotes = new List<List<int>>();
        }

        while(sequenceData.trackNotes.Count < sequenceData.numberOFTracks)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }

        foreach(var track in sequenceData.trackNotes)
        {
            while(track.Count < totalBeats)
            {
                track.Add(0);
            }
        }

        if(audioSource != null)
        {
            audioSource.clip = sequenceData.audioClip;
        }
    }

    private void Update()
    {
        if (this == null)
        {
            EditorApplication.update -= Update;
            return;
        }

        if(isPlaying && audioSource != null && audioSource.isPlaying)
        {
            float elapseTime = audioSource.time;
            currentBeatTime = Mathf.FloorToInt(elapseTime * sequenceData.bpm / 60f);

            if (currentBeatTime >= totalBeats)
            {
                StopPlayback();
            }
            Repaint();
        }
    }

    private void StartPlayback(int fromBeat)
    {
        if (sequenceData == null || sequenceData.audioClip == null || audioSource == null) return;

        isPlaying = true;
        currentBeatTime = fromBeat;
        playFromBeat = fromBeat;

        if (audioSource.clip != sequenceData.audioClip)
        {
            audioSource.clip = sequenceData.audioClip;
        }

        float startTime = fromBeat * 60f / sequenceData.bpm;
        audioSource.time = startTime;
        audioSource.Play();

        this.startTime = (float)EditorApplication.timeSinceStartup - startTime;
        EditorApplication.update += Update;
    }

    private void PausePlayback()
    {
        isPlaying = false;
        if (audioSource != null) audioSource.Pause();
    }

    private void StopPlayback()
    {
        isPlaying = false;
        currentBeatTime = 0;
        playFromBeat = 0;

        if (audioSource != null) audioSource.Stop();
        EditorApplication.update -= Update;
    }

    private void DrawBeat(int trackIndex, int beatIndex)
    {
        if (sequenceData == null || sequenceData.trackNotes == null || trackIndex >= sequenceData.trackNotes.Count) return;

        Rect rect = GUILayoutUtility.GetRect(trackIndex, beatIndex);
        bool isCurrentBeat = currentBeatTime == beatIndex;
        int noteValue = (sequenceData.trackNotes[trackIndex].Count > beatIndex) ? sequenceData.trackNotes[trackIndex][beatIndex] : 0;

        Color color = Color.gray;
        if (isCurrentBeat) color = Color.cyan;
        else
        {
            switch(noteValue)
            {
                case 1: color = Color.green; break;
                case 2: color = Color.yellow; break;
                case 3: color = Color.red; break;
                case 4: color = Color.blue; break;
            }
        }

        EditorGUI.DrawRect(rect, color);

        if(Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if(Event.current.button == 0)
            {
                noteValue = (noteValue + 1) % 5;
                while (sequenceData.trackNotes[trackIndex].Count <= beatIndex)
                {
                    sequenceData.trackNotes[trackIndex].Add(0);
                }
                sequenceData.trackNotes[trackIndex][beatIndex] = noteValue;
            }
            else if(Event.current.button == 1)
            {
                if (sequenceData.trackNotes[trackIndex].Count > beatIndex)
                {
                    sequenceData.trackNotes[trackIndex][beatIndex] = 0;
                }
                
            }
            Event.current.Use();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("시퀀스 데이터 설정 " , EditorStyles.boldLabel);

        sequenceData = (SequenceData)EditorGUILayout.ObjectField("시퀀스 데이터", sequenceData, typeof(SequenceData), false);

        if(sequenceData == null)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        InitializeTracks();

        EditorGUILayout.LabelField("BPM", sequenceData.bpm.ToString());
        EditorGUILayout.LabelField("오디오 클립", EditorStyles.boldLabel);
        int newNumberOfTracks = EditorGUILayout.IntField("트랙 수", sequenceData.numberOFTracks);
        if (newNumberOfTracks != sequenceData.numberOFTracks)
        {
            sequenceData.numberOFTracks = newNumberOfTracks;
            InitializeTracks();
        }

        if(sequenceData.numberOFTracks < 1) sequenceData.numberOFTracks = 1;

        if(sequenceData.audioClip != null)
        {
            totalBeats = Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm);
        }
        else
        {
            totalBeats = 0;
        }

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button(isPlaying ? "일시 정지" : "재생"))
        {
            if (isPlaying) PausePlayback();
            else StartPlayback(currentBeatTime);
        }
        if(GUILayout.Button("처음부터 재생"))
        {
            StartPlayback(0);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("특정 비트부터 재생" , EditorStyles.boldLabel);
        playFromBeat = EditorGUILayout.IntSlider("비트 인덱스", playFromBeat, 0, totalBeats - 1);
        if(GUILayout.Button("해당 비트부터 재생"))
        {
            StartPlayback(playFromBeat);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        scrolPos = EditorGUILayout.BeginScrollView(scrolPos, GUILayout.Height(position.height - 210));
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        GUILayout.Space(beatHedight);

        for(int j = 0; j < totalBeats; j++)
        {
            float beatTime = j * 60 / sequenceData.bpm;
            int minutes = Mathf.FloorToInt(beatTime / 60f);
            int seconds = Mathf.FloorToInt(beatTime % 60f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{minutes:00}:{seconds:00}", GUILayout.Width(50));
            EditorGUILayout.LabelField($"{j}", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(beatHedight - EditorGUIUtility.singleLineHeight);
        }
        EditorGUILayout.EndVertical();

        for(int i = 0; i < sequenceData.numberOFTracks; i++)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label($"트랙 {i + 1}" , GUILayout.Width(trackWidth));
            for(int j = 0;j < totalBeats; j++)
            {
                DrawBeat(i, j);
            }
            EditorGUILayout.EndVertical ();
        }

        if(GUILayout.Button("데이터 저장"))
        {
            sequenceData.SaveToJason();
        }

        if(GUILayout.Button("데이터 불러오기"))
        {
            sequenceData.LoadFromJson();
            InitializeTracks();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
}
