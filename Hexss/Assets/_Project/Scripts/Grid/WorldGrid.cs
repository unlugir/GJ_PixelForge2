using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class WorldCell
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;

    public WorldCell()
    {
        gridPosition = Vector2Int.zero;
        worldPosition = Vector3.zero;
    }

    public WorldCell(BakedCell bakedCell)
    {
        gridPosition = bakedCell.gridPosition;
        worldPosition = bakedCell.worldPosition;
    }
}
public class WorldGrid : MonoBehaviour
{
    public Vector2Int gridSize => _gridSize;
    
    [SerializeField] private BakedGrid bakedGrid;
    private Dictionary<Vector2Int, GridActor> _gridActors = new Dictionary<Vector2Int, GridActor>();
    private Dictionary<Vector2Int, WorldCell> _cells = new Dictionary<Vector2Int, WorldCell>();
    private Vector2Int _gridSize;
    

    public void RegisterActor(GridActor actor)
    {
        var closestCell = GetFreeCellAtWorldPosition(actor.transform.position);
        if (closestCell == null)
        {
            Debug.LogError("No free cell found, destroying actor");
            Destroy(actor.gameObject);
            return;
        }
        actor.transform.position = closestCell.worldPosition;
        _gridActors[closestCell.gridPosition] = actor;
        actor.id = _gridActors.Count + 1;
    }

    public GridActor GetActorById(int id)
    {
        return _gridActors.Values.FirstOrDefault(a => a.id == id);
    }
    public bool TrySetActorPosition(GridActor actor, Vector2Int position)
    {
        if (_gridActors.ContainsKey(position)) return false;
        
        var oldPosition = GetActorPosition(actor);
        _gridActors[position] = actor;
        _gridActors.Remove(oldPosition);
        //actor.transform.position = _cells[position].worldPosition;
        
        actor.transform.DOMove(_cells[position].worldPosition, 0.5f).SetEase(Ease.InOutBack);
        return true;
    }
    
    public Vector2Int GetActorPosition(GridActor actor)
    {
        var pair = _gridActors.FirstOrDefault(a => a.Value == actor);
        return pair.Key;
    }

    public GridActor GetActorAtPosition(Vector2Int position)
    {
        return _gridActors.GetValueOrDefault(position);
    }
    
    //TODO: improve
    public WorldCell GetCellAtWorldPosition(Vector3 worldPosition)
    {
        return _cells.Values
            .OrderBy(cell => Vector3.Distance(cell.worldPosition, worldPosition))
            .FirstOrDefault();
    }

    public WorldCell GetFreeCellAtWorldPosition(Vector3 worldPosition)
    {
        return _cells.Values
            .OrderBy(c => Vector3.Distance(c.worldPosition, worldPosition))
            .FirstOrDefault(c => !_gridActors.ContainsKey(c.gridPosition));
    }

    public bool TryGetCell(Vector2Int gridPosition, out WorldCell cell)
    {
        return _cells.TryGetValue(gridPosition, out cell);
    }
    
    public void LoadBakedGrid()
    {
        if (bakedGrid == null) return;
        Clear();
        _gridSize = bakedGrid.GridSize;
        for (int x = 0; x < _gridSize.x; x++)
            for (int y = 0; y < _gridSize.y; y++)
            {
                if (bakedGrid.Get(new Vector2Int(x, y)).isObstructed) continue;
                _cells[new Vector2Int(x, y)] = new WorldCell(bakedGrid.Get(new Vector2Int(x, y)));
            }
        
    }
    public void Clear()
    {
        _cells.Clear();
        _gridActors.Clear();
    }
}
