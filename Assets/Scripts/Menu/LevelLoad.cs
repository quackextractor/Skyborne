using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Menu
{
    public class LevelLoad : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraFadeController cameraFadeController;
        [SerializeField] private ParticleSystem motionLinesParticleSystem;
        [SerializeField] private GameObject cameraGameObject;
        [SerializeField] private GameObject uiComponentsToHide;

        [Header("Animation Settings")]
        [Tooltip("The point the camera will look at, then travel to.")]
        [SerializeField] private Transform targetPoint;
        [SerializeField] private float rotateDuration = 2.0f;
        [SerializeField] private float moveDuration = 1.0f;
        [SerializeField] private float postFadeDelay = 0.1f;

        [Header("Fade Settings")]
        [Tooltip("Delay before the fade starts, counted from the start of LoadSequence.")]
        [SerializeField] private float fadeDelay = 0.0f;
        [Tooltip("Duration of the fade to white.")]
        [SerializeField] private float fadeDuration = 1.0f;

        private void Start()
        {
            // Turn off particle emission on start
            var emission = motionLinesParticleSystem.emission;
            emission.enabled = false;
            Time.timeScale = 1f;
        }

        public void LoadLevel()
        {
            StartCoroutine(LoadSequence());
        }

        private IEnumerator LoadSequence()
        {
            // Ensure we have a target
            if (targetPoint == null)
            {
                Debug.LogError("LevelLoad: targetPoint is not assigned!");
                yield break;
            }
            
            uiComponentsToHide.SetActive(false);

            // Start fade after a delay
            StartCoroutine(DelayedFade());

            // Cache start state
            Transform camT = cameraGameObject.transform;
            Vector3 startPos = camT.position;
            Quaternion startRot = camT.rotation;

            // Compute desired end rotation (look directly at the target)
            Vector3 directionToTarget = (targetPoint.position - startPos).normalized;
            Quaternion endRot = Quaternion.LookRotation(directionToTarget);

            // --- ROTATION PHASE ---
            float elapsed = 0f;
            while (elapsed < rotateDuration)
            {
                float t = rotateDuration > 0f
                    ? Mathf.SmoothStep(0f, 1f, elapsed / rotateDuration)
                    : 1f;

                camT.rotation = Quaternion.Slerp(startRot, endRot, t);

                elapsed += Time.deltaTime;
                yield return null;
            }
            camT.rotation = endRot;

            // Motion Lines
            var emission = motionLinesParticleSystem.emission;
            emission.enabled = true;
            motionLinesParticleSystem.Play();

            // --- MOVEMENT PHASE ---
            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                float t = moveDuration > 0f
                    ? Mathf.SmoothStep(0f, 1f, elapsed / moveDuration)
                    : 1f;

                camT.position = Vector3.Lerp(startPos, targetPoint.position, t);

                elapsed += Time.deltaTime;
                yield return null;
            }
            camT.position = targetPoint.position;

            // 4) Small delay so fade can finish cleanly
            yield return new WaitForSeconds(postFadeDelay);

            // 5) Load the next scene
            SceneManager.LoadScene(1);
        }

        private IEnumerator DelayedFade()
        {
            // Wait for the specified delay before starting the fade
            if (fadeDelay > 0f)
                yield return new WaitForSeconds(fadeDelay);

            cameraFadeController.FadeToWhite(fadeDuration);
        }

        public void Terminate()
        {
            Application.Quit();
        }
    }
}
