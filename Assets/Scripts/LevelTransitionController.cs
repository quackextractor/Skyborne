using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    public float whiteScreenDelay;

    [Header("Shake Settings")]
    public GameObject platform;
    public Vector3 shakeAmount = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("References")]
    public CloudSpawner cloudSpawner;
    public LevelLoader levelLoader;
    public CameraFadeController cameraFadeController;

    private Queue<IEnumerator> _transitionQueue = new Queue<IEnumerator>();
    private bool _isTransitioning = false;
    private Vector3 _originalPlatformPosition;
    private bool _isLevelLoaded;
    private float _tDistanceRemaining;
    private float _tDurationRemaining;

    private void Awake()
    {
        // Store the original platform position
        _originalPlatformPosition = platform.transform.position;
    }

    private void Start()
    {
        // Initialize remaining calculations
        _tDistanceRemaining = tDistanceTotal - tDistanceStop;
        _tDurationRemaining = tDurationTotal - tDurationStop;

        // Prepare top cloud
        topCloud.SetActive(false);
        PositionTopCloud();
    }

    /// <summary>
    /// Enqueue a transition request. Only one transition runs at a time.
    /// </summary>
    public void StartTransition()
    {
        // Enqueue the transition coroutine
        _transitionQueue.Enqueue(TransitionRoutine());

        // If not already transitioning, start processing the queue
        if (!_isTransitioning)
        {
            StartCoroutine(ProcessTransitionQueue());
        }
    }

    /// <summary>
    /// Processes queued transitions one by one.
    /// </summary>
    private IEnumerator ProcessTransitionQueue()
    {
        _isTransitioning = true;

        while (_transitionQueue.Count > 0)
        {
            // Dequeue and execute the next transition
            yield return StartCoroutine(_transitionQueue.Dequeue());
        }

        _isTransitioning = false;
    }

    /// <summary>
    /// The core transition routine.
    /// </summary>
    private IEnumerator TransitionRoutine()
    {
        cloudSpawner.enableSpawning = false;
        topCloud.SetActive(true);

        // Recalculate in case settings changed
        _tDistanceRemaining = tDistanceTotal - tDistanceStop;
        _tDurationRemaining = tDurationTotal - tDurationStop;

        // Shake until white, then resume after load
        Coroutine shakeUntilWhite = StartCoroutine(ShakePlatformUntilWhite());

        // First cloud sweep and fade
        float fadeStartDelay = tDurationStop * 0.5f;
        StartCoroutine(MoveClouds(tDistanceStop, tDurationStop));
        yield return new WaitForSeconds(fadeStartDelay);
        cameraFadeController.FadeToWhite();
        yield return new WaitForSeconds(tDurationStop - fadeStartDelay);
        yield return new WaitUntil(() => cameraFadeController.Alpha >= 1f);

        // Stop initial shake
        StopCoroutine(shakeUntilWhite);
        platform.transform.position = _originalPlatformPosition;

        // Load level and resume shake
        levelLoader.LoadNextLevel();
        _isLevelLoaded = true;
        Coroutine shakeAfterLoad = StartCoroutine(ShakePlatformContinuously());

        // Fade from white after delay
        StartCoroutine(FadeFromWhiteAfterDelay());

        // Second cloud sweep if needed
        if (_tDistanceRemaining > 0)
        {
            yield return MoveClouds(_tDistanceRemaining, _tDurationRemaining);
        }

        // Cleanup
        StopCoroutine(shakeAfterLoad);
        platform.transform.position = _originalPlatformPosition;
        bottomCloud.SetActive(false);
        RepositionClouds();
        cloudSpawner.enableSpawning = true;
        _isLevelLoaded = false;
    }

    private IEnumerator FadeFromWhiteAfterDelay()
    {
        yield return new WaitForSeconds(whiteScreenDelay);
        cameraFadeController.FadeFromWhite();
    }

    private IEnumerator MoveClouds(float distance, float duration)
    {
        Vector3 topStart = topCloud.transform.position;
        Vector3 middleStart = middleCloud.transform.position;
        Vector3 bottomStart = bottomCloud.transform.position;

        Vector3 offset = Vector3.down * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            topCloud.transform.position = Vector3.Lerp(topStart, topStart + offset, t);
            middleCloud.transform.position = Vector3.Lerp(middleStart, middleStart + offset, t);
            bottomCloud.transform.position = Vector3.Lerp(bottomStart, bottomStart + offset, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact placement
        topCloud.transform.position = topStart + offset;
        middleCloud.transform.position = middleStart + offset;
        bottomCloud.transform.position = bottomStart + offset;
    }

    private IEnumerator ShakePlatformUntilWhite()
    {
        Vector3 basePos = platform.transform.position;
        while (cameraFadeController.Alpha < 1f)
        {
            Vector3 shake = new Vector3(
                Random.Range(-shakeAmount.x, shakeAmount.x),
                Random.Range(-shakeAmount.y, shakeAmount.y),
                Random.Range(-shakeAmount.z, shakeAmount.z)
            );
            platform.transform.position = basePos + shake;
            yield return null;
        }
        platform.transform.position = basePos;
    }

    private IEnumerator ShakePlatformContinuously()
    {
        Vector3 basePos = platform.transform.position;
        while (true)
        {
            Vector3 shake = new Vector3(
                Random.Range(-shakeAmount.x, shakeAmount.x),
                Random.Range(-shakeAmount.y, shakeAmount.y),
                Random.Range(-shakeAmount.z, shakeAmount.z)
            );
            platform.transform.position = basePos + shake;
            yield return null;
        }
    }

    private void PositionTopCloud()
    {
        topCloud.transform.position = middleCloud.transform.position + Vector3.up * tDistanceTotal;
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
