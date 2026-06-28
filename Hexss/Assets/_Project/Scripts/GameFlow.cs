using System;
using UnityEngine;
using VContainer;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private Transform cursor;
    
    [Inject] private PlayerController _playerController;
    [Inject] private WorldGrid _worldGrid;
    [Inject] private GridDrawer _gridDrawer;
    private void Start()
    {
        _playerController.onCellHovered.AddListener(OnCellHovered);
        _playerController.onActorDropped.AddListener(OnActorDropped);
        _playerController.onActorDragged.AddListener(OnActorDragged);
    }

    private void OnCellHovered(WorldCell cell)
    {
        cursor.transform.position = cell.worldPosition;
    }

    private void OnActorDragged(GridActor actor, WorldCell cell)
    {
        var actorPosition = _worldGrid.GetActorPosition(actor);
        var pattern = actor.GetMovementPattern();
        _gridDrawer.DrawPattern(pattern, actorPosition);
    }

    private void OnActorDropped(GridActor actor, WorldCell cell)
    {
        _gridDrawer.Clear();
        var initialActorPosition = _worldGrid.GetActorPosition(actor);
        var pattern = actor.GetMovementPattern();
        var validMovementCells = pattern.GetValidCellsForOrigin(initialActorPosition);
        if (!validMovementCells.Contains(cell.gridPosition)) return;
        _worldGrid.SetActorPosition(actor, cell.gridPosition);
    }

    private void OnDestroy()
    {
        _playerController.onCellHovered.RemoveListener(OnCellHovered);
        _playerController.onActorDropped.RemoveListener(OnActorDropped);
        _playerController.onActorDragged.RemoveListener(OnActorDragged);
    }
}
