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
        private int _dragIndex = -1;
        private const float DotSize = 8f;

        private void OnEnable()
        {
            _pixelTex = new Texture2D(1, 1);
            _pixelTex.SetPixel(0, 0, Color.white);
            _pixelTex.Apply();
        }

        public override void OnInspectorGUI()
        {
            // Draw default fields
            DrawDefaultInspector();

            // Fetch our target data
            LevelData data = (LevelData)target;

            var entries = data.enemyEntries;
            if (entries == null || entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No enemy entries to preview.", MessageType.Info);
                return;
            }

            // Collect positions for bounds calculation
            var positions = entries.Select(e => e.position).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Positions Preview", EditorStyles.boldLabel);

            // Compute symmetric world-bounds around (0,0)
            float maxX = positions.Max(p => Mathf.Abs(p.x));
            float maxY = positions.Max(p => Mathf.Abs(p.y));
            float maxExtent = Mathf.Max(maxX, maxY) * 1.05f;
            int gridExtent = Mathf.CeilToInt(maxExtent);

            // Decide a square size (max 200px, fits view)
            float availableWidth = EditorGUIUtility.currentViewWidth - 20f;
            float size = Mathf.Min(availableWidth, 200f);

            // Reserve and center a square rect
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect previewRect = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.Width(size), GUILayout.Height(size)
            );
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Draw background box
            GUI.Box(previewRect, GUIContent.none);

            // Draw grid
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
                float yPx = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v);
                GUI.DrawTexture(new Rect(previewRect.xMin, yPx, previewRect.width, 1f), _pixelTex);
            }

            // Draw axes
            GUI.color = Color.black;
            {
                float u0 = (0 + maxExtent) / (2 * maxExtent);
                float x0 = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u0);
                GUI.DrawTexture(new Rect(x0 - 1f, previewRect.yMin, 2f, previewRect.height), _pixelTex);
            }
            {
                float v0 = (0 + maxExtent) / (2 * maxExtent);
                float y0 = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v0);
                GUI.DrawTexture(new Rect(previewRect.xMin, y0 - 1f, previewRect.width, 2f), _pixelTex);
            }
            GUI.color = prev;

            // Handle mouse events
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Mouse down: begin drag if over a dot
            if (evt.type == EventType.MouseDown && evt.button == 0 && previewRect.Contains(evt.mousePosition))
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    Rect dotRect = GetDotRect(previewRect, positions[i], maxExtent);
                    if (dotRect.Contains(evt.mousePosition))
                    {
                        _dragIndex = i;
                        GUIUtility.hotControl = controlID;
                        evt.Use();
                        break;
                    }
                }
            }
            // Mouse drag: update position (with optional shift-snapping)
            else if (evt.type == EventType.MouseDrag && _dragIndex >= 0)
            {
                Vector2 local = evt.mousePosition - new Vector2(previewRect.xMin, previewRect.yMin);
                float u = Mathf.Clamp01(local.x / previewRect.width);
                float v = Mathf.Clamp01(1f - (local.y / previewRect.height));
                float worldX = Mathf.Lerp(-maxExtent, maxExtent, u);
                float worldY = Mathf.Lerp(-maxExtent, maxExtent, v);

                // Snap to whole values if shift is held
                if (evt.shift)
                {
                    worldX = Mathf.Round(worldX);
                    worldY = Mathf.Round(worldY);
                }

                // Commit back to data
                Undo.RecordObject(data, "Move Spawn Point");
                entries[_dragIndex].position = new Vector2(worldX, worldY);
                EditorUtility.SetDirty(data);

                Repaint();
                evt.Use();
            }
            // Mouse up: end drag
            else if (evt.type == EventType.MouseUp && evt.button == 0 && _dragIndex >= 0)
            {
                _dragIndex = -1;
                GUIUtility.hotControl = 0;
                evt.Use();
            }

            // Finally draw each point (so dragged one draws last)
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 pos = entries[i].position;
                Rect dotRect = GetDotRect(previewRect, pos, maxExtent);

                Color saved = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(dotRect, _pixelTex);
                GUI.color = saved;

                // Change cursor when hovering
                if (evt.type == EventType.Repaint && dotRect.Contains(evt.mousePosition))
                {
                    EditorGUIUtility.AddCursorRect(dotRect, MouseCursor.MoveArrow);
                }
            }
        }

        // Helper: get the on-screen rect for a world position
        private Rect GetDotRect(Rect previewRect, Vector2 pos, float maxExtent)
        {
            float u = (pos.x + maxExtent) / (2 * maxExtent);
            float v = (pos.y + maxExtent) / (2 * maxExtent);

            float xPix = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u) - DotSize / 2f;
            float yPix = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v) - DotSize / 2f;
            return new Rect(xPix, yPix, DotSize, DotSize);
        }
    }
}
