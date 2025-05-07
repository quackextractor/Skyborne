using UnityEngine;

namespace Clouds
{
    public class CloudBehavior : MonoBehaviour
    {
        [Header("Fade Settings")] [SerializeField]
        private float fadeInDuration = 1.5f;

        [SerializeField] private float fadeOutDuration = 1.5f;
        private Vector3 direction;
        private Vector3 endPoint;
        private bool initialized;
        private float lifetime;

        private Vector3 originalScale;
        private float speed;
        private float timer;

        private void Update()
        {
            if (!initialized) return;

            transform.position += direction * (speed * Time.deltaTime);
            timer += Time.deltaTime;

            if (timer < fadeInDuration)
            {
                var t = Mathf.Clamp01(timer / fadeInDuration);
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            }
            else
            {
                var remainingDist = Vector3.Distance(transform.position, endPoint);
                var fadeT = Mathf.Clamp01(1f - remainingDist / (speed * fadeOutDuration));
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, fadeT);
            }

            if (timer >= lifetime || Vector3.Distance(transform.position, endPoint) < 0.1f)
            {
                gameObject.SetActive(false);
                initialized = false;
            }
        }

        public void Initialize(Vector3 dir, float speed, float sizeMod, float lifetime, Vector3 endPoint)
        {
            direction = dir;
            this.speed = speed;
            this.lifetime = lifetime;
            timer = 0f;
            this.endPoint = endPoint;

            originalScale = Vector3.one * sizeMod;
            transform.localScale = Vector3.zero;
            initialized = true;
        }
    }
}