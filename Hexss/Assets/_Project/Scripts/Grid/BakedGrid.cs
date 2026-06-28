using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class BakedCell
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public Color color;
    public bool isObstructed;
}

[CreateAssetMenu(fileName = "BakedGrid", menuName = "Game/BakedGrid", order = 1)]
public class BakedGrid : ScriptableObject
{
  
    [SerializeField] private List<BakedCell> cells = new();
    private Dictionary<Vector2Int, BakedCell> _lookup;
    private Vector2Int _gridSize;
    public IReadOnlyList<BakedCell> Cells => cells;
    public Vector2Int GridSize => _gridSize;
    
    public BakedCell Get(Vector2Int position)
    {
        _lookup ??= BuildLookup();
        return _lookup[position];
    }
    
    public void SetCells(List<BakedCell> newCells, Vector2Int gridSize)
    {
        cells = newCells;
        _gridSize = gridSize;
        _lookup = null;
    }

    private Dictionary<Vector2Int, BakedCell> BuildLookup()
    {
        var dict = new Dictionary<Vector2Int, BakedCell>();

        foreach (var cell in cells)
            dict[cell.gridPosition] = cell;

        return dict;
    }
    
}
