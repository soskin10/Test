#if UNITY_EDITOR
using System;
using System.Reflection;
using Project.Scripts.Configs;
using Project.Scripts.Constants;
using Project.Scripts.Services.BoardEdit;
using Project.Scripts.Tiles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project.Scripts.Utils.Editor
{
    public class GDToolsWindow : EditorWindow
    {
        private BoardConfig _boardConfig;


        [MenuItem("Tools/GD Tools")]
        public static void Open()
        {
            GetWindow<GDToolsWindow>("GD Tools");
        }


        private void OnGUI()
        {
            EditorGUILayout.LabelField("Board Edit", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            var activeScene = EditorSceneManager.GetActiveScene();
            var isGameplayActive = Application.isPlaying && activeScene.name == SceneNames.GamePlay;
            EditorGUI.BeginDisabledGroup(false == isGameplayActive);

            EditorGUILayout.BeginHorizontal();

            var size = EditorGUIUtility.singleLineHeight;
            var sprite = GetSpriteForKind(BoardEditMode.SelectedKind);
            var iconRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            if (sprite)
            {
                var uv = new Rect(
                    sprite.rect.x / sprite.texture.width,
                    sprite.rect.y / sprite.texture.height,
                    sprite.rect.width / sprite.texture.width,
                    sprite.rect.height / sprite.texture.height);
                var prevColor = GUI.color;
                if (false == GUI.enabled)
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUI.DrawTextureWithTexCoords(iconRect, sprite.texture, uv);
                GUI.color = prevColor;
            }

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
            if (GUILayout.Button("Copy Console"))
                CopyConsole();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }


        private Sprite GetSpriteForKind(TileKind kind)
        {
            if (!_boardConfig)
            {
                var guids = AssetDatabase.FindAssets("t:BoardConfig");
                if (guids.Length == 0)
                    return null;
                
                _boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (!_boardConfig)
                return null;

            for (var i = 0; i < _boardConfig.RegularTiles.Length; i++)
                if (_boardConfig.RegularTiles[i].Kind == kind)
                    return _boardConfig.RegularTiles[i].Sprite;

            for (var i = 0; i < _boardConfig.SpecialTiles.Length; i++)
                if (_boardConfig.SpecialTiles[i].Kind == kind)
                    return _boardConfig.SpecialTiles[i].Sprite;

            return null;
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