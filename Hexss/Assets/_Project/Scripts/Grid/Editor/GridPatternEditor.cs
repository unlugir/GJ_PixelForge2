#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridPattern))]
public class GridPatternEditor : Editor
{
    private const int PreviewRadius = 5;

    private const float HexSize = 22f;
    private const float Padding = 40f;

    private SerializedProperty cellsProperty;
    private HashSet<Vector2Int> selectedCells;

    private static readonly Color SelectedColor = new Color(0.2f, 0.85f, 0.25f, 0.9f);
    private static readonly Color UnselectedColor = new Color(0.9f, 0.2f, 0.2f, 0.65f);
    private static readonly Color OutlineColor = new Color(0f, 0f, 0f, 0.8f);

    private void OnEnable()
    {
        cellsProperty = serializedObject.FindProperty("cells");
        RebuildSelectedSet();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Grid Pattern", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Cells are stored as axial hex offsets: x = q, y = r. Runtime converts them to your Unity Odd-R grid around the origin.",
            MessageType.Info
        );

        EditorGUILayout.Space(6);

        DrawToolbar();

        EditorGUILayout.Space(10);

        DrawHexGrid();

        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(cellsProperty, includeChildren: true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear"))
            ClearCells();

        if (GUILayout.Button("Fill Radius 1"))
            FillRadius(1);

        if (GUILayout.Button("Fill Radius 2"))
            FillRadius(2);

        if (GUILayout.Button("Fill Radius 3"))
            FillRadius(3);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Fill Radius 5"))
            FillRadius(5);

        if (GUILayout.Button("Add Center"))
            AddCellIfMissing(Vector2Int.zero);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawHexGrid()
    {
        float hexWidth = HexSize * Mathf.Sqrt(3f);
        float hexHeight = HexSize * 2f;

        float gridWidth = (PreviewRadius * 2 + 4) * hexWidth + Padding * 2f;
        float gridHeight = (PreviewRadius * 2 + 4) * hexHeight * 0.75f + Padding * 2f;

        Rect rect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        Vector2 center = rect.center;

        Handles.BeginGUI();

        foreach (Vector2Int axialCell in GetAxialHexCells(PreviewRadius))
        {
            Vector2 position = AxialToPixel(axialCell);
            position += center;

            bool isSelected = selectedCells.Contains(axialCell);

            DrawHex(
                position,
                HexSize,
                isSelected ? SelectedColor : UnselectedColor,
                OutlineColor
            );

            DrawCellLabel(position, axialCell);

            Rect buttonRect = new Rect(
                position.x - HexSize,
                position.y - HexSize,
                HexSize * 2f,
                HexSize * 2f
            );

            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                ToggleCell(axialCell);
                Event.current.Use();
            }
        }

        Handles.EndGUI();
    }

    private void ToggleCell(Vector2Int axialCell)
    {
        serializedObject.Update();

        Undo.RecordObject(target, "Toggle Grid Pattern Cell");

        RebuildSelectedSet();

        if (selectedCells.Contains(axialCell))
            RemoveCell(axialCell);
        else
            AddCell(axialCell);

        serializedObject.ApplyModifiedProperties();

        RebuildSelectedSet();

        EditorUtility.SetDirty(target);
        Repaint();
    }

    private void AddCellIfMissing(Vector2Int axialCell)
    {
        serializedObject.Update();

        Undo.RecordObject(target, "Add Grid Pattern Cell");

        RebuildSelectedSet();

        if (!selectedCells.Contains(axialCell))
            AddCell(axialCell);

        serializedObject.ApplyModifiedProperties();

        RebuildSelectedSet();

        EditorUtility.SetDirty(target);
        Repaint();
    }

    private void ClearCells()
    {
        serializedObject.Update();

        Undo.RecordObject(target, "Clear Grid Pattern Cells");

        cellsProperty.ClearArray();

        serializedObject.ApplyModifiedProperties();

        RebuildSelectedSet();

        EditorUtility.SetDirty(target);
        Repaint();
    }

    private void FillRadius(int radius)
    {
        serializedObject.Update();

        Undo.RecordObject(target, $"Fill Grid Pattern Radius {radius}");

        cellsProperty.ClearArray();

        foreach (Vector2Int axialCell in GetAxialHexCells(radius))
            AddCell(axialCell);

        serializedObject.ApplyModifiedProperties();

        RebuildSelectedSet();

        EditorUtility.SetDirty(target);
        Repaint();
    }

    private void AddCell(Vector2Int axialCell)
    {
        int index = cellsProperty.arraySize;
        cellsProperty.InsertArrayElementAtIndex(index);

        SerializedProperty element = cellsProperty.GetArrayElementAtIndex(index);
        element.vector2IntValue = axialCell;
    }

    private void RemoveCell(Vector2Int axialCell)
    {
        for (int i = cellsProperty.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty element = cellsProperty.GetArrayElementAtIndex(i);

            if (element.vector2IntValue == axialCell)
            {
                cellsProperty.DeleteArrayElementAtIndex(i);
                return;
            }
        }
    }

    private void RebuildSelectedSet()
    {
        selectedCells = new HashSet<Vector2Int>();

        if (cellsProperty == null)
            return;

        for (int i = 0; i < cellsProperty.arraySize; i++)
            selectedCells.Add(cellsProperty.GetArrayElementAtIndex(i).vector2IntValue);
    }

    private static IEnumerable<Vector2Int> GetAxialHexCells(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int rMin = Mathf.Max(-radius, -q - radius);
            int rMax = Mathf.Min(radius, -q + radius);

            for (int r = rMin; r <= rMax; r++)
                yield return new Vector2Int(q, r);
        }
    }

    private static Vector2 AxialToPixel(Vector2Int axial)
    {
        float x = HexSize * Mathf.Sqrt(3f) * (axial.x + axial.y * 0.5f);
        float y = HexSize * 1.5f * axial.y;

        return new Vector2(x, y);
    }

    private static void DrawHex(Vector2 center, float size, Color fillColor, Color outlineColor)
    {
        Vector3[] points = new Vector3[6];

        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60f * i - 30f;
            float angleRad = Mathf.Deg2Rad * angleDeg;

            points[i] = new Vector3(
                center.x + size * Mathf.Cos(angleRad),
                center.y + size * Mathf.Sin(angleRad),
                0f
            );
        }

        Handles.color = fillColor;
        Handles.DrawAAConvexPolygon(points);

        Handles.color = outlineColor;
        Handles.DrawAAPolyLine(
            2f,
            points[0],
            points[1],
            points[2],
            points[3],
            points[4],
            points[5],
            points[0]
        );
    }

    private static void DrawCellLabel(Vector2 center, Vector2Int axialCell)
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9,
            normal =
            {
                textColor = Color.white
            }
        };

        Rect labelRect = new Rect(
            center.x - 24f,
            center.y - 8f,
            48f,
            16f
        );

        GUI.Label(labelRect, $"{axialCell.x},{axialCell.y}", style);
    }
}
#endif