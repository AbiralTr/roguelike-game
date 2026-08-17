extends SceneTree

func _init() -> void:
	var main_scene: PackedScene = load("res://scenes/Main.tscn")
	var main: Node = main_scene.instantiate()
	get_root().add_child(main)
	current_scene = main

	var player: Node2D = main.get_node("Player")
	var drone: Node2D = main.get_node("Drone")
	player.global_position = Vector2(150, 250)
	drone.global_position = Vector2(200, 250)

	for i in range(240):
		await physics_frame
		for child in main.get_children():
			if child is Projectile:
				print("frame ", i, " proj pos=", child.global_position, " dir=", child.direction)

	print("done")
	quit()
