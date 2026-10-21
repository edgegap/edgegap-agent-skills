extends Node2D
# Round flow. Runs on the host; others follow via RPCs.

const ROUNDS_TO_WIN := 3
var scores := {}

func _ready() -> void:
	if Match.is_host():
		start_round()

func start_round() -> void:
	rpc("spawn_players")

@rpc("authority", "call_local", "reliable")
func spawn_players() -> void:
	pass # spawns a fish per peer

# Called by the host when only one fish is left alive.
func on_round_won(peer_id: int) -> void:
	if not Match.is_host():
		return
	scores[peer_id] = scores.get(peer_id, 0) + 1
	if scores[peer_id] >= ROUNDS_TO_WIN:
		rpc("show_match_winner", peer_id)
		_record_win()
	else:
		start_round()

@rpc("authority", "call_local", "reliable")
func show_match_winner(peer_id: int) -> void:
	print("Winner: ", peer_id)

func _record_win() -> void:
	# The host writes its own win (only the host can win in practice...).
	await Online.nakama_client.write_leaderboard_record_async(Online.nakama_session, "reefrush_wins", 1)
