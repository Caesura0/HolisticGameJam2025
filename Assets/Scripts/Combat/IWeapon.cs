using UnityEngine;

public interface IWeapon
{

    public void DamagePlayer(Transform playerTransform, int damageAmount);

    public void DropWeapon();

}
