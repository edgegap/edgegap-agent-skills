extends Node2D
# Round flow. On a dedicated server, the server runs this; clients follow via RPCs.

const ROUNDS_TO_WIN := 3
var scores := {}

func on_round_won(peer_id: int) -> void:
	if not multiplayer.is_server():
		return
	scores[peer_id] = scores.get(peer_id, 0) + 1
	if scores[peer_id] >= ROUNDS_TO_WIN:
		rpc("show_winner", peer_id)
		EdgegapServer.match_over(EdgegapServer.players.get(peer_id, "?"))

@rpc("authority", "call_local", "reliable")
func show_winner(peer_id: int) -> void:
	if peer_id == multiplayer.get_unique_id():
		Online.submit_win()
