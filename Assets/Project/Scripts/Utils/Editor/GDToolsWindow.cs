#if UNITY_EDITOR
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
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
#endif