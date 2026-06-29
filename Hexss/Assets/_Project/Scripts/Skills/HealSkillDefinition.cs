using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "Game/Skills/Heal")]
public class HealSkillDefinition : SkillDefinition
{
    [SerializeField] private float amount = 1f;

    protected override void Apply(GridActor caster, GridActor target, WorldGrid worldGrid, Vector2Int initialActorPosition)
    {
        target.Heal(amount);
    }
}
