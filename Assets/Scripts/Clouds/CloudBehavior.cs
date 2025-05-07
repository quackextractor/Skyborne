using UnityEngine;

namespace Clouds
{
    public class CloudBehavior : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private float timer;
        private bool initialized;

        private Vector3 originalScale;
        private Vector3 endPoint;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 1.5f;

        public void Initialize(Vector3 dir, float speed, float sizeMod, float lifetime, Vector3 endPoint)
        {
            this.direction    = dir;
            this.speed        = speed;
            this.lifetime     = lifetime;
            this.timer        = 0f;
            this.endPoint     = endPoint;

            this.originalScale = Vector3.one * sizeMod;
            transform.localScale = Vector3.zero;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            transform.position += direction * (speed * Time.deltaTime);
            timer += Time.deltaTime;

            if (timer < fadeInDuration)
            {
                float t = Mathf.Clamp01(timer / fadeInDuration);
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            }
            else
            {
                float remainingDist = Vector3.Distance(transform.position, endPoint);
                float fadeT = Mathf.Clamp01(1f - (remainingDist / (speed * fadeOutDuration)));
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, fadeT);
            }

            if (timer >= lifetime || Vector3.Distance(transform.position, endPoint) < 0.1f)
            {
                gameObject.SetActive(false);
                initialized = false;
            }
        }
    }
}