using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSequence" , menuName = "Sequencer/Sequence")]

public class SequenceData : MonoBehaviour
{
    public int bpm;
    public int numberOFTracks;
    public AudioClip audioClip;
    public List<List<int>> trackNotes = new List<List<int>>();
    public TextAsset trackJsonFile;


    public void SaveToJason()
    {
        if (trackJsonFile == null)
        {
            Debug.LogError("Track JSON 파일이 없습니다.");
            return;
        }

        var data = JsonConvert.SerializeObject(new
        {
            bpm,
            numberOFTracks,
            AdioClipPath = AssetDatabase.GetAssetPath(audioClip),
            trackNotes
        }, Formatting.Indented);

        System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(trackJsonFile), data);
        AssetDatabase.Refresh();

    }

    public void LoadFromJson()
    {
        if (trackJsonFile == null)
        {
            Debug.LogError("Track JSON 파일이 없습니다.");
            return;
        }

        var data = JsonConvert.DeserializeAnonymousType(trackJsonFile.text, new
        {
            bpm = 0,
            numberOFTracks = 0,
            AudioClipPath = "",
            trackNotes = new List<List<int>>()
        });

        bpm = data.bpm;
        numberOFTracks = data.numberOFTracks;
        audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(data.AudioClipPath);
        trackNotes = data.trackNotes;
    }

    [CustomEditor(typeof(SequenceData))]

    public class SequenceDataEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var sequenceData = (SequenceData)target;

            DrawDefaultInspector();

            if (sequenceData != null)
            {
                EditorGUILayout.LabelField("Track Notes", EditorStyles.boldLabel);
                for (int i = 0; i < sequenceData.trackNotes.Count; i++)
                {
                    EditorGUILayout.LabelField($"Track {i + 1} : [{string.Join(",", sequenceData.trackNotes[i])}]");
                }
            }

            if (GUILayout.Button("Load from JSON")) sequenceData.LoadFromJson();
            if (GUILayout.Button("Save from JSON")) sequenceData.SaveToJason();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(sequenceData);
            }

        }

    }

}
