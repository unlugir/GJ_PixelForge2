using System;
using UnityEngine;
using UnityEngine.Scripting;
using VContainer;

public class GridActor : MonoBehaviour
{
    public int team;
    public float health { get; private set; }
    public float maxHealth { get; private set; }
    
    [SerializeField] private ActorDefinition actorDefinition;
    [SerializeField] private Collider collider;
    [Inject] private WorldGrid _worldGrid;

    private void Awake()
    {
        if (actorDefinition == null)
        {
            Debug.LogError("actorDefinition is not set");
            Destroy(this.gameObject);
        }
        health = maxHealth = actorDefinition.health;
    }

    void Start()
    {
        //_worldGrid.RegisterActor(this);
    }

    public void TakeDamage(float amount)
    {
        health = MathF.Max(health - amount, 0);
    }

    public void Heal(float amount)
    {
        health = MathF.Min(health + amount, maxHealth);
    }
    public void ToggleCollider(bool state)
    {
        collider.enabled = state;
    }
    public GridPattern GetMovementPattern()
    {
        return actorDefinition.movementPattern;
    }
    public GridPattern GetAttackPattern()
    {
        return actorDefinition.attackPattern;
    }
    public GridPattern GetSkillPattern()
    {
        return actorDefinition.skillPattern;
    }

    public void UseSkill()
    {
        if (actorDefinition.skillDefinition == null || actorDefinition.skillPattern == null) return;
        var myPosition = _worldGrid.GetActorPosition(this);
        actorDefinition.skillDefinition.Cast(this, _worldGrid, actorDefinition.skillPattern, myPosition);
    }

    public void UseAttack()
    {
        if (actorDefinition.attackPattern == null) return;
        var myPosition = _worldGrid.GetActorPosition(this);
        var attackedCells = actorDefinition.attackPattern.GetValidCellsForOrigin(myPosition);
        foreach (var cell in attackedCells)
        {
            var actor = _worldGrid.GetActorAtPosition(cell);
            if (actor == null) continue;
            if (!actorDefinition.attackAllies && actor.team == team) continue;
            actor.TakeDamage(actorDefinition.damage);
        }
    }
}
