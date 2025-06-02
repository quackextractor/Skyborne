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
            // Prepare a 1×1 white texture for drawing
            _pixelTex = new Texture2D(1, 1);
            _pixelTex.SetPixel(0, 0, Color.white);
            _pixelTex.Apply();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw all properties except the enemyEntries list
            DrawPropertiesExcluding(serializedObject, "enemyEntries");

            // Access the serialized list
            SerializedProperty entriesProp = serializedObject.FindProperty("enemyEntries");

            // If there are entries, draw the preview above the list
            if (entriesProp != null && entriesProp.arraySize > 0)
            {
                // Gather positions
                LevelData data = (LevelData)target;
                var positions = data.enemyEntries.Select(e => e.position).ToList();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Spawn Positions Preview", EditorStyles.boldLabel);

                // Compute bounds
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

                // Draw the background and grid
                GUI.Box(previewRect, GUIContent.none);
                DrawGrid(previewRect, gridExtent, maxExtent);

                // Handle input & draw dots
                HandleMouse(previewRect, data, maxExtent);
                DrawDots(previewRect, data, maxExtent);
            }
            else
            {
                EditorGUILayout.HelpBox("No enemy entries to preview.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Now draw the enemyEntries list below the preview
            EditorGUILayout.PropertyField(entriesProp, includeChildren: true);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(Rect rect, int gridExtent, float maxExtent)
        {
            Color old = GUI.color;

            // Fine grid lines
            GUI.color = new Color(.7f, .7f, .7f, 1f);
            for (int x = -gridExtent; x <= gridExtent; x++)
            {
                float u = (x + maxExtent) / (2f * maxExtent);
                float xPx = Mathf.Lerp(rect.xMin, rect.xMax, u);
                GUI.DrawTexture(new Rect(xPx, rect.yMin, 1f, rect.height), _pixelTex);
            }
            for (int y = -gridExtent; y <= gridExtent; y++)
            {
                float v = (y + maxExtent) / (2f * maxExtent);
                float yPx = Mathf.Lerp(rect.yMax, rect.yMin, v);
                GUI.DrawTexture(new Rect(rect.xMin, yPx, rect.width, 1f), _pixelTex);
            }

            // Axes
            GUI.color = Color.black;
            float u0 = (0 + maxExtent) / (2f * maxExtent);
            float x0 = Mathf.Lerp(rect.xMin, rect.xMax, u0);
            GUI.DrawTexture(new Rect(x0 - 1f, rect.yMin, 2f, rect.height), _pixelTex);
            float v0 = u0; // symmetric for Y
            float y0 = Mathf.Lerp(rect.yMax, rect.yMin, v0);
            GUI.DrawTexture(new Rect(rect.xMin, y0 - 1f, rect.width, 2f), _pixelTex);

            GUI.color = old;
        }

        private void HandleMouse(Rect previewRect, LevelData data, float maxExtent)
        {
            var entries = data.enemyEntries;
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // Right-click deletes
            if (evt.type == EventType.MouseDown && evt.button == 1 && previewRect.Contains(evt.mousePosition))
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (GetDotRect(previewRect, entries[i].position, maxExtent)
                        .Contains(evt.mousePosition))
                    {
                        Undo.RecordObject(data, "Delete Spawn Point");
                        entries.RemoveAt(i);
                        EditorUtility.SetDirty(data);
                        evt.Use();
                        return;
                    }
                }
            }
            // Left-click: drag or add
            if (evt.type == EventType.MouseDown && evt.button == 0 && previewRect.Contains(evt.mousePosition))
            {
                bool hit = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (GetDotRect(previewRect, entries[i].position, maxExtent)
                        .Contains(evt.mousePosition))
                    {
                        _dragIndex = i;
                        GUIUtility.hotControl = controlID;
                        evt.Use();
                        hit = true;
                        break;
                    }
                }
                if (!hit)
                {
                    Vector2 local = evt.mousePosition - 
                                    new Vector2(previewRect.xMin, previewRect.yMin);
                    float u = Mathf.Clamp01(local.x / previewRect.width);
                    float v = Mathf.Clamp01(1f - local.y / previewRect.height);
                    float wx = Mathf.Lerp(-maxExtent, maxExtent, u);
                    float wy = Mathf.Lerp(-maxExtent, maxExtent, v);
                    // snap to integer
                    wx = Mathf.Round(wx);
                    wy = Mathf.Round(wy);

                    var last = entries[entries.Count - 1];
                    var ne = new EnemyEntry {
                        enemyPrefab = last.enemyPrefab,
                        enemyStats  = last.enemyStats,
                        position    = new Vector2(wx, wy)
                    };
                    Undo.RecordObject(data, "Add Spawn Point");
                    entries.Add(ne);
                    EditorUtility.SetDirty(data);
                    Repaint();
                    evt.Use();
                }
            }
            // Dragging
            else if (evt.type == EventType.MouseDrag && _dragIndex >= 0)
            {
                Vector2 local = evt.mousePosition - 
                                new Vector2(previewRect.xMin, previewRect.yMin);
                float u = Mathf.Clamp01(local.x / previewRect.width);
                float v = Mathf.Clamp01(1f - local.y / previewRect.height);
                float wx = Mathf.Lerp(-maxExtent, maxExtent, u);
                float wy = Mathf.Lerp(-maxExtent, maxExtent, v);
                if (evt.shift)
                {
                    wx = Mathf.Round(wx);
                    wy = Mathf.Round(wy);
                }
                Undo.RecordObject(data, "Move Spawn Point");
                data.enemyEntries[_dragIndex].position = new Vector2(wx, wy);
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
        }

        private void DrawDots(Rect previewRect, LevelData data, float maxExtent)
        {
            Event evt = Event.current;
            for (int i = 0; i < data.enemyEntries.Count; i++)
            {
                Rect r = GetDotRect(previewRect, data.enemyEntries[i].position, maxExtent);
                Color old = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(r, _pixelTex);
                GUI.color = old;
                if (evt.type == EventType.Repaint && r.Contains(evt.mousePosition))
                    EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
            }
        }

        private Rect GetDotRect(Rect previewRect, Vector2 pos, float maxExtent)
        {
            float u = (pos.x + maxExtent) / (2f * maxExtent);
            float v = (pos.y + maxExtent) / (2f * maxExtent);
            float xP = Mathf.Lerp(previewRect.xMin, previewRect.xMax, u) - DotSize/2f;
            float yP = Mathf.Lerp(previewRect.yMax, previewRect.yMin, v) - DotSize/2f;
            return new Rect(xP, yP, DotSize, DotSize);
        }
    }
}
