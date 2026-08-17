extends Resource
class_name EnemyData

enum AttackType { MELEE, RANGED }

@export var max_health: float = 25.0
@export var contact_damage: int = 10
@export var attack_type: AttackType = AttackType.RANGED
@export var attack_damage: int = 10
@export var attack_range: float = 220.0
@export var attack_cooldown: float = 1.5
@export var projectile_speed: float = 400.0
@export var projectile_range: float = 300.0
@export var move_speed: float = 120.0
@export var detection_range: float = 320.0
