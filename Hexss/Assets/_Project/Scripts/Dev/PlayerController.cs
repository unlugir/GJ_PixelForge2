using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerController : MonoBehaviour
{
    public UnityEvent<GridActor, WorldCell> onActorDragged = new UnityEvent<GridActor, WorldCell>();
    public UnityEvent<GridActor, WorldCell> onActorDropped = new UnityEvent<GridActor, WorldCell>();
    public UnityEvent<GridActor> onActorHovered = new UnityEvent<GridActor>();
    public UnityEvent<GridActor> onActorUnhovered = new UnityEvent<GridActor>();
    public UnityEvent<WorldCell> onCellHovered = new UnityEvent<WorldCell>();
    public UnityEvent<GridActor> onActorClicked = new UnityEvent<GridActor>();
    
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private InputActionReference click;
    
    [Inject] private WorldGrid _worldGrid;
        
    private WorldCell _hoveredCell;
    private WorldCell _draggedToCell;
    private GridActor _hoveredActor;
    private GridActor _draggedActor;
    
    private bool _isDragging;
    private void OnEnable()
    {
        click.action.started += OnClickStarted;
        click.action.canceled += OnClickEnded;
        click.action.performed += OnClickPerformed;
    }

    private void OnClickStarted(InputAction.CallbackContext obj)
    {
        _draggedActor = _hoveredActor;
        _draggedActor?.ToggleCollider(false);
        _isDragging = true;
    }

    private void OnClickEnded(InputAction.CallbackContext obj)
    {
        if (_draggedActor != null && _hoveredCell != null)
            onActorDropped.Invoke(_draggedActor, _hoveredCell);
        
        _draggedActor?.ToggleCollider(true);
        _draggedActor = null;
        _isDragging = false;
    }

    private void OnClickPerformed(InputAction.CallbackContext obj)
    {
        
    }
    private void Update()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform.gameObject.TryGetComponent<GridActor>(out var actor))
            {
                var position = _worldGrid.GetActorPosition(actor);
                if (!_worldGrid.TryGetCell(position, out var cell)) return;
                
                bool newActorHovered = actor != _hoveredActor;
                bool newCellHovered = cell != _hoveredCell;
                _hoveredCell = cell;
                _hoveredActor = actor;
                
                if (newCellHovered)
                    onCellHovered.Invoke(_hoveredCell);
                if (newActorHovered)
                    onActorHovered.Invoke(_hoveredActor);
            }
            else
            {
                var cell = _worldGrid.GetCellAtWorldPosition(hit.point);
                
                var actorInCell= _worldGrid.GetActorAtPosition(cell.gridPosition);
                bool newActorHovered = actorInCell != _hoveredActor && actorInCell != null;
                bool actorUnhovered = _hoveredActor != null && actorInCell == null;
                bool newCellHovered = cell != _hoveredCell;
                
                _hoveredActor = actorInCell;
                _hoveredCell = cell;
               
                if (newCellHovered)
                    onCellHovered.Invoke(_hoveredCell);
                if (newActorHovered)
                    onActorHovered.Invoke(_hoveredActor);
                if (actorUnhovered)
                    onActorUnhovered.Invoke(_hoveredActor);
            }
        }
        else
        {
            _hoveredCell = null;
            _hoveredActor = null;
        }
        
        if (_isDragging && _draggedActor != null && _hoveredCell != null)
        {
            if (_draggedToCell != _hoveredCell)
            {
                _draggedToCell = _hoveredCell;
                onActorDragged.Invoke(_draggedActor, _hoveredCell);    
            }
        }
    }

    private void OnDisable()
    {
        click.action.started -= OnClickStarted;
        click.action.canceled -= OnClickEnded;
    }
}
