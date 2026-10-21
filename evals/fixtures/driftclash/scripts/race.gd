extends Node3D
## Race scene. The server counts laps and decides the winner.

const LAPS := 3

var laps := {}


func _ready() -> void:
	if multiplayer.is_server():
		multiplayer.peer_connected.connect(func(id): laps[id] = 0)
		multiplayer.peer_disconnected.connect(func(id): laps.erase(id))


## Called by the finish-line Area3D on the server.
func on_lap_completed(peer_id: int) -> void:
	if not multiplayer.is_server():
		return
	laps[peer_id] = laps.get(peer_id, 0) + 1
	if laps[peer_id] >= LAPS:
		announce_winner.rpc(peer_id)


@rpc("authority", "call_local", "reliable")
func announce_winner(peer_id: int) -> void:
	print("Winner: %d" % peer_id)
