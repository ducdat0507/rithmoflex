using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PlayableSong))]
public class PlayableSongEditor : Editor 
{

    public override void OnInspectorGUI() 
    {
        PlayableSong song = (PlayableSong)target;

        int currentIndent = EditorGUI.indentLevel;

        GUIStyle lStyle = new GUIStyle(GUI.skin.GetStyle("label"));
        lStyle.fontSize = 16;
        GUILayout.TextField(song.SongName, lStyle);
        lStyle.fontSize = 12;
        GUILayout.TextField(song.SongArtist, lStyle);

        EditorGUI.indentLevel = 0;

        if (GUILayout.Button("Open in Charter"))
        {
            Charter.Open(song);
        }

        EditorGUI.indentLevel = currentIndent;
    }
}