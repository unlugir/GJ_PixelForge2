using Coherence;
using Coherence.Toolkit;
using UnityEngine;

[RequireComponent(typeof(CoherenceSync))]
public class PlayerCommands : MonoBehaviour
{
    private CoherenceSync _coherenceSync;
    private GameFlow _gameFlow;

    public bool isLocalPlayer => _coherenceSync == null || _coherenceSync.HasInputAuthority;

    private void Awake()
    {
        _coherenceSync = GetComponent<CoherenceSync>();
    }

    public void RequestActorMove(int actorId, Vector2Int targetPosition)
    {
        if (!isLocalPlayer) return;

        SendOrApply(
            nameof(ApplyActorMoveCommand),
            actorId,
            targetPosition.x,
            targetPosition.y);
    }

    public void RequestTurnEnd()
    {
        if (!isLocalPlayer) return;

        if (_coherenceSync == null)
        {
            ApplyTurnEndCommand();
            return;
        }

        _coherenceSync.SendOrderedCommand<PlayerCommands>(
            nameof(ApplyTurnEndCommand),
            MessageTarget.All);
    }

    [Command(defaultRouting = MessageTarget.All)]
    public void ApplyActorMoveCommand(int actorId, int targetX, int targetY)
    {
        var gameFlow = GetGameFlow();
        if (gameFlow == null) return;

        gameFlow.ApplyActorMove(actorId, targetX, targetY);
    }

    [Command(defaultRouting = MessageTarget.All)]
    public void ApplyTurnEndCommand()
    {
        var gameFlow = GetGameFlow();
        if (gameFlow == null) return;

        gameFlow.ApplyTurnEnd();
    }

    private void SendOrApply(string commandName, int actorId, int targetX, int targetY)
    {
        if (_coherenceSync == null)
        {
            ApplyActorMoveCommand(actorId, targetX, targetY);
            return;
        }

        _coherenceSync.SendOrderedCommand<PlayerCommands>(
            commandName,
            MessageTarget.All,
            actorId,
            targetX,
            targetY);
    }

    private GameFlow GetGameFlow()
    {
        if (_gameFlow != null) return _gameFlow;

        _gameFlow = FindAnyObjectByType<GameFlow>();
        return _gameFlow;
    }
}
