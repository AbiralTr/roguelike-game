[System.Serializable]
public class WeaponStats
{
    public int damage;
    public float critChance;
    public float attackSpeedBonus;

    public WeaponStats Add(WeaponStats other)
    {
        return new WeaponStats
        {
            damage = damage + other.damage,
            critChance = critChance + other.critChance,
            attackSpeedBonus = attackSpeedBonus + other.attackSpeedBonus
        };
    }
}
