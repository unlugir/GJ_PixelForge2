using System.Collections.Generic;
using UnityEngine;

public abstract class SkillDefinition : ScriptableObject
{
    public void Cast(GridActor caster, WorldGrid worldGrid, GridPattern gridPattern, Vector2Int initialActorPosition)
    {
        if (caster == null || worldGrid == null || gridPattern == null) return;

        foreach (var target in GetTargets(worldGrid, gridPattern, initialActorPosition))
            Apply(caster, target, worldGrid, initialActorPosition);
    }

    protected abstract void Apply(GridActor caster, GridActor target, WorldGrid worldGrid, Vector2Int initialActorPosition);

    private static IEnumerable<GridActor> GetTargets(WorldGrid worldGrid, GridPattern gridPattern, Vector2Int origin)
    {
        foreach (var cell in gridPattern.GetValidCellsForOrigin(origin))
        {
            var actor = worldGrid.GetActorAtPosition(cell);
            if (actor != null)
                yield return actor;
        }
    }
}
