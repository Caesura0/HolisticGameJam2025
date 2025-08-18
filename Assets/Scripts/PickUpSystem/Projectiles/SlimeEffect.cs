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
            target.currentWeapon.DisarmNPC();
        }

        // Force panic and apply slow
        target.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
        target.panicState.ApplySlime(slowDuration);
    }

    public string GetEffectDescription() => "Slime: Disarm + Slow";
}
