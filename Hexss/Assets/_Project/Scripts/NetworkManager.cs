using System.Linq;
using Coherence.Connection;
using Coherence.Toolkit;
using UnityEngine;
using VContainer;

[DefaultExecutionOrder(-100)]
public class NetworkManager : MonoBehaviour
{
    [SerializeField] private CoherenceBridge bridge;
    [SerializeField] private int requiredPlayers = 2;
    [SerializeField] private bool logStatus = true;
    
    [Inject] private GameFlow _gameFlow;
    public int connectedPlayerCount { get; private set; }
    public bool isMatchInProgress { get; private set; }

    private bool _clientConnectionEventsRegistered;
    

    private void OnEnable()
    {
        if (bridge == null)
        {
            Debug.LogError($"{nameof(NetworkManager)} needs a {nameof(CoherenceBridge)} in the scene.", this);
            return;
        }

        bridge.onConnected.AddListener(OnBridgeConnected);
        bridge.onLiveQuerySynced.AddListener(OnLiveQuerySynced);
        bridge.onDisconnected.AddListener(OnBridgeDisconnected);

        if (bridge.IsConnected)
        {
            RegisterClientConnectionEvents();
            EvaluateMatchReadiness();
        }
    }

    private void Start()
    {
        EvaluateMatchReadiness();
    }

    private void OnDisable()
    {
        if (bridge != null)
        {
            bridge.onConnected.RemoveListener(OnBridgeConnected);
            bridge.onLiveQuerySynced.RemoveListener(OnLiveQuerySynced);
            bridge.onDisconnected.RemoveListener(OnBridgeDisconnected);
        }

        UnregisterClientConnectionEvents();
    }

    private void OnBridgeConnected(CoherenceBridge _)
    {
        RegisterClientConnectionEvents();
        EvaluateMatchReadiness();
    }

    private void OnLiveQuerySynced(CoherenceBridge _)
    {
        RegisterClientConnectionEvents();
        EvaluateMatchReadiness();
    }

    private void OnBridgeDisconnected(CoherenceBridge _, ConnectionCloseReason __)
    {
        connectedPlayerCount = 0;
        isMatchInProgress = false;
        _gameFlow.StopGame();
        UnregisterClientConnectionEvents();
    }

    private void OnClientConnectionCreated(CoherenceClientConnection _)
    {
        EvaluateMatchReadiness();
    }

    private void OnClientConnectionDestroyed(CoherenceClientConnection _)
    {
        EvaluateMatchReadiness();
    }

    private void OnClientConnectionsSynced(CoherenceClientConnectionManager _)
    {
        EvaluateMatchReadiness();
    }


    private void RegisterClientConnectionEvents()
    {
        if (_clientConnectionEventsRegistered || bridge?.ClientConnections == null)
        {
            return;
        }

        if (!bridge.EnableClientConnections)
        {
            Debug.LogWarning($"{nameof(NetworkManager)} needs {nameof(CoherenceBridge)} client connections enabled to count room players.", this);
        }

        bridge.ClientConnections.OnCreated += OnClientConnectionCreated;
        bridge.ClientConnections.OnDestroyed += OnClientConnectionDestroyed;
        bridge.ClientConnections.OnSynced += OnClientConnectionsSynced;
        _clientConnectionEventsRegistered = true;
    }

    private void UnregisterClientConnectionEvents()
    {
        if (!_clientConnectionEventsRegistered || bridge?.ClientConnections == null)
        {
            _clientConnectionEventsRegistered = false;
            return;
        }

        bridge.ClientConnections.OnCreated -= OnClientConnectionCreated;
        bridge.ClientConnections.OnDestroyed -= OnClientConnectionDestroyed;
        bridge.ClientConnections.OnSynced -= OnClientConnectionsSynced;
        _clientConnectionEventsRegistered = false;
    }

    private void EvaluateMatchReadiness()
    {
        if (bridge?.ClientConnections == null)
        {
            _gameFlow.StopGame();
            return;
        }

        connectedPlayerCount = bridge.ClientConnections.GetAllClients().Count();

        if (isMatchInProgress && connectedPlayerCount < requiredPlayers)
        {
            _gameFlow.StopGame();
            bridge.Disconnect();
            return;
        }
        
        if (isMatchInProgress || connectedPlayerCount < requiredPlayers)
        {
            LogWaitingStatus();
            return;
        }

        isMatchInProgress = true;
        AssignLocalTeam();
        _gameFlow.BeginGame();

        if (logStatus)
        {
            Debug.Log($"Match ready: {connectedPlayerCount}/{requiredPlayers} players joined. Local team: {_gameFlow.IsTeamA()}", this);
        }
    }

    private void AssignLocalTeam()
    {
        var myConnection = bridge.ClientConnections.GetMine();
        if (myConnection == null)
        {
            return;
        }

        var clients = bridge.ClientConnections.GetAllClients()
            .OrderBy(connection => connection.ClientId)
            .ToList();

        
        int localIndex = clients.FindIndex(connection => connection.ClientId == myConnection.ClientId);
        if (clients.Count != requiredPlayers || localIndex < 0)
        {
            Debug.LogError($"Expected {requiredPlayers} players in a match, but found {clients.Count}.", this);
            bridge.Disconnect();
        }

        bool isTeamA = localIndex == 0;
        _gameFlow.SetTeam(isTeamA);
        
    }

    private void LogWaitingStatus()
    {
        if (!logStatus || isMatchInProgress)
        {
            return;
        }

        Debug.Log($"Waiting for players: {connectedPlayerCount}/{requiredPlayers}", this);
    }
}
