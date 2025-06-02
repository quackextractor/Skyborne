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
            // Draw default inspector for other fields
            DrawDefaultInspector();

            LevelData data = (LevelData)target;
            var entries = data.enemyEntries;
            if (entries == null || entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No enemy entries to preview.", MessageType.Info);
                return;
            }

            // Gather positions for bounds calculation
            var positions = entries.Select(e => e.position).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Positions Preview", EditorStyles.boldLabel);

            // Compute symmetric world-bounds
            float maxX = positions.Max(p => Mathf.Abs(p.x));
            float maxY = positions.Max(p => Mathf.Abs(p.y));
            float maxExtent = Mathf.Max(maxX, maxY) * 1.05f;
            int gridExtent = Mathf.CeilToInt(maxExtent);

            // Determine preview size
            float availW = EditorGUIUtility.currentViewWidth - 20f;
            float size = Mathf.Min(availW, 200f);

            // Center the preview rect
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect previewRect = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.Width(size), GUILayout.Height(size)
            );
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Draw background
            GUI.Box(previewRect, GUIContent.none);

            // Draw grid lines
            Color old = GUI.color;
            GUI.color = new Color(.7f, .7f, .7f, 1f);
            for (int x = -gridExtent; x <= gridExtent; x++)
            {
                float u = (x + maxExtent) / (2f * maxExtent);
                float xPx = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u);
                GUI.DrawTexture(new Rect(xPx, previewRect.yMin, 1f, previewRect.height), _pixelTex);
            }
            for (int y = -gridExtent; y <= gridExtent; y++)
            {
                float v = (y + maxExtent) / (2f * maxExtent);
                float yPx = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v);
                GUI.DrawTexture(new Rect(previewRect.xMin, yPx, previewRect.width, 1f), _pixelTex);
            }

            // Draw axes
            GUI.color = Color.black;
            {
                float u0 = (0 + maxExtent) / (2f * maxExtent);
                float x0 = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u0);
                GUI.DrawTexture(new Rect(x0 - 1f, previewRect.yMin, 2f, previewRect.height), _pixelTex);
            }
            {
                float v0 = (0 + maxExtent) / (2f * maxExtent);
                float y0 = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v0);
                GUI.DrawTexture(new Rect(previewRect.xMin, y0 - 1f, previewRect.width, 2f), _pixelTex);
            }
            GUI.color = old;

            // Handle mouse events
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Right-click: delete entry
            if (evt.type == EventType.MouseDown && evt.button == 1 && previewRect.Contains(evt.mousePosition))
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    if (GetDotRect(previewRect, positions[i], maxExtent).Contains(evt.mousePosition))
                    {
                        Undo.RecordObject(data, "Delete Spawn Point");
                        entries.RemoveAt(i);
                        EditorUtility.SetDirty(data);
                        evt.Use();
                        return;
                    }
                }
            }
            // Left-click: drag existing or add new
            if (evt.type == EventType.MouseDown && evt.button == 0 && previewRect.Contains(evt.mousePosition))
            {
                bool clickedDot = false;
                for (int i = 0; i < positions.Count; i++)
                {
                    if (GetDotRect(previewRect, positions[i], maxExtent).Contains(evt.mousePosition))
                    {
                        _dragIndex = i;
                        GUIUtility.hotControl = controlID;
                        evt.Use();
                        clickedDot = true;
                        break;
                    }
                }

                if (!clickedDot)
                {
                    // Add new entry at nearest integer grid
                    Vector2 local = evt.mousePosition - new Vector2(previewRect.xMin, previewRect.yMin);
                    float u = Mathf.Clamp01(local.x / previewRect.width);
                    float v = Mathf.Clamp01(1f - local.y / previewRect.height);
                    float worldX = Mathf.Lerp(-maxExtent, maxExtent, u);
                    float worldY = Mathf.Lerp(-maxExtent, maxExtent, v);

                    float intX = Mathf.Round(worldX);
                    float intY = Mathf.Round(worldY);

                    // Clone last entry's prefab and stats
                    var last = entries[entries.Count - 1];
                    var newEntry = new EnemyEntry
                    {
                        enemyPrefab = last.enemyPrefab,
                        enemyStats  = last.enemyStats,
                        position    = new Vector2(intX, intY)
                    };

                    Undo.RecordObject(data, "Add Spawn Point");
                    entries.Add(newEntry);
                    EditorUtility.SetDirty(data);
                    Repaint();
                    evt.Use();
                }
            }
            // Drag to move (with optional Shift-snapping)
            else if (evt.type == EventType.MouseDrag && _dragIndex >= 0)
            {
                Vector2 local = evt.mousePosition - new Vector2(previewRect.xMin, previewRect.yMin);
                float u = Mathf.Clamp01(local.x / previewRect.width);
                float v = Mathf.Clamp01(1f - local.y / previewRect.height);
                float worldX = Mathf.Lerp(-maxExtent, maxExtent, u);
                float worldY = Mathf.Lerp(-maxExtent, maxExtent, v);

                if (evt.shift)
                {
                    worldX = Mathf.Round(worldX);
                    worldY = Mathf.Round(worldY);
                }

                Undo.RecordObject(data, "Move Spawn Point");
                entries[_dragIndex].position = new Vector2(worldX, worldY);
                EditorUtility.SetDirty(data);
                Repaint();
                evt.Use();
            }
            // End drag
            else if (evt.type == EventType.MouseUp && evt.button == 0 && _dragIndex >= 0)
            {
                _dragIndex = -1;
                GUIUtility.hotControl = 0;
                evt.Use();
            }

            // Draw all points (dragged last)
            for (int i = 0; i < entries.Count; i++)
            {
                Rect r = GetDotRect(previewRect, entries[i].position, maxExtent);
                Color save = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(r, _pixelTex);
                GUI.color = save;

                if (evt.type == EventType.Repaint && r.Contains(evt.mousePosition))
                    EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
            }
        }

        // Calculates the on-screen rect for a world-space position
        private Rect GetDotRect(Rect previewRect, Vector2 pos, float maxExtent)
        {
            float u = (pos.x + maxExtent) / (2f * maxExtent);
            float v = (pos.y + maxExtent) / (2f * maxExtent);

            float xPix = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u) - DotSize / 2f;
            float yPix = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v) - DotSize / 2f;
            return new Rect(xPix, yPix, DotSize, DotSize);
        }
    }
}
