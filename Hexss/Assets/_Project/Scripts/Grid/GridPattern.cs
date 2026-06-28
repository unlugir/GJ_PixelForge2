using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridPattern", menuName = "Game/Grid Pattern")]
public class GridPattern : ScriptableObject
{
    public IReadOnlyCollection<Vector2Int> Cells => cells;
    [SerializeField] private Vector2Int[] cells;

    public List<Vector2Int> GetValidCellsForOrigin(Vector2Int origin)
    {
        List<Vector2Int> validCells = new List<Vector2Int>();
        Vector2Int originAxial = OffsetOddRToAxial(origin);

        foreach (Vector2Int axialOffset in this.Cells)
        {
            Vector2Int targetAxial = originAxial + axialOffset;
            Vector2Int targetCell = AxialToOffsetOddR(targetAxial);
            validCells.Add(targetCell);
        }

        return validCells;
    }
    private static Vector2Int OffsetOddRToAxial(Vector2Int cell)
    {
        int col = cell.x;
        int row = cell.y;

        int q = col - (row - (row & 1)) / 2;
        int r = row;

        return new Vector2Int(q, r);
    }

    private static Vector2Int AxialToOffsetOddR(Vector2Int axial)
    {
        int q = axial.x;
        int r = axial.y;

        int col = q + (r - (r & 1)) / 2;
        int row = r;

        return new Vector2Int(col, row);
    }
    
}
