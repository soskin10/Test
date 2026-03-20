#if UNITY_EDITOR
using System;
using System.Reflection;
using Project.Scripts.Services.BoardEdit;
using Project.Scripts.Tiles;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Utils.Editor
{
    public class GDToolsWindow : EditorWindow
    {
        [MenuItem("Tools/GD Tools")]
        public static void Open()
        {
            GetWindow<GDToolsWindow>("GD Tools");
        }


        private void OnGUI()
        {
            EditorGUILayout.LabelField("Board Edit", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginDisabledGroup(false == Application.isPlaying);

            EditorGUILayout.BeginHorizontal();
            BoardEditMode.SelectedKind = (TileKind)EditorGUILayout.EnumPopup(BoardEditMode.SelectedKind);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            var buttonLabel = BoardEditMode.IsActive ? "Stop Edit" : "Start Edit";
            var buttonColor = BoardEditMode.IsActive ? new Color(1f, 0.45f, 0.45f) : new Color(0.45f, 1f, 0.6f);

            var prev = GUI.backgroundColor;
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button(buttonLabel, GUILayout.Height(30)))
                BoardEditMode.SetActive(false == BoardEditMode.IsActive);
            GUI.backgroundColor = prev;

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Copy Console", GUILayout.Height(28)))
                CopyConsole();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }


        private void CopyConsole()
        {
            try
            {
                var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
                if (null == logEntries)
                {
                    Debug.LogError("Failed to access console types");
                    return;
                }

                var getCountMethod = logEntries.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                var startGettingEntriesMethod = logEntries.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var getEntryInternalMethod = logEntries.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                var endGettingEntriesMethod = logEntries.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);

                var count = (int)getCountMethod.Invoke(null, null);
                var sb = new System.Text.StringBuilder();
                startGettingEntriesMethod.Invoke(null, null);
                var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor.dll");
                var logEntry = Activator.CreateInstance(logEntryType);

                for (var i = 0; i < count; i++)
                {
                    getEntryInternalMethod.Invoke(null, new[] { i, logEntry });
                    var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
                    var message = (string)messageField.GetValue(logEntry);
                    var header = message.Split('\n')[0];
                    sb.AppendLine(header);
                }

                endGettingEntriesMethod.Invoke(null, null);
                GUIUtility.systemCopyBuffer = sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while copying console:{ex.Message}");
            }
        }
    }
}
#endif