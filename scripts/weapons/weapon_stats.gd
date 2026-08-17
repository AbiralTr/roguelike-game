extends Resource
class_name WeaponStats

@export var damage: int = 0
@export var crit_chance: float = 0.0
@export var attack_speed_bonus: float = 0.0

func add(other: WeaponStats) -> WeaponStats:
	var result := WeaponStats.new()
	result.damage = damage + other.damage
	result.crit_chance = crit_chance + other.crit_chance
	result.attack_speed_bonus = attack_speed_bonus + other.attack_speed_bonus
	return result
