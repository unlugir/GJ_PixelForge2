using UnityEngine;

[CreateAssetMenu(fileName = "DamageSkill", menuName = "Game/Skills/Damage")]
public class DamageSkillDefinition : SkillDefinition
{
    [SerializeField] private float amount = 1f;

    protected override void Apply(GridActor caster, GridActor target, WorldGrid worldGrid, Vector2Int initialActorPosition)
    {
        if (caster == target) return;
        target.TakeDamage(amount);
    }
}
