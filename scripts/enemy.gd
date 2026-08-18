extends CharacterBody2D

const GRAVITY: float = 980.0

@export var enemy_data: EnemyData

var current_health: float
var facing: float = -1.0
var attack_cooldown_timer: float = 0.0

@onready var sprite: AnimatedSprite2D = $Sprite2D
@onready var contact_area: Area2D = get_node_or_null("ContactArea")
@onready var melee_visual: AnimatedSprite2D = get_node_or_null("MeleeVisual")

func _ready() -> void:
	add_to_group("enemies")
	current_health = enemy_data.max_health
	if contact_area:
		contact_area.body_entered.connect(_on_contact_body_entered)
	if melee_visual:
		melee_visual.animation_finished.connect(func() -> void: melee_visual.visible = false)

func _physics_process(delta: float) -> void:
	if not is_on_floor():
		velocity.y += GRAVITY * delta
	else:
		velocity.y = 0.0

	var player := get_tree().get_first_node_in_group("player") as Node2D
	velocity.x = 0.0

	if player:
		_update_movement(player)
		_update_attack(player, delta)

	move_and_slide()

	if velocity.x != 0.0:
		sprite.play("move")
	else:
		sprite.play("idle")

func _update_movement(player: Node2D) -> void:
	var dx: float = player.global_position.x - global_position.x
	var distance: float = absf(dx)
	var hold_ground: bool = enemy_data.attack_type == EnemyData.AttackType.RANGED and distance <= enemy_data.attack_range

	if distance <= enemy_data.detection_range:
		if dx != 0.0:
			facing = signf(dx)
		sprite.flip_h = facing < 0.0
		if not hold_ground:
			velocity.x = facing * enemy_data.move_speed

func _update_attack(player: Node2D, delta: float) -> void:
	if attack_cooldown_timer > 0.0:
		attack_cooldown_timer -= delta

	var distance: float = absf(player.global_position.x - global_position.x)
	if distance > enemy_data.attack_range or attack_cooldown_timer > 0.0:
		return

	if enemy_data.attack_type == EnemyData.AttackType.RANGED:
		_fire(player)
	else:
		_melee_attack(player)

	attack_cooldown_timer = enemy_data.attack_cooldown

func _fire(player: Node2D) -> void:
	var facing_sign := signf(player.global_position.x - global_position.x)
	var spawn_pos: Vector2 = global_position + Vector2(16.0 * facing_sign, -6.0)
	var projectile: Projectile = preload("res://scenes/Projectile.tscn").instantiate()
	get_tree().current_scene.add_child(projectile)
	projectile.global_position = spawn_pos
	projectile.launch(Vector2(facing_sign, 0.0), enemy_data.attack_damage, enemy_data.projectile_speed, enemy_data.projectile_range, "player")

func _melee_attack(player: Node2D) -> void:
	if player.has_method("take_damage"):
		player.take_damage(enemy_data.attack_damage, global_position)
	if melee_visual:
		var facing_sign := signf(player.global_position.x - global_position.x)
		melee_visual.position.x = enemy_data.attack_range * facing_sign
		melee_visual.flip_h = facing_sign < 0.0
		melee_visual.visible = true
		melee_visual.play("slash")

func _on_contact_body_entered(body: Node2D) -> void:
	if body.is_in_group("player") and body.has_method("take_damage"):
		body.take_damage(enemy_data.contact_damage, global_position)

func take_damage(amount: float) -> void:
	if current_health <= 0.0:
		return
	current_health -= amount
	if current_health <= 0.0:
		_die()

func _die() -> void:
	for i in range(enemy_data.currency_drop):
		var nanite: NanitePickup = preload("res://scenes/NanitePickup.tscn").instantiate()
		get_tree().current_scene.add_child(nanite)
		nanite.global_position = global_position
	if enemy_data.score_value > 0:
		GameState.add_score(enemy_data.score_value)
	queue_free()
