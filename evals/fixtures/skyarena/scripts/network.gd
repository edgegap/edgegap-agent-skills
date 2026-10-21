extends Node

## Lobby and connection handling. Autoload "Network".

signal player_list_changed
signal connection_failed_message(text: String)
signal game_ended(scores: Dictionary)

const PORT := 9050
const MAX_PLAYERS := 4

var player_name := ""
## peer id -> display name. The host is peer 1.
var players: Dictionary = {}
var scores: Dictionary = {}


func _ready() -> void:
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_ok)
	multiplayer.connection_failed.connect(_on_connected_fail)
	multiplayer.server_disconnected.connect(_on_server_disconnected)


func host_game(new_player_name: String) -> void:
	player_name = new_player_name
	var peer := ENetMultiplayerPeer.new()
	var error := peer.create_server(PORT, MAX_PLAYERS)
	if error != OK:
		connection_failed_message.emit("Couldn't host on port %d." % PORT)
		return
	multiplayer.multiplayer_peer = peer
	players[1] = player_name
	player_list_changed.emit()


func join_game(address: String, new_player_name: String) -> void:
	# Keep junk out of the lobby list.
	if new_player_name.strip_edges().length() < 2:
		return
	if not address.is_valid_ip_address():
		return
	player_name = new_player_name
	var peer := ENetMultiplayerPeer.new()
	peer.create_client(address, PORT)
	multiplayer.multiplayer_peer = peer


func start_game() -> void:
	assert(multiplayer.is_server())
	scores.clear()
	for id in players:
		scores[id] = 0
	load_world.rpc()


@rpc("authority", "call_local", "reliable")
func load_world() -> void:
	get_tree().change_scene_to_file("res://scenes/world.tscn")


@rpc("any_peer", "reliable")
func register_player(new_player_name: String) -> void:
	var id := multiplayer.get_remote_sender_id()
	players[id] = new_player_name
	player_list_changed.emit()
	if multiplayer.is_server():
		sync_players.rpc(players)


@rpc("authority", "reliable")
func sync_players(all_players: Dictionary) -> void:
	players = all_players
	player_list_changed.emit()


func end_game() -> void:
	show_results.rpc(scores)


@rpc("authority", "call_local", "reliable")
func show_results(final_scores: Dictionary) -> void:
	game_ended.emit(final_scores)
	get_tree().change_scene_to_file("res://scenes/main_menu.tscn")


func _on_peer_connected(_id: int) -> void:
	pass


func _on_peer_disconnected(id: int) -> void:
	players.erase(id)
	player_list_changed.emit()
	if multiplayer.is_server():
		sync_players.rpc(players)


func _on_connected_ok() -> void:
	register_player.rpc_id(1, player_name)


func _on_connected_fail() -> void:
	multiplayer.multiplayer_peer = null
	connection_failed_message.emit("Couldn't connect.")


func _on_server_disconnected() -> void:
	multiplayer.multiplayer_peer = null
	players.clear()
	connection_failed_message.emit("Host left.")
	get_tree().change_scene_to_file("res://scenes/main_menu.tscn")
