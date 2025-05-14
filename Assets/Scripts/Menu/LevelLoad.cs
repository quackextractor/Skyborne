using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class LevelLoad : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraFadeController cameraFadeController;
        [SerializeField] private ParticleSystem motionLinesParticleSystem;
        [SerializeField] private GameObject cameraGameObject;

        [Header("Animation Settings")]
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 targetEulerAngles;
        [SerializeField] private float moveDuration = 1.0f;
        [SerializeField] private float rotateDuration = 2.0f;
        [SerializeField] private float postFadeDelay = 0.1f;

        private void Start()
        {
            // Turn off particle emission on start
            var emission = motionLinesParticleSystem.emission;
            emission.enabled = false;
        }

        public void LoadLevel()
        {
            StartCoroutine(LoadSequence());
        }

        private IEnumerator LoadSequence()
        {
            // Determine overall time (so fade covers the full motion)
            float totalDuration = Mathf.Max(moveDuration, rotateDuration);

            // 1) Fade to white over totalDuration
            cameraFadeController.FadeToWhite(totalDuration);

            // 2) Enable and play motion-lines particles
            var emission = motionLinesParticleSystem.emission;
            emission.enabled = true;
            motionLinesParticleSystem.Play();

            // Cache start transform data
            Transform camT     = cameraGameObject.transform;
            Vector3   startPos = camT.position;
            Quaternion startRot = camT.rotation;
            Quaternion endRot   = Quaternion.Euler(targetEulerAngles);

            float elapsed = 0f;
            while (elapsed < totalDuration)
            {
                // How far along are we in each
                float tMove   = moveDuration  > 0f ? Mathf.Clamp01(elapsed / moveDuration)   : 1f;
                float tRotate = rotateDuration > 0f ? Mathf.Clamp01(elapsed / rotateDuration) : 1f;

                // Interpolate position and rotation separately
                camT.position = Vector3.Lerp(startPos, targetPosition, tMove);
                camT.rotation = Quaternion.Slerp(startRot, endRot,     tRotate);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to final values
            camT.position = targetPosition;
            camT.rotation = endRot;

            // 4) Small delay so fade can finish cleanly
            yield return new WaitForSeconds(postFadeDelay);

            // 5) Finally load the next scene
            SceneManager.LoadScene(1);
        }

        public void Terminate()
        {
            Application.Quit();
        }
    }
}
