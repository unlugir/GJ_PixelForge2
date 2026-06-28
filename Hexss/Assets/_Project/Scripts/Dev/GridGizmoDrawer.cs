using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Grid))]
public class GridGizmoDrawer : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Vector2Int gridSize;

    
    [SerializeField] private bool considerGroundHeight;
    [SerializeField] private bool considerObstacle;
    [SerializeField] private float obstacleSpherecastRadius = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Grid _grid;

    private void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    private void OnDrawGizmos()
    {
        _grid ??= GetComponent<Grid>();
     
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                bool discardCell = false;
                var originalCellPosition = this.transform.position + _grid.CellToWorld(new Vector3Int(x, y, 0));
                var cellPosition = originalCellPosition;
                if (considerGroundHeight)
                {
                    if (Physics.Raycast(
                            originalCellPosition + Vector3.up * 10, 
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
                    Gizmos.DrawSphere(cellPosition, 0.1f); 
                    Gizmos.DrawWireMesh(mesh,cellPosition);
                }
            }
        }
    }
}
