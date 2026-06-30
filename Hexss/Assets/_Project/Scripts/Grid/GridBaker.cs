using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Grid))]
public class GridBaker : MonoBehaviour
{
    [SerializeField] private BakedGrid bakedGrid;
    [SerializeField] private Vector2Int gridSize;
    
    [Header("Considerations")]
    [SerializeField] private bool considerGroundHeight;
    [SerializeField] private bool considerObstacle;
    [SerializeField] private float groundSpherecastRadius = 0.25f;
    [SerializeField] private float obstacleSpherecastRadius = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Gizmo")]
    [SerializeField] private bool drawSceneGizmo;
    [SerializeField] private bool drawBakedGizmo;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    #if UNITY_EDITOR
    [ContextMenu("Bake")]
    private void Bake()
    {
        if (bakedGrid == null)
        {
            Debug.LogError("BakedGrid is null");
            return;
        }
        grid ??= GetComponent<Grid>();
        List<BakedCell> cells = new();
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                bool isObstructed = false;
                var originalCellPosition = grid.CellToWorld(new Vector3Int(x, y, 0));
                var cellPosition = originalCellPosition;
                if (considerGroundHeight)
                {
                    if (Physics.SphereCast(
                            originalCellPosition + Vector3.up * 10,
                            groundSpherecastRadius,
                            Vector3.down, 
                            out RaycastHit hit, 
                            1000, 
                            groundLayer))
                    {
                        cellPosition.y = hit.point.y;
                    }
                }

                if (considerObstacle)
                {
                    if (Physics.SphereCast(
                            originalCellPosition+  Vector3.up * 10, 
                            obstacleSpherecastRadius, 
                            Vector3.down, 
                            out RaycastHit hit, 
                            1000, 
                            obstacleLayer))
                    {
                        isObstructed = true;
                    }
                }

                cells.Add(new BakedCell()
                {
                    gridPosition = new Vector2Int(x, y),
                    worldPosition = cellPosition,
                    color = Color.white,
                    isObstructed = isObstructed
                });
            }
        }
        bakedGrid.SetCells(cells.ToList(), gridSize);
        Debug.Log($"Baked {bakedGrid.Cells.Count} cells");
            
        UnityEditor.Undo.RecordObject(bakedGrid, "Bake Grid");
        UnityEditor.EditorUtility.SetDirty(bakedGrid);
        UnityEditor.AssetDatabase.SaveAssets();
    }
    
    #endif
    
    private void OnDrawGizmos()
    {
        grid ??= GetComponent<Grid>();

        if (drawBakedGizmo)
        {
            if (bakedGrid == null) return;
            for (int x = 0; x < bakedGrid.GridSize.x; x++)
            {
                for (int y = 0; y < bakedGrid.GridSize.y; y++)
                {
                    var cell = bakedGrid.Get(new Vector2Int(x, y));
                    Gizmos.color = cell.isObstructed ? Color.red : Color.green;
                    Gizmos.DrawSphere(cell.worldPosition, 0.1f); 
                    Gizmos.DrawWireMesh(mesh, cell.worldPosition);
                }
            }
        }
        
        if (drawSceneGizmo)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    bool discardCell = false;
                    var originalCellPosition = grid.CellToWorld(new Vector3Int(x, y, 0));
                    var cellPosition = originalCellPosition;
                    if (considerGroundHeight)
                    {
                        if (Physics.SphereCast(
                                originalCellPosition + Vector3.up * 10,
                                groundSpherecastRadius,
                                Vector3.down, 
                                out RaycastHit hit, 
                                1000, 
                                groundLayer))
                        {
                            cellPosition.y = hit.point.y;
                        }
                    }

                    if (considerObstacle)
                    {
                        if (Physics.SphereCast(
                                originalCellPosition+  Vector3.up * 10, 
                                obstacleSpherecastRadius, 
                                Vector3.down, 
                                out RaycastHit hit, 
                                1000, 
                                obstacleLayer))
                        {
                            discardCell = true;
                        }
                    }
                    if (!discardCell)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(cellPosition, 0.1f); 
                        Gizmos.DrawWireMesh(mesh,cellPosition);
                    }
                }
            }
        }
    }
}
