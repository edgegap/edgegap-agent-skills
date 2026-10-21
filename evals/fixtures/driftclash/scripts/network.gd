extends Node
## ENet networking (autoload "Network"). `godot --headless -- --server` runs the dedicated server.

const PORT := 9050
const MAX_PLAYERS := 4

signal connected
signal connection_failed


func _ready() -> void:
	if "--server" in OS.get_cmdline_user_args() or OS.has_feature("dedicated_server"):
		start_server()


func start_server() -> void:
	var peer := ENetMultiplayerPeer.new()
	var err := peer.create_server(PORT, MAX_PLAYERS)
	if err != OK:
		push_error("Could not listen on %d: %s" % [PORT, err])
		get_tree().quit(1)
		return
	multiplayer.multiplayer_peer = peer
	print("Server listening on %d" % PORT)
	get_tree().change_scene_to_file.call_deferred("res://scenes/race.tscn")


func join(address: String, port: int = PORT) -> void:
	var peer := ENetMultiplayerPeer.new()
	peer.create_client(address, port)
	multiplayer.multiplayer_peer = peer
	multiplayer.connected_to_server.connect(func(): connected.emit(), CONNECT_ONE_SHOT)
	multiplayer.connection_failed.connect(func(): connection_failed.emit(), CONNECT_ONE_SHOT)
