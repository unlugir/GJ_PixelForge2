using System;
using UnityEngine;
using UnityEngine.Scripting;
using VContainer;

public class GridActor : MonoBehaviour
{
    [SerializeField] private Collider collider;
    [SerializeField] private GridPattern movementPattern;
 
    [Inject] private WorldGrid _worldGrid;

    private void Awake()
    {
        if (movementPattern == null)
        {
            Debug.LogError("Movement pattern is not set");
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        _worldGrid.RegisterActor(this);
    }
    
    public void ToggleCollider(bool state)
    {
        collider.enabled = state;
    }

    public GridPattern GetMovementPattern()
    {
        return movementPattern;
    }
}
