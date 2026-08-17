extends CanvasLayer

@onready var fill: ColorRect = $HealthBarBackground/HealthBarFill
@onready var score_label: Label = $RunStatsBackground/ScoreLabel
@onready var currency_label: Label = $RunStatsBackground/CurrencyLabel

var full_width: float
var player: Player

func _ready() -> void:
	full_width = fill.size.x
	player = get_tree().get_first_node_in_group("player") as Player

func _process(_delta: float) -> void:
	score_label.text = "Score: %d" % GameState.run_score
	currency_label.text = "Nanites: %d" % GameState.run_currency

	if player == null or player.player_data == null:
		return
	var pd: PlayerData = player.player_data
	var fraction: float = 0.0
	if pd.max_health > 0.0:
		fraction = clampf(pd.current_health / pd.max_health, 0.0, 1.0)
	fill.size.x = full_width * fraction
