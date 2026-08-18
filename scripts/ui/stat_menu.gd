extends CanvasLayer
class_name StatMenu

var is_open: bool = false
var player: Player = null

@onready var health_label: Label = $Panel/StatsArea/StatsBox/HealthLabel
@onready var damage_label: Label = $Panel/StatsArea/StatsBox/DamageLabel
@onready var speed_label: Label = $Panel/StatsArea/StatsBox/SpeedLabel
@onready var crit_label: Label = $Panel/StatsArea/StatsBox/CritLabel
@onready var attack_speed_label: Label = $Panel/StatsArea/StatsBox/AttackSpeedLabel
@onready var weapon_label: Label = $Panel/InvArea/WeaponLabel
@onready var currency_label: Label = $Panel/TerminalArea/TerminalBox/CurrencyLabel
@onready var score_label: Label = $Panel/TerminalArea/TerminalBox/ScoreLabel
@onready var time_label: Label = $Panel/TerminalArea/TerminalBox/TimeLabel

func _ready() -> void:
	visible = false
	player = get_tree().get_first_node_in_group("player") as Player

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("toggle_stat_menu"):
		toggle()

func toggle() -> void:
	var accepted: bool = GameState.request_close_stat_menu() if is_open else GameState.request_open_stat_menu()
	if accepted:
		is_open = not is_open
		visible = is_open

func _process(_delta: float) -> void:
	if not is_open or player == null:
		return
	var pd: PlayerData = player.player_data
	health_label.text = "Health: %s / %s" % [int(pd.current_health), int(pd.max_health)]
	damage_label.text = "Melee Damage: %s\nProjectile Damage: %s" % [pd.melee_damage, pd.projectile_damage]
	speed_label.text = "Speed: %.1f" % pd.move_speed
	crit_label.text = "Crit Chance: %d%%" % int(round(pd.crit_chance * 100.0))
	attack_speed_label.text = "Attack Speed: +%d%%" % int(round(pd.attack_speed_bonus * 100.0))
	weapon_label.text = "Weapon: %s" % (player.equipped_weapon.weapon_name if player.equipped_weapon != null else "None")

	currency_label.text = "Nanites: %d" % GameState.run_currency
	score_label.text = "Score: %d" % GameState.run_score
	var total_seconds: int = int(GameState.run_elapsed_time)
	time_label.text = "Time: %02d:%02d" % [total_seconds / 60, total_seconds % 60]
