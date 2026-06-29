using System;
using Mono.Cecil;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private Transform cursor;
    private Transform _actorsParent;
    
    [Inject] private PlayerController _playerController;
    [Inject] private WorldGrid _worldGrid;
    [Inject] private GridDrawer _gridDrawer;
    [Inject] private GameSettings _gameSettings;
    [Inject] private IObjectResolver _resolver;
    
    private void Start()
    {
        _playerController.onCellHovered.AddListener(OnCellHovered);
        _playerController.onActorDropped.AddListener(OnActorDropped);
        _playerController.onActorDragged.AddListener(OnActorDragged);
        InitializeActors();
    }

    private void InitializeActors()
    {
        ClearActors();
        _actorsParent = new GameObject("Actors").transform;
        for (int team = 0; team < 2; team++)
        {
            foreach (var actorSpawnData in _gameSettings.deck)
            {
                var cell = actorSpawnData.spawnPoint;
                if (team == 1)
                {
                    cell = _worldGrid.gridSize - cell;
                }
                var actorPrefab = actorSpawnData.actor;
                var actor = _resolver.Instantiate(actorPrefab, _actorsParent);
                actor.team = team + 1;
                _worldGrid.RegisterActor(actor);
                _worldGrid.TrySetActorPosition(actor, cell);
            }
        }
    }

    private void ClearActors()
    {
        if (_actorsParent == null) return;
        Destroy(_actorsParent.gameObject);
    }
    private void OnCellHovered(WorldCell cell)
    {
        cursor.transform.position = cell.worldPosition;
    }

    private void OnActorDragged(GridActor actor, WorldCell cell)
    {
        var actorPosition = _worldGrid.GetActorPosition(actor);
        var movement = actor.GetMovementPattern();
        
        var attack = actor.GetAttackPattern();
        var skill = actor.GetSkillPattern();
        _gridDrawer.DrawMovement(movement, actorPosition);
        if (attack != null)
            _gridDrawer.DrawAttack(attack, cell.gridPosition);
        if (skill != null)
            _gridDrawer.DrawSkill(skill, cell.gridPosition);
        
    }

    private void OnActorDropped(GridActor actor, WorldCell cell)
    {
        _gridDrawer.ClearAttack();
        _gridDrawer.ClearMovement();
        _gridDrawer.ClearSkill();
        var initialActorPosition = _worldGrid.GetActorPosition(actor);
        var pattern = actor.GetMovementPattern();
        var validMovementCells = pattern.GetValidCellsForOrigin(initialActorPosition);
        if (!validMovementCells.Contains(cell.gridPosition)) return;
        if (!_worldGrid.TrySetActorPosition(actor, cell.gridPosition)) return;
        
        actor.UseSkill();
        actor.UseAttack();
    }

    private void OnDestroy()
    {
        _playerController.onCellHovered.RemoveListener(OnCellHovered);
        _playerController.onActorDropped.RemoveListener(OnActorDropped);
        _playerController.onActorDragged.RemoveListener(OnActorDragged);
    }
}
