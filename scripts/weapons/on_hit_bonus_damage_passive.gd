extends WeaponPassiveEffect
class_name OnHitBonusDamagePassive

@export var bonus_damage: int = 5

func on_hit(_wielder: Node, target: Node) -> void:
	if target.has_method("take_damage"):
		target.take_damage(bonus_damage)
