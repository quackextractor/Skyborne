using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    public GameObject hitEffectPrefab; // Reference to the hit effect prefab
    public AudioClip hitSound; // Reference to the hit sound clip
    public float hitSoundVolume = 1.0f; // Volume of the hit sound (range 0.0 to 1.0)

    // Call this method when an enemy is hit
    public void SpawnHitEffect(Vector3 hitPosition)
    {
        if (hitEffectPrefab != null)
        {
            var effect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            Debug.LogWarning("hitEffectPrefab is not assigned!");
        }

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, hitPosition, hitSoundVolume);
        else
            Debug.LogWarning("hitSound is not assigned!");
    }
}