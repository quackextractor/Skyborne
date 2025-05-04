using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CloudBehavior : MonoBehaviour
{
    private Vector3 center;
    private Vector3 direction;
    private float speed;
    private float fadeDistance;
    private float sizeMod;

    private Renderer rend;
    private Material mat;
    private Color baseColor;
    private float travelSign;

    private bool initialized;

    public void Initialize(Vector3 center, Vector3 dir, float speed, float fadeDistance, float sizeMod)
    {
        this.center = center;
        this.direction = dir.normalized;
        this.speed = speed;
        this.fadeDistance = fadeDistance;
        this.sizeMod = sizeMod;

        // Apply uniform scale
        transform.localScale = Vector3.one * sizeMod;

        // Cache renderer and material
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("CloudBehavior: Missing Renderer component.");
            return;
        }
        mat = rend.material;
        baseColor = mat.color;

        // Determine initial travel direction towards center
        travelSign = Vector3.Dot((transform.position - center), direction) > 0 ? -1f : 1f;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        // Move cloud
        transform.position += direction * (speed * Time.deltaTime * travelSign);

        // Compute alpha based on distance along axis
        float dist = Vector3.Dot((transform.position - center), direction);
        float absDist = Mathf.Abs(dist);
        float alpha = absDist <= fadeDistance
            ? 1f
            : Mathf.Clamp01(1f - ((absDist - fadeDistance) / fadeDistance));

        // Apply transparency
        mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        // Deactivate when fully transparent
        if (alpha <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}