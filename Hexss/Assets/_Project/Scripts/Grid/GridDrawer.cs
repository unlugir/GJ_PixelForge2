using UnityEngine;
using VContainer;

public class GridDrawer : MonoBehaviour
{
    [SerializeField] private Mesh mesh;

    [Inject] private WorldGrid _worldGrid;

    private GridPattern _patternToDraw;
    private Vector2Int _patternOrigin;

    public void DrawPattern(GridPattern pattern, Vector2Int origin)
    {
        _patternToDraw = pattern;
        _patternOrigin = origin;
    }
    
    public void Clear()
    {
        _patternToDraw = null;
        _patternOrigin = Vector2Int.zero;
    }

    private void OnDrawGizmos()
    {
        if (_patternToDraw == null)
            return;

        foreach (Vector2Int offsetCell in _patternToDraw.GetValidCellsForOrigin(_patternOrigin))
        {
            if (_worldGrid.TryGetCell(offsetCell, out var cell))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawMesh(mesh, cell.worldPosition + Vector3.up * 0.25f);
            }
        }
    }

   
}