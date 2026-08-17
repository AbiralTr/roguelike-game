extends Resource
class_name WeaponData

@export var weapon_name: String = ""
@export var icon: Texture2D
@export var offense_part: WeaponPartData
@export var defense_part: WeaponPartData
@export var movement_part: WeaponPartData

func _parts() -> Array[WeaponPartData]:
	var result: Array[WeaponPartData] = []
	if offense_part != null:
		result.append(offense_part)
	if defense_part != null:
		result.append(defense_part)
	if movement_part != null:
		result.append(movement_part)
	return result

func get_aggregated_stats() -> WeaponStats:
	var stats := WeaponStats.new()
	for part in _parts():
		if part.stat_contribution != null:
			stats = stats.add(part.stat_contribution)
	return stats

func get_passives() -> Array[WeaponPassiveEffect]:
	var result: Array[WeaponPassiveEffect] = []
	for part in _parts():
		if part.passive != null:
			result.append(part.passive)
	return result
