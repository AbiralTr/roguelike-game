extends Resource
class_name WeaponPartData

enum PartSlot { OFFENSE, DEFENSE, MOVEMENT }

@export var part_name: String = ""
@export var slot: PartSlot = PartSlot.OFFENSE
@export var stat_contribution: WeaponStats
@export var passive: WeaponPassiveEffect
