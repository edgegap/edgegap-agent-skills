extends Node
# Dedicated server mode for Edgegap (headless). Started by Nakama's fleet manager.

const PORT := 7777

var active := false
var instance_id := ''
var instance_event_url := ''
var connection_event_url := ''
var players := {}  # peer_id -> name

func _ready() -> void:
	active = DisplayServer.get_name() == "headless"
	if not active:
		return
	instance_id = OS.get_environment("ARBITRIUM_REQUEST_ID")
	instance_event_url = OS.get_environment("NAKAMA_INSTANCE_EVENT_URL")
	connection_event_url = OS.get_environment("NAKAMA_CONNECTION_EVENT_URL")
	print("Edgegap env: ", OS.get_environment("NAKAMA_INSTANCE_EVENT_URL"))

	var peer := ENetMultiplayerPeer.new()
	peer.create_server(PORT, 4)
	multiplayer.multiplayer_peer = peer
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)

	await _post(instance_event_url, { instance_id = instance_id, action = "READY" })

# Clients call this right after connecting.
@rpc("any_peer", "call_remote", "reliable")
func hello(player_name: String) -> void:
	players[multiplayer.get_remote_sender_id()] = player_name

func _on_peer_connected(peer_id: int) -> void:
	await _post(connection_event_url, { instance_id = instance_id, connections = [str(peer_id)] })

func _on_peer_disconnected(peer_id: int) -> void:
	players.erase(peer_id)

# Battle.gd calls this when a captain has won 3 rounds.
func match_over(winner_name: String) -> void:
	print("Match over, winner ", winner_name)
	get_tree().quit()

func _post(url: String, body: Dictionary) -> void:
	var request := HTTPRequest.new()
	add_child(request)
	request.request(url, ["Content-Type: application/json"], HTTPClient.METHOD_POST, JSON.stringify(body))
	await request.request_completed
	request.queue_free()
