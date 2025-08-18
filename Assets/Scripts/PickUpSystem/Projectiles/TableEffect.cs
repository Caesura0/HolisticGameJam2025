using UnityEngine;
public class TableEffect : MonoBehaviour, IProjectileEffect
{
    [SerializeField] float stunDuration = 3f;
    [SerializeField] bool disarmsWeapon = true;

    public void ApplyEffect(NPCSuperStateMachine target)
    {
        if (disarmsWeapon && target.currentWeapon != null)
        {
            target.currentWeapon.DisarmNPC();
        }

        target.OnStunHit(stunDuration);
        // Play sfx, vfx
        target.onStunned?.Invoke();
    }

    public string GetEffectDescription() => "Table: Disarm + Stun";
}
