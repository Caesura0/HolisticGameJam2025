using UnityEngine;
public class SlimeEffect : MonoBehaviour, IProjectileEffect
{
    [SerializeField] float slowDuration = 2f;
    [SerializeField] bool disarmsWeapon = true;

    public void ApplyEffect(NPCSuperStateMachine target)
    {
        if (disarmsWeapon && target.currentWeapon != null)
        {
            Debug.Log("Slime disarming weapon!");
            target.currentWeapon.Drop();
        }

        // Force panic and apply slow
        target.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
        target.OnSlimeHit(slowDuration);
    }

    public string GetEffectDescription() => "Slime: Disarm + Slow";
}
