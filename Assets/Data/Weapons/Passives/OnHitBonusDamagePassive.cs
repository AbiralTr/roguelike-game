using UnityEngine;

[CreateAssetMenu(fileName = "OnHitBonusDamagePassive", menuName = "Game/Weapons/Passives/On-Hit Bonus Damage")]
public class OnHitBonusDamagePassive : WeaponPassiveEffect
{
    public int bonusDamage = 5;

    public override void OnHit(GameObject wielder, GameObject target)
    {
        target.GetComponent<EnemyHealth>()?.TakeDamage(bonusDamage);
    }
}
