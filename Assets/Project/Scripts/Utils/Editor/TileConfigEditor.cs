#if UNITY_EDITOR
using Project.Scripts.Configs;
using Project.Scripts.Configs.Board;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Utils.Editor
{
    [CustomEditor(typeof(TileConfig))]
    public class TileConfigEditor : UnityEditor.Editor
    {
        private const int IconGridRows = 3;
        private const float IconPadding = 8f;

        
        private SerializedProperty _kind;
        private SerializedProperty _sprite;
        private SerializedProperty _behaviour;


        private void OnEnable()
        {
            _kind = serializedObject.FindProperty("_kind");
            _sprite = serializedObject.FindProperty("_sprite");
            _behaviour = serializedObject.FindProperty("_behaviour");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var squareSize = lineHeight * IconGridRows + spacing * (IconGridRows - 1);

            EditorGUILayout.BeginHorizontal();

            var iconRect = GUILayoutUtility.GetRect(squareSize, squareSize, GUILayout.Width(squareSize), GUILayout.Height(squareSize));
            var spriteObj = _sprite.objectReferenceValue as Sprite;
            GUI.Box(iconRect, GUIContent.none);

            if (spriteObj)
            {
                var uv = new Rect(
                    spriteObj.rect.x / spriteObj.texture.width,
                    spriteObj.rect.y / spriteObj.texture.height,
                    spriteObj.rect.width / spriteObj.texture.width,
                    spriteObj.rect.height / spriteObj.texture.height);
                var innerRect = new Rect(iconRect.x + IconPadding, iconRect.y + IconPadding, iconRect.width - IconPadding * 2, iconRect.height - IconPadding * 2);
                GUI.DrawTextureWithTexCoords(innerRect, spriteObj.texture, uv);
            }

            GUILayout.Space(4);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(_kind);
            EditorGUILayout.PropertyField(_sprite);
            EditorGUILayout.PropertyField(_behaviour);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif