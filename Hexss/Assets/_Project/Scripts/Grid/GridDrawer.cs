using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class GridDrawer : MonoBehaviour
{

    [SerializeField] private GameObject movementPrefab;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private GameObject skillPrefab;

    
    [Inject] private WorldGrid _worldGrid;
    
    private List<GameObject> _movementCells = new();
    private List<GameObject> _attackCells = new();
    private List<GameObject> _skillCells = new();


    public void DrawMovement(GridPattern pattern, Vector2Int origin)
    {
        Draw(_movementCells, movementPrefab, pattern, origin);
    }
    public void DrawAttack(GridPattern pattern, Vector2Int origin)
    {
        Draw(_attackCells, attackPrefab, pattern, origin);
    }

    public void DrawSkill(GridPattern pattern, Vector2Int origin)
    {
        Draw(_skillCells, skillPrefab, pattern, origin);
    }
    
    public void ClearMovement()
    {
        _movementCells.ForEach(c => c.gameObject.SetActive(false));
    }

    public void ClearAttack()
    {
        _attackCells.ForEach(c => c.gameObject.SetActive(false));
    }
    public void ClearSkill()
    {
        _skillCells.ForEach(c => c.gameObject.SetActive(false));
    }
    
    private void Draw(List<GameObject> cells, GameObject prefab, GridPattern pattern, Vector2Int origin)
    {
        if (pattern == null)
            return;
        cells.ForEach(c => c.gameObject.SetActive(false));
        
        var cellsToDraw = pattern.GetValidCellsForOrigin(origin);
        for (int i = 0; i < cellsToDraw.Count; i++)
        {
            if (!_worldGrid.TryGetCell(cellsToDraw[i], out var cell))
                continue;
            GameObject cellObject = null;
            if (cells.Count <= i)
            {
                cellObject = Instantiate(prefab, cell.worldPosition, Quaternion.identity);
                cellObject.transform.SetParent(transform);
                cells.Add(cellObject);
            }
            else
            {
                cellObject = cells[i];
            }
            cellObject.gameObject.SetActive(true);
            cellObject.transform.position = cell.worldPosition;
           
        }
    }

   
}