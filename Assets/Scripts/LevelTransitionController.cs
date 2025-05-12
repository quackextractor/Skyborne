using UnityEngine;
using System.Collections;
using Clouds;
using UnityEngine.Serialization;

public class LevelTransitionController : MonoBehaviour
{
    [Header("Cloud References")]
    public GameObject topCloud;
    public GameObject middleCloud;
    public GameObject bottomCloud;

    [Header("Transition Settings")]
    public float tDistanceTotal = 50f;
    public float tDistanceStop = 25f;
    
    public float tDurationTotal = 2f;
    public float tDurationStop = 1f;

    private float tDistanceRemaining;
    private float tDurationRemaining;

    [Header("Shake Settings")]
    public GameObject platform;
    public Vector3 shakeAmount = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("References")]
    public CloudSpawner cloudSpawner;
    public LevelLoader levelLoader;
    public CameraFadeController cameraFadeController;

    private Vector3 originalPlatformPosition;

    private void Start()
    {
        tDistanceRemaining = tDistanceTotal - tDistanceStop;
        tDurationRemaining = tDurationTotal - tDurationStop;
        
        originalPlatformPosition = platform.transform.position;
        topCloud.SetActive(false);
        PositionTopCloud();
    }

    private void PositionTopCloud()
    {
        topCloud.transform.position = middleCloud.transform.position + Vector3.up * tDistanceTotal;
    }

    public void StartTransition()
    {
        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        cloudSpawner.enableSpawning = false;
        
        topCloud.SetActive(true);

        yield return ShakePlatform(tDurationStop);
        yield return MoveClouds(tDistanceStop, tDurationStop);

        cameraFadeController.FadeToWhite(1);
        levelLoader.LoadLevel();    
        cameraFadeController.FadeFromWhite(1);

        yield return ShakePlatform(tDurationRemaining);
        yield return MoveClouds(tDurationRemaining, tDurationRemaining);

        bottomCloud.SetActive(false);
        RepositionClouds();
        cloudSpawner.enableSpawning = true;
    }

    private IEnumerator MoveClouds(float distance, float duration)
    {
        Vector3 topStart = topCloud.transform.position;
        Vector3 middleStart = middleCloud.transform.position;
        Vector3 bottomStart = bottomCloud.transform.position;

        Vector3 topEnd = topStart + Vector3.down * distance;
        Vector3 middleEnd = middleStart + Vector3.down * distance;
        Vector3 bottomEnd = bottomStart + Vector3.down * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            topCloud.transform.position = Vector3.Lerp(topStart, topEnd, t);
            middleCloud.transform.position = Vector3.Lerp(middleStart, middleEnd, t);
            bottomCloud.transform.position = Vector3.Lerp(bottomStart, bottomEnd, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        topCloud.transform.position = topEnd;
        middleCloud.transform.position = middleEnd;
        bottomCloud.transform.position = bottomEnd;
    }

    private IEnumerator ShakePlatform(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount.x, shakeAmount.x),
                Random.Range(-shakeAmount.y, shakeAmount.y),
                Random.Range(-shakeAmount.z, shakeAmount.z)
            );
            platform.transform.position = originalPlatformPosition + shakeOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        platform.transform.position = originalPlatformPosition;
    }

    private void RepositionClouds()
    {
        bottomCloud.transform.position = topCloud.transform.position + Vector3.up * tDistanceTotal;
        GameObject temp = bottomCloud;
        bottomCloud = middleCloud;
        middleCloud = topCloud;
        topCloud = temp;
    }
}