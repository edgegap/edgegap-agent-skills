extends Node

## Starts the dedicated server on Edgegap. Autoload "EdgegapBoot".

func _ready() -> void:
	if not "--server" in OS.get_cmdline_user_args():
		return
	print("Sky Arena dedicated server starting")
	var peer := ENetMultiplayerPeer.new()
	peer.set_bind_ip("127.0.0.1")
	var error := peer.create_server(Network.PORT, Network.MAX_PLAYERS)
	if error != OK:
		print("create_server failed: %d" % error)
		get_tree().quit(1)
		return
	multiplayer.multiplayer_peer = peer
	Network.players[1] = "Server"
	print("listening on %d" % Network.PORT)


func _exit_tree() -> void:
	# Tell Edgegap we're done so the deployment stops.
	var url := OS.get_environment("ARBITRIUM_DELETE_URL")
	var token := OS.get_environment("ARBITRIUM_DELETE_TOKEN")
	if url.is_empty():
		return
	var http := HTTPRequest.new()
	add_child(http)
	http.request(url, ["Authorization: Bearer %s" % token], HTTPClient.METHOD_DELETE)
