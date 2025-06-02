// Editor/LevelDataEditor.cs

using System.Linq;
using ScriptObj;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private Texture2D _pixelTex;

        private void OnEnable()
        {
            _pixelTex = new Texture2D(1, 1);
            _pixelTex.SetPixel(0, 0, Color.white);
            _pixelTex.Apply();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelData data = (LevelData)target;
            var positions = data.enemyEntries.Select(e => e.position).ToList();

            if (positions.Count == 0)
            {
                EditorGUILayout.HelpBox("No enemy entries to preview.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Positions Preview", EditorStyles.boldLabel);

            // 1) Compute symmetric world‐bounds around (0,0)
            float maxX = positions.Max(p => Mathf.Abs(p.x));
            float maxY = positions.Max(p => Mathf.Abs(p.y));
            float maxExtent = Mathf.Max(maxX, maxY) * 1.05f; 
            int gridExtent = Mathf.CeilToInt(maxExtent);

            // 2) Decide a square size (max 200px, fits current view)
            float availableWidth = EditorGUIUtility.currentViewWidth - 20f;
            float size = Mathf.Min(availableWidth, 200f);

            // 3) Reserve a square rect (explicit width+height, no ExpandWidth)
            //    Wrap in HORIZONTAL + FlexibleSpace to center it.
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                                           GUILayout.Width(size),
                                           GUILayout.Height(size));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.Box(previewRect, GUIContent.none);

            // 4) Draw fine grid lines at each integer between -gridExtent and +gridExtent
            Color prev = GUI.color;
            GUI.color = new Color(.7f, .7f, .7f, 1f);
            for (int x = -gridExtent; x <= gridExtent; x++)
            {
                float u = (x + maxExtent) / (2 * maxExtent);
                float xPx = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u);
                GUI.DrawTexture(new Rect(xPx, previewRect.yMin, 1f, previewRect.height), _pixelTex);
            }
            for (int y = -gridExtent; y <= gridExtent; y++)
            {
                float v = (y + maxExtent) / (2 * maxExtent);
                // flip Y so +Y goes up
                float yPx = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v);
                GUI.DrawTexture(new Rect(previewRect.xMin, yPx, previewRect.width, 1f), _pixelTex);
            }

            // 5) Draw axes at X=0 and Y=0 (thicker/darker)
            GUI.color = Color.black;
            // Y axis:
            {
                float u0 = (0 + maxExtent) / (2 * maxExtent);
                float x0 = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u0);
                GUI.DrawTexture(new Rect(x0 - 1f, previewRect.yMin, 2f, previewRect.height), _pixelTex);
            }
            // X axis:
            {
                float v0 = (0 + maxExtent) / (2 * maxExtent);
                float y0 = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v0);
                GUI.DrawTexture(new Rect(previewRect.xMin, y0 - 1f, previewRect.width, 2f), _pixelTex);
            }
            GUI.color = prev;

            // 6) Plot each spawn‐point as a red square
            float dotSize = 8f;
            foreach (var pos in positions)
            {
                float u = (pos.x + maxExtent) / (2 * maxExtent);
                float v = (pos.y + maxExtent) / (2 * maxExtent);

                float xPix = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u) - dotSize / 2f;
                float yPix = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v) - dotSize / 2f;

                Color saved = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(xPix, yPix, dotSize, dotSize), _pixelTex);
                GUI.color = saved;
            }
        }
    }
}
