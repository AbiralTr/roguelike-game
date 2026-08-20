extends Resource
class_name PlayerData

const MIN_DASH_COOLDOWN: float = 0.2

enum StatType { HEALTH, DAMAGE, SPEED, CRIT_CHANCE, ATTACK_SPEED, DASH_COOLDOWN }

@export var base_max_health: float = 100.0
@export var base_move_speed: float = 250.0
@export var jump_force: float = 420.0
@export var dash_speed: float = 600.0
@export var dash_duration: float = 0.15
@export var dash_cooldown: float = 1.0
@export var base_melee_damage: int = 10
@export var melee_range: float = 50.0
@export var melee_cooldown: float = 0.4
@export var base_projectile_damage: int = 8
@export var projectile_speed: float = 500.0
@export var projectile_range: float = 350.0
@export var projectile_cooldown: float = 0.5
@export var base_crit_chance: float = 0.0
@export var base_attack_speed_bonus: float = 0.0
@export var inventory_capacity: int = 12

var inventory: Inventory
var max_health: float
var current_health: float
var move_speed: float
var melee_damage: int
var projectile_damage: int
var crit_chance: float
var attack_speed_bonus: float

func initialize() -> void:
	max_health = base_max_health
	current_health = max_health
	move_speed = base_move_speed
	melee_damage = base_melee_damage
	projectile_damage = base_projectile_damage
	crit_chance = base_crit_chance
	attack_speed_bonus = base_attack_speed_bonus
	inventory = Inventory.new()
	inventory.capacity = inventory_capacity

func take_damage(amount: float) -> void:
	current_health = max(0.0, current_health - amount)

func heal(amount: float) -> void:
	current_health = min(max_health, current_health + amount)

func is_dead() -> bool:
	return current_health <= 0.0

func apply_stat_boost(stat_type: StatType, amount: float) -> void:
	match stat_type:
		StatType.HEALTH:
			max_health += amount
			current_health += amount
		StatType.DAMAGE:
			melee_damage += int(amount)
			projectile_damage += int(amount)
		StatType.SPEED:
			move_speed += amount
		StatType.CRIT_CHANCE:
			crit_chance += amount
		StatType.ATTACK_SPEED:
			attack_speed_bonus += amount
		StatType.DASH_COOLDOWN:
			dash_cooldown = max(MIN_DASH_COOLDOWN, dash_cooldown - amount)
