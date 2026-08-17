extends CharacterBody2D
class_name Player

const GRAVITY: float = 980.0
const ATTACK_RADIUS_RATIO: float = 0.6
const GROUND_MASK: int = 1
const ENEMY_MASK_BIT: int = 4

const INVINCIBILITY_DURATION: float = 1.5
const KNOCKBACK_FORCE: float = 400.0
const KNOCKBACK_DURATION: float = 0.15
const NO_SOURCE: Vector2 = Vector2(INF, INF)

@export var player_data: PlayerData
@export var equipped_weapon: WeaponData

var facing: float = 1.0
var melee_cooldown_timer: float = 0.0
var ranged_cooldown_timer: float = 0.0

var is_dashing: bool = false
var dash_timer: float = 0.0
var dash_cooldown_timer: float = 0.0
var dash_direction: float = 1.0

var knockback_timer: float = 0.0
var knockback_velocity_x: float = 0.0
var invincible_timer: float = 0.0
var is_dead: bool = false

@onready var sprite: AnimatedSprite2D = $Sprite2D
@onready var melee_visual: AnimatedSprite2D = $MeleeVisual
@onready var attack_area: Area2D = $AttackArea
@onready var attack_shape: CollisionShape2D = $AttackArea/AttackShape

func _ready() -> void:
	add_to_group("player")
	player_data = player_data.duplicate() if player_data else PlayerData.new()
	player_data.initialize()
	melee_visual.animation_finished.connect(func() -> void: melee_visual.visible = false)

	var shape: CircleShape2D = attack_shape.shape.duplicate()
	shape.radius = player_data.melee_range * ATTACK_RADIUS_RATIO
	attack_shape.shape = shape

func _physics_process(delta: float) -> void:
	var move_input := Input.get_axis("move_left", "move_right")

	if move_input != 0.0:
		facing = sign(move_input)
		sprite.flip_h = facing < 0.0
	attack_area.position.x = player_data.melee_range * facing

	if dash_cooldown_timer > 0.0:
		dash_cooldown_timer -= delta
	if Input.is_action_just_pressed("dash") and dash_cooldown_timer <= 0.0 and not is_dashing:
		is_dashing = true
		dash_timer = player_data.dash_duration
		dash_cooldown_timer = player_data.dash_cooldown
		dash_direction = facing

	if is_dashing:
		velocity = Vector2(dash_direction * player_data.dash_speed, 0.0)
		dash_timer -= delta
		if dash_timer <= 0.0:
			is_dashing = false
	elif knockback_timer > 0.0:
		velocity = Vector2(knockback_velocity_x, 0.0)
		knockback_timer -= delta
	else:
		velocity.x = move_input * player_data.move_speed
		if not is_on_floor():
			velocity.y += GRAVITY * delta
		elif Input.is_action_just_pressed("jump"):
			velocity.y = -player_data.jump_force

	move_and_slide()

	if move_input != 0.0:
		sprite.play("walk")
	else:
		sprite.play("idle")

	if invincible_timer > 0.0:
		invincible_timer -= delta
		sprite.modulate.a = 0.5 + 0.5 * sin(Time.get_ticks_msec() / 50.0)
		collision_mask = GROUND_MASK
	else:
		sprite.modulate.a = 1.0
		collision_mask = GROUND_MASK | ENEMY_MASK_BIT

	if melee_cooldown_timer > 0.0:
		melee_cooldown_timer -= delta
	if ranged_cooldown_timer > 0.0:
		ranged_cooldown_timer -= delta

	if melee_cooldown_timer <= 0.0 and Input.is_action_just_pressed("melee_attack"):
		var weapon_stats: WeaponStats = equipped_weapon.get_aggregated_stats() if equipped_weapon != null else WeaponStats.new()
		_do_melee_attack(weapon_stats)
		melee_cooldown_timer = player_data.melee_cooldown / (1.0 + weapon_stats.attack_speed_bonus)

	if ranged_cooldown_timer <= 0.0 and Input.is_action_just_pressed("ranged_attack"):
		_do_ranged_attack()
		ranged_cooldown_timer = player_data.projectile_cooldown

func _do_melee_attack(weapon_stats: WeaponStats) -> void:
	var damage: int = player_data.melee_damage + weapon_stats.damage
	if randf() < weapon_stats.crit_chance:
		damage *= 2

	for body in attack_area.get_overlapping_bodies():
		if body.is_in_group("enemies") and body.has_method("take_damage"):
			body.take_damage(damage)
			if equipped_weapon != null:
				for passive in equipped_weapon.get_passives():
					passive.on_hit(self, body)

	melee_visual.position.x = player_data.melee_range * facing
	melee_visual.flip_h = facing < 0.0
	melee_visual.visible = true
	melee_visual.play("slash")

func _do_ranged_attack() -> void:
	var projectile: Projectile = preload("res://scenes/Projectile.tscn").instantiate()
	get_tree().current_scene.add_child(projectile)
	projectile.global_position = global_position + Vector2(20.0 * facing, -6.0)
	projectile.launch(Vector2(facing, 0.0), player_data.projectile_damage, player_data.projectile_speed, player_data.projectile_range, "enemies")

func apply_stat_boost(stat_type: PlayerData.StatType, amount: float) -> void:
	player_data.apply_stat_boost(stat_type, amount)

func equip(new_weapon: WeaponData, drop_position: Vector2) -> void:
	if equipped_weapon != null:
		var dropped: WeaponPickup = preload("res://scenes/WeaponPickup.tscn").instantiate()
		get_tree().current_scene.add_child(dropped)
		dropped.global_position = drop_position
		dropped.init(equipped_weapon)
	equipped_weapon = new_weapon

func apply_knockback(direction: float, force: float, duration: float) -> void:
	knockback_velocity_x = direction * force
	knockback_timer = duration

func take_damage(amount: float, source_position: Vector2 = NO_SOURCE) -> void:
	if is_dead or invincible_timer > 0.0:
		return
	player_data.take_damage(amount)
	invincible_timer = INVINCIBILITY_DURATION
	if source_position != NO_SOURCE:
		var dir := signf(global_position.x - source_position.x)
		if dir == 0.0:
			dir = -facing
		apply_knockback(dir, KNOCKBACK_FORCE, KNOCKBACK_DURATION)
	if player_data.is_dead():
		_die()

func _die() -> void:
	is_dead = true
	set_physics_process(false)
	GameState.die()
	var death_screen := get_tree().get_first_node_in_group("death_screen") as DeathScreen
	if death_screen != null:
		death_screen.show_screen()
