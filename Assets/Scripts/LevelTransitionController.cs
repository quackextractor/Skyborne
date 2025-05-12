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
    private bool isLevelLoaded = false;

    private void Start()
    {
        Debug.Log($"Initial values - Total: {tDistanceTotal}, Stop: {tDistanceStop}");
        
        tDistanceRemaining = tDistanceTotal - tDistanceStop;
        tDurationRemaining = tDurationTotal - tDurationStop;
        
        Debug.Log($"Calculated values - Remaining Distance: {tDistanceRemaining}, Remaining Duration: {tDurationRemaining}");
        Debug.Log($"Cloud Movement - Stop Distance: {tDistanceStop}, Remaining Distance: {tDistanceRemaining}, Total: {tDistanceTotal}");
        
        topCloud.SetActive(false);
        PositionTopCloud();
    }
    
    private void Awake()
    {
        // Store the original position in Awake to ensure it's captured before any Start() methods run
        originalPlatformPosition = platform.transform.position;
    }

    private void PositionTopCloud()
    {
        topCloud.transform.position = middleCloud.transform.position + Vector3.up * tDistanceTotal;
    }

    public void StartTransition()
    {
        // Capture the platform position right before transition starts
        originalPlatformPosition = platform.transform.position;
        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        cloudSpawner.enableSpawning = false;
        
        topCloud.SetActive(true);

        // Recalculate distances at transition start in case they were changed
        tDistanceRemaining = tDistanceTotal - tDistanceStop;
        tDurationRemaining = tDurationTotal - tDurationStop;
        Debug.Log($"Transition start - Total: {tDistanceTotal}, Stop: {tDistanceStop}, Remaining: {tDistanceRemaining}");

        // Start shaking platform continuously until fade is complete
        Coroutine shakeCoroutine = StartCoroutine(ShakePlatformUntilWhite());
        
        // Start the first cloud movement and fade timing
        float fadeStartDelay = tDurationStop * 0.5f; // Start fade halfway through
        
        Debug.Log($"Starting first cloud movement: {tDistanceStop} units over {tDurationStop} seconds");
        // Start moving clouds
        StartCoroutine(MoveClouds(tDistanceStop, tDurationStop));
        
        // Wait before starting fade to white
        yield return new WaitForSeconds(fadeStartDelay);
        cameraFadeController.FadeToWhite();
        
        // Wait for the first cloud movement to finish
        yield return new WaitForSeconds(tDurationStop - fadeStartDelay);
        
        // Wait until camera is fully white before loading level
        yield return new WaitUntil(() => cameraFadeController.Alpha >= 1f);
        
        // Stop shaking once fully white
        StopCoroutine(shakeCoroutine);
        platform.transform.position = originalPlatformPosition;
        
        // Load the level
        Debug.Log("Loading level...");
        levelLoader.LoadLevel();
        isLevelLoaded = true;
        
        // Resume shaking after level is loaded
        Coroutine resumeShakeCoroutine = StartCoroutine(ShakePlatformContinuously());
        
        // Start fading from white
        cameraFadeController.FadeFromWhite();
        
        // Move the clouds the remaining distance if there is any
        if (tDistanceRemaining > 0)
        {
            Debug.Log($"Starting second cloud movement: {tDistanceRemaining} units over {tDurationRemaining} seconds");
            yield return MoveClouds(tDistanceRemaining, tDurationRemaining);
        }
        else
        {
            Debug.LogWarning($"No remaining distance to move. Total: {tDistanceTotal}, Stop: {tDistanceStop}");
        }
        
        // Stop shaking after transition is complete
        StopCoroutine(resumeShakeCoroutine);
        platform.transform.position = originalPlatformPosition;

        bottomCloud.SetActive(false);
        RepositionClouds();
        cloudSpawner.enableSpawning = true;
        isLevelLoaded = false;
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

    private IEnumerator ShakePlatformUntilWhite()
    {
        Vector3 shakeStartPosition = platform.transform.position;
        
        while (cameraFadeController.Alpha < 1f)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount.x, shakeAmount.x),
                Random.Range(-shakeAmount.y, shakeAmount.y),
                Random.Range(-shakeAmount.z, shakeAmount.z)
            );
            platform.transform.position = shakeStartPosition + shakeOffset;
            yield return null;
        }
        platform.transform.position = shakeStartPosition;
    }

    private IEnumerator ShakePlatformContinuously()
    {
        Vector3 shakeStartPosition = platform.transform.position;
        
        while (true)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount.x, shakeAmount.x),
                Random.Range(-shakeAmount.y, shakeAmount.y),
                Random.Range(-shakeAmount.z, shakeAmount.z)
            );
            platform.transform.position = shakeStartPosition + shakeOffset;
            yield return null;
        }
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