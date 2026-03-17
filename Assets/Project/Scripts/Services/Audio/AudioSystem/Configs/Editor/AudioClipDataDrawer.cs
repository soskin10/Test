using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Services.Audio.AudioSystem.Configs.Editor
{
    [CustomPropertyDrawer(typeof(AudioClipData))]
    public class AudioClipDataDrawer : PropertyDrawer
    {
        private static AudioSource _previewSource;
        private static GameObject _previewObject;
        private static bool _isPlaying;
        

        static AudioClipDataDrawer()
        {
            EditorApplication.quitting += StopClip;
        }
        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4 + 15;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = 2f;
            var buttonWidth = (position.width - 5f) / 2;
            var clipProperty = property.FindPropertyRelative("Clip");
            var tagProperty = property.FindPropertyRelative("Tag");
            var volumeProperty = property.FindPropertyRelative("DefaultVolume");
            var clipRect = new Rect(position.x, position.y, position.width, lineHeight);
            
            EditorGUI.PropertyField(clipRect, clipProperty, new GUIContent("Clip"));
            position.y += lineHeight + spacing;
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, lineHeight), tagProperty, new GUIContent("Tag"));
            position.y += lineHeight + spacing;
            EditorGUI.Slider(new Rect(position.x, position.y, position.width, lineHeight), volumeProperty, 0f, 1f, new GUIContent("Volume"));
            position.y += lineHeight + spacing + 2;
            
            var playButtonRect = new Rect(position.x, position.y, buttonWidth, lineHeight);
            var stopButtonRect = new Rect(position.x + buttonWidth + 5f, position.y, buttonWidth, lineHeight);
            
            if (GUI.Button(playButtonRect, "▶ Play") && clipProperty.objectReferenceValue) 
                PlayClip((AudioClip)clipProperty.objectReferenceValue, volumeProperty.floatValue);
            if (GUI.Button(stopButtonRect, "Stop")) 
                StopClip();
            EditorGUI.EndProperty();
        }
        

        private static void PlayClip(AudioClip clip, float volume)
        {
            if (!clip)
                return;
            
            StopClip();
            _previewObject = new GameObject("TempAudioSource");
            _previewObject.hideFlags = HideFlags.HideAndDontSave;
            _previewSource = _previewObject.AddComponent<AudioSource>();
            _previewSource.clip = clip;
            _previewSource.volume = volume;
            _previewSource.Play();
            _isPlaying = true;
            EditorApplication.update += CheckAudioState;
        }

        private static void StopClip()
        {
            if (_previewSource)
            {
                _previewSource.Stop();
                GameObject.DestroyImmediate(_previewSource.gameObject);
                _previewSource = null;
                _previewObject = null;
                _isPlaying = false;
                EditorApplication.update -= CheckAudioState;
            }
        }

        private static void CheckAudioState()
        {
            if (!_previewSource || false == _previewSource.isPlaying) 
                StopClip();
        }
    }
}