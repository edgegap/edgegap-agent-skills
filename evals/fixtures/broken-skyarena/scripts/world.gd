extends Node2D

const PLAYER_SCENE := preload("res://scenes/player.tscn")
const ROUND_SECONDS := 180.0

@onready var players_root: Node2D = $Players
@onready var round_timer: Timer = $RoundTimer
@onready var hud_label: Label = $HUD/TimeLeft


func _ready() -> void:
	if multiplayer.is_server():
		var index := 0
		for id in Network.players:
			_spawn_player(id, index)
			index += 1
		multiplayer.peer_disconnected.connect(_on_peer_left)
		round_timer.wait_time = ROUND_SECONDS
		round_timer.timeout.connect(Network.end_game)
		round_timer.start()


func _process(_delta: float) -> void:
	hud_label.text = "%d" % int(round_timer.time_left)


func _spawn_player(id: int, index: int) -> void:
	var player := PLAYER_SCENE.instantiate()
	player.name = str(id)
	player.position = Vector2(200 + index * 250, 360)
	players_root.add_child(player, true)


func _on_peer_left(id: int) -> void:
	var node := players_root.get_node_or_null(str(id))
	if node != null:
		node.queue_free()


func add_score(id: int) -> void:
	Network.scores[id] = Network.scores.get(id, 0) + 1
