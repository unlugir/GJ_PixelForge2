using UnityEngine;

[CreateAssetMenu(fileName = "New Actor", menuName = "Game/Actor")]
public class ActorDefinition : ScriptableObject
{
    public GridPattern movementPattern;
    public GridPattern attackPattern;
    public GridPattern skillPattern;
    public SkillDefinition skillDefinition;
    public bool attackAllies;
    public float damage;
    public float health;
}
