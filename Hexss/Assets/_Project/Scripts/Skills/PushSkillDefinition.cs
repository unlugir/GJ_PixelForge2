using UnityEngine;

[CreateAssetMenu(fileName = "PushSkill", menuName = "Game/Skills/Push")]
public class PushSkillDefinition : SkillDefinition
{
    [SerializeField] private int distance = 1;

    protected override void Apply(GridActor caster, GridActor target, WorldGrid worldGrid, Vector2Int initialActorPosition)
    {
        if (caster == target) return;
        var targetPosition = worldGrid.GetActorPosition(target);
        var direction = targetPosition - initialActorPosition;

        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        if (direction == Vector2Int.zero) return;

        var destination = targetPosition + direction * distance;
        if (!worldGrid.TryGetCell(destination, out _)) return;
        if (worldGrid.GetActorAtPosition(destination) != null) return;

        worldGrid.TrySetActorPosition(target, destination);
    }
}
