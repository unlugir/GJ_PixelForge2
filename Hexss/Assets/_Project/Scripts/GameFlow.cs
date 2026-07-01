using System;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class GameFlow : MonoBehaviour
{
    public bool isPlaying => _isPlaying;
    public bool teamATurn { get; private set; }
    
    [SerializeField] private Transform cursor;
    private Transform _actorsParent;
    
    [Inject] private PlayerController _playerController;
    [Inject] private WorldGrid _worldGrid;
    [Inject] private GridDrawer _gridDrawer;
    [Inject] private GameSettings _gameSettings;
    [Inject] private IObjectResolver _resolver;
    [Inject] private CameraController _cameraController;
    [Inject] private UIController _uiController;
    private bool _isPlaying;
    private int _movesCounter;
    private float _turnTimer;
    private void Awake()
    {
        _isPlaying = true;
        StopGame();
    }

    public void SetTeam(bool teamA)
    {
        _playerController.teamA = teamA;
    }

    public bool IsMyTurn()
    {
        return _playerController.teamA == teamATurn;
    }

    public bool IsTeamA()
    {
        return _playerController.teamA;
    }
    
    public void BeginGame()
    {
        if (_isPlaying) return;
        _playerController.enabled = true;
        _playerController.onCellHovered.AddListener(OnCellHovered);
        _playerController.onActorDropped.AddListener(OnActorDropped);
        _playerController.onActorDragged.AddListener(OnActorDragged);
        
        _worldGrid.LoadBakedGrid();
        InitializeActors();
        _isPlaying = true;
        _uiController.ShowHud();
        teamATurn = false;
        EndTurn();
    }

    public void EndTurn()
    {
        teamATurn = !teamATurn;
        _movesCounter = _gameSettings.movesPerTurn;
        _cameraController.ToggleTeamCamera(teamATurn);
        _turnTimer = _gameSettings.turnTime;
    }
    
    public void StopGame()
    {
        if (!_isPlaying) return;
        _playerController.enabled = false;
        _playerController.onCellHovered.RemoveListener(OnCellHovered);
        _playerController.onActorDropped.RemoveListener(OnActorDropped);
        _playerController.onActorDragged.RemoveListener(OnActorDragged);
        ClearActors();
        _worldGrid.Clear();
        _isPlaying = false;
        _cameraController.ToggleCamera(CameraType.Menu);
        _uiController.ShowMenu();   
    }
    
    private void InitializeActors()
    {
        ClearActors();
        _actorsParent = new GameObject("Actors").transform;
        for (int team = 0; team < 2; team++)
        {
            bool teamA = team == 0;
            foreach (var actorSpawnData in _gameSettings.deck)
            {
                var cell = actorSpawnData.spawnPoint;
                if (team == 1)
                {
                    cell = _worldGrid.gridSize - cell;
                }
                var actorPrefab = actorSpawnData.actor;
                var actor = _resolver.Instantiate(actorPrefab, _actorsParent);
                actor.SetTeam(teamA);
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
        if (actor.teamA != teamATurn) return;
        
        //REPLACE WITH A NETWORKED CALL.
        var initialActorPosition = _worldGrid.GetActorPosition(actor);
        var pattern = actor.GetMovementPattern();
        var validMovementCells = pattern.GetValidCellsForOrigin(initialActorPosition);
        if (!validMovementCells.Contains(cell.gridPosition)) return;
        if (!_worldGrid.TrySetActorPosition(actor, cell.gridPosition)) return;
        
        actor.UseSkill();
        actor.UseAttack();
        _movesCounter--;
        
        //the client, who currently makes a move should be resposible for calling an end on his side
        // && CURRENT TEAM TURN == MY TEAM
        if (_movesCounter == 0)
            EndTurn();
    }

    private void Update()
    {
        if (_isPlaying) UpdateGame();
    }

    private void UpdateGame()
    {
        UpdateHud();
        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0)
        {
            
            //the client, who currently makes a move should be resposible for calling an end on his side
            // && CURRENT TEAM TURN == MY TEAM
            //REPLACE WITH A NETWORKED CALL
            EndTurn();
        }
    }
    //its a gamejam, relax
    private void UpdateHud()
    {
        string turnString = IsMyTurn() ? $"You have {_movesCounter} moves." : $"Enemy has {_movesCounter} moves.";
        _uiController.hud.turnText.text = turnString;
        _uiController.hud.turnTimer.text = $"{_turnTimer:0.00}";
    }
}
