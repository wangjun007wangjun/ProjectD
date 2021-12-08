using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace MonoB.Msic
{
    public class FullScreenSprite : MonoBehaviour
    {


        public float DesignWidth = 750;
        public float DesignHeight = 1334;
        public Camera Cam2D;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;


        void Start()
        {
            StartCoroutine(AdaptCoroutine());
        }

        private IEnumerator AdaptCoroutine()
        {
            yield return null;
            Adapt();
        }
        public void Adapt()
        {
            if (Cam2D == null)
            {
                Cam2D = Camera.main;
            }

            float cameraHeight = Cam2D.orthographicSize * 2;
            Vector2 cameraSize = new Vector2(Cam2D.aspect * cameraHeight, cameraHeight);

            Vector2 spriteSize = Vector2.zero;
            if (_spriteRenderer == null)
            {
                spriteSize = new Vector2(DesignWidth / 100.0f, DesignHeight / 100.0f);
            }
            else
            {
                spriteSize = _spriteRenderer.sprite.bounds.size;
            }

            Vector2 scale = transform.localScale;
            float squareAspect = DesignWidth / DesignHeight;

            if (Cam2D.aspect <= squareAspect)
            {
                scale *= cameraSize.y / spriteSize.y;
            }
            else
            {
                scale *= cameraSize.x / spriteSize.x;
            }

            transform.position = Vector2.zero; // Optional
            transform.localScale = scale;
        }
    }
}