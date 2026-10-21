class_name Matchmaking
extends Node

## Find Match for the main menu: Edgegap matchmaker tickets, then join.

signal status(text: String)

const MATCHMAKER_URL := "https://om-r4t8y2u6i0.edgegap.net"
const AUTH_TOKEN := "PASTE_MATCHMAKER_AUTH_TOKEN"

var ticket_id := ""
var _http: HTTPRequest
var _timer: Timer


func _ready() -> void:
	_http = HTTPRequest.new()
	add_child(_http)
	_timer = Timer.new()
	_timer.wait_time = 2.0
	_timer.timeout.connect(_poll)
	add_child(_timer)


func find_match() -> void:
	status.emit("Searching…")
	_http.request_completed.connect(_on_created, CONNECT_ONE_SHOT)
	_http.request(MATCHMAKER_URL + "/tickets", ["Authorization: " + AUTH_TOKEN, "Content-Type: application/json"],
		HTTPClient.METHOD_POST, JSON.stringify({"profile": "default", "attributes": {}}))


func _on_created(_result: int, code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	if code != 200 and code != 201:
		status.emit("Matchmaker error %d" % code)
		return
	ticket_id = JSON.parse_string(body.get_string_from_utf8()).id
	_timer.start()


func _poll() -> void:
	_http.request_completed.connect(_on_status, CONNECT_ONE_SHOT)
	_http.request(MATCHMAKER_URL + "/tickets/" + ticket_id, ["Authorization: " + AUTH_TOKEN])


func _on_status(_result: int, code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	if code != 200:
		return
	var data: Dictionary = JSON.parse_string(body.get_string_from_utf8())
	if data.status == "HOST_ASSIGNED":
		_timer.stop()
		status.emit("Joining…")
		Network.join_game(data.assignment.fqdn, Network.player_name)
