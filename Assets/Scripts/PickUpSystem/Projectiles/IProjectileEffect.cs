public interface IProjectileEffect
{
    void ApplyEffect(NPCSuperStateMachine target);
    string GetEffectDescription(); // debugging
}