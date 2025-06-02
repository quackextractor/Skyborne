using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class TachometerHUD : MaskableGraphic
    {
        [Header("Value Range")]
        [Tooltip("Minimum value (will map to -90°).")]
        public float minValue = 1f;
        [Tooltip("Maximum value (will map to +90°).")]
        public float maxValue = 5f;
        [Header("Zone Thresholds")]
        [Tooltip("Enter vibration in this zone (orange) at and above this value.")]
        public float yellowThreshold = 3f;
        [Tooltip("Enter intense vibration in this zone (red) at and above this value.")]
        public float redThreshold = 4f;

        [Header("Arc Appearance")]
        [Tooltip("Thickness of the colored arcs as a fraction of radius (0..1).")]
        [Range(0.1f, 0.9f)]
        public float arcThickness = 0.2f;
        [Tooltip("Number of segments per zone (higher = smoother).")]
        public int segmentsPerZone = 60;

        [Header("Needle Appearance")]
        [Tooltip("Needle width as fraction of total width (0..1).")]
        [Range(0.005f, 0.05f)]
        public float needleWidth = 0.02f;
        [Tooltip("Needle length as fraction of radius (0..1).")]
        [Range(0.5f, 1f)]
        public float needleLength = 0.9f;
        [Tooltip("Base color of the needle.")]
        public Color needleColor = Color.white;

        [Header("Vibration Settings")]
        [Tooltip("Maximum angular jitter (in degrees) while in yellow zone.")]
        public float yellowJitter = 1f;
        [Tooltip("Maximum angular jitter (in degrees) while in red zone.")]
        public float redJitter = 3f;
        [Tooltip("Frequency of vibration (cycles per second).")]
        public float vibrationFrequency = 20f;

        // Current “logical” value (1..5)
        private float currentValue = 1f;
        // Smoothed angle we draw the needle at
        private float displayedAngle = -90f;
        // Raw target angle before vibration
        private float targetAngle = -90f;
        // Cached Rect dimensions
        private float radius;
        private Vector2 center;

        protected override void OnEnable()
        {
            base.OnEnable();
            // Initialize to minValue:
            SetValue(minValue);
        }

        /// <summary>
        /// Public method: set the tachometer’s reading (clamped between minValue and maxValue).
        /// Call this from your other scripts whenever you want to update the gauge.
        /// </summary>
        public void SetValue(float v) 
        {
            currentValue = Mathf.Clamp(v, minValue, maxValue);
            // Map currentValue → angle in degrees: minValue => -90°, maxValue => +90°
            float t = (currentValue - minValue) / (maxValue - minValue);
            targetAngle = Mathf.Lerp(-90f, 90f, t);
            SetVerticesDirty(); 
        }

        void Update()
        {
            // Determine if we should vibrate: 
            bool inYellow = (currentValue >= yellowThreshold && currentValue < redThreshold);
            bool inRed = (currentValue >= redThreshold);

            float jitter = 0f;
            if (inRed)
            {
                jitter = redJitter;
            }
            else if (inYellow)
            {
                jitter = yellowJitter;
            }

            // If either zone, apply a sinusoidal jitter around targetAngle
            if (jitter > Mathf.Epsilon)
            {
                float osc = Mathf.Sin(Time.time * vibrationFrequency * Mathf.PI * 2f);
                displayedAngle = targetAngle + osc * jitter;
            }
            else
            {
                displayedAngle = targetAngle;
            }

            // We only need to redraw the needle when the angle changes noticeably
            SetVerticesDirty();
        }

        /// <summary>
        /// Override the UI mesh generation. We build:
        ///   1) Three colored arc segments (green/orange/red).
        ///   2) A needle (triangle) pointing at 'displayedAngle'.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            center = rect.center;
            // Use half of the narrower dimension as radius
            radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            // 1) Draw the three colored arcs
            DrawZone(vh, minValue, yellowThreshold, Color.green);
            DrawZone(vh, yellowThreshold, redThreshold, new Color(1f, 0.65f, 0f)); // orange
            DrawZone(vh, redThreshold, maxValue, Color.red);

            // 2) Draw the needle on top
            DrawNeedle(vh);
        }

        /// <summary>
        /// Draws one “zone” (arc) from valueStart → valueEnd, using the given color.
        /// Internally converts value → angles and builds a ring segment of thickness arcThickness.
        /// </summary>
        void DrawZone(VertexHelper vh, float valueStart, float valueEnd, Color zoneColor)
        {
            // If zone is zero‐width, skip
            if (valueEnd <= valueStart) return;

            // Map each boundary to an angle in degrees
            float tStart = Mathf.Clamp01((valueStart - minValue) / (maxValue - minValue));
            float tEnd = Mathf.Clamp01((valueEnd - minValue) / (maxValue - minValue));
            float angleStart = Mathf.Lerp(-90f, 90f, tStart) * Mathf.Deg2Rad;
            float angleEnd = Mathf.Lerp(-90f, 90f, tEnd) * Mathf.Deg2Rad;

            int segCount = Mathf.Max(1, Mathf.RoundToInt(segmentsPerZone * (tEnd - tStart)));
            float innerR = radius * (1f - arcThickness);
            float outerR = radius;

            // Generate points around the two radii
            List<Vector2> innerPts = new List<Vector2>(segCount + 1);
            List<Vector2> outerPts = new List<Vector2>(segCount + 1);

            for (int i = 0; i <= segCount; i++)
            {
                float f = (float)i / segCount;
                float angle = Mathf.Lerp(angleStart, angleEnd, f);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                innerPts.Add(center + dir * innerR);
                outerPts.Add(center + dir * outerR);
            }

            // For each subsector, add a quad (two triangles)
            int baseIndex = vh.currentVertCount;
            for (int i = 0; i < segCount; i++)
            {
                // Vertices: 
                //   innerPts[i], innerPts[i+1], outerPts[i+1], outerPts[i]
                UIVertex v0 = UIVertex.simpleVert;
                v0.position = innerPts[i];
                v0.color = zoneColor;

                UIVertex v1 = UIVertex.simpleVert;
                v1.position = innerPts[i + 1];
                v1.color = zoneColor;

                UIVertex v2 = UIVertex.simpleVert;
                v2.position = outerPts[i + 1];
                v2.color = zoneColor;

                UIVertex v3 = UIVertex.simpleVert;
                v3.position = outerPts[i];
                v3.color = zoneColor;

                vh.AddVert(v0);
                vh.AddVert(v1);
                vh.AddVert(v2);
                vh.AddVert(v3);

                // Two triangles: (0,1,2) and (2,3,0)
                vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 2);
                vh.AddTriangle(baseIndex + 2, baseIndex + 3, baseIndex + 0);

                baseIndex += 4;
            }
        }

        /// <summary>
        /// Draws a simple triangular “needle” pointing at displayedAngle.
        /// </summary>
        void DrawNeedle(VertexHelper vh)
        {
            // Convert displayedAngle → radians
            float angRad = displayedAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));

            float len = radius * needleLength;
            float halfW = (rectTransform.rect.width * needleWidth) * 0.5f;

            // The tip of the needle:
            Vector2 tip = center + dir * len;

            // Compute perp direction to build base width
            Vector2 perp = new Vector2(-dir.y, dir.x);

            Vector2 baseCenter = center; 
            Vector2 leftBase = baseCenter + perp * halfW;
            Vector2 rightBase = baseCenter - perp * halfW;

            int startIdx = vh.currentVertCount;

            UIVertex vt0 = UIVertex.simpleVert;
            vt0.position = tip;
            vt0.color = needleColor;

            UIVertex vt1 = UIVertex.simpleVert;
            vt1.position = leftBase;
            vt1.color = needleColor;

            UIVertex vt2 = UIVertex.simpleVert;
            vt2.position = rightBase;
            vt2.color = needleColor;

            vh.AddVert(vt0);
            vh.AddVert(vt1);
            vh.AddVert(vt2);

            vh.AddTriangle(startIdx + 0, startIdx + 1, startIdx + 2);
        }

        // Ensure the component redraws if any serialized field changes in‐editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            segmentsPerZone = Mathf.Max(1, segmentsPerZone);
            arcThickness = Mathf.Clamp01(arcThickness);
            needleLength = Mathf.Clamp01(needleLength);
            needleWidth = Mathf.Clamp01(needleWidth);
            SetVerticesDirty();
        }
#endif
    }
}
