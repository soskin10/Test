using UnityEngine;

namespace Project.Scripts.Utils
{
    [ExecuteAlways]
    public class FullscreenBackground : MonoBehaviour
    {
        private void Awake()
        {
            var mainCamera = Camera.main;
            if (!mainCamera) 
                return;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (!sr || !sr.sprite)
                return;

            float camHeight = mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * mainCamera.aspect;
            Vector2 camSize = new Vector2(camWidth, camHeight);

            Vector2 spriteSize = sr.sprite.bounds.size;

            float scaleX = camSize.x / spriteSize.x;
            float scaleY = camSize.y / spriteSize.y;
            float uniformScale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
            transform.position = Vector3.zero;
        }
    }
}