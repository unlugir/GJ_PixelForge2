using Coherence;
using Coherence.Toolkit;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameFlow : MonoBehaviour
{
    public bool isPlaying => _isPlaying;
    public bool teamATurn { get; private set; }
    
    [SerializeField] private Transform cursor;
    [SerializeField] private CoherenceSync coherenceSync;
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
    private void Start()
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
        _cameraController.ResetCameras();
        _cameraController.ToggleTeamCamera(_playerController.teamA);
        EndTurn();
    }

    public void EndTurn()
    {
        teamATurn = !teamATurn;
        _movesCounter = _gameSettings.movesPerTurn;
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
        _cameraController.ResetCameras();
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
        if (!CanSendTurnAction(actor)) return;
        if (!IsValidActorMove(actor, cell.gridPosition)) return;

        SendActorMove(actor.id, cell.gridPosition);
    }

    private bool CanSendTurnAction(GridActor actor)
    {
        if (actor == null || actor.teamA != teamATurn) return false;
        return _gameSettings.allowSoloPlay || IsMyTurn();
    }

    private bool CanSendTurnEnd()
    {
        return _gameSettings.allowSoloPlay || IsMyTurn();
    }
    
    private bool IsValidActorMove(GridActor actor, Vector2Int targetPosition)
    {
        if (actor == null || actor.teamA != teamATurn) return false;

        var initialActorPosition = _worldGrid.GetActorPosition(actor);
        var pattern = actor.GetMovementPattern();
        var validMovementCells = pattern.GetValidCellsForOrigin(initialActorPosition);
        return validMovementCells.Contains(targetPosition);
    }

    private bool TryApplyActorMove(int actorId, Vector2Int targetPosition)
    {
        var actor = _worldGrid.GetActorById(actorId);
        if (!_worldGrid.TrySetActorPosition(actor, targetPosition)) return false;
        
        actor.UseSkill();
        actor.UseAttack();
        return true;
    }

    private void SendActorMove(int actorId, Vector2Int targetPosition)
    {
        if (coherenceSync == null)
        {
            ApplyActorMoveCommand(actorId, targetPosition.x, targetPosition.y);
            return;
        }

        coherenceSync.SendOrderedCommand<GameFlow>(
            nameof(ApplyActorMoveCommand),
            MessageTarget.All,
            actorId,
            targetPosition.x,
            targetPosition.y);
    }

    [Command(defaultRouting = MessageTarget.All)]
    public void ApplyActorMoveCommand(int actorId, int targetX, int targetY)
    {
        TryApplyActorMove(actorId, new Vector2Int(targetX, targetY));

        _movesCounter--;
        if (_movesCounter <= 0 && CanSendTurnEnd())
        {
            SendTurnEnd();
        }
    }

    private void SendTurnEnd()
    {
        if (coherenceSync == null)
        {
            ApplyTurnEndCommand();
            return;
        }

        coherenceSync.SendOrderedCommand<GameFlow>(nameof(ApplyTurnEndCommand), MessageTarget.All);
    }

    [Command(defaultRouting = MessageTarget.All)]
    public void ApplyTurnEndCommand()
    {
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
        if (_turnTimer <= 0 && CanSendTurnEnd())
        {
            SendTurnEnd();
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
