using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ActorSpawnData
{
    public GridActor actor;
    public Vector2Int spawnPoint;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] public float roundTime;
    [SerializeField] public int maxRounds;
    [SerializeField] public List<ActorSpawnData> deck;
    [SerializeField] public int turnTime;
    [SerializeField] public int movesPerTurn;
    [SerializeField] public bool allowSoloPlay;
}
