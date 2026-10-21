extends Node
## PlayFab login over REST (autoload "PlayFab").

signal logged_in(playfab_id: String)
signal failed(message: String)

const TITLE_ID := "D41F7"

var session_ticket := ""
var entity_token := ""
var entity := {}
var playfab_id := ""


func login() -> void:
	var custom_id := _custom_id()
	var data = await post("/Client/LoginWithCustomID", {"TitleId": TITLE_ID, "CustomId": custom_id, "CreateAccount": true})
	if data == null:
		return
	session_ticket = data["SessionTicket"]
	entity_token = data["EntityToken"]["EntityToken"]
	entity = data["EntityToken"]["Entity"]
	playfab_id = data["PlayFabId"]
	logged_in.emit(playfab_id)


## POST to PlayFab, returns the "data" dictionary or null (and emits failed).
func post(path: String, body: Dictionary):
	var http := HTTPRequest.new()
	add_child(http)
	var headers := PackedStringArray(["Content-Type: application/json"])
	if session_ticket != "":
		headers.append("X-Authorization: " + session_ticket)
	if entity_token != "":
		headers.append("X-EntityToken: " + entity_token)
	http.request("https://%s.playfabapi.com%s" % [TITLE_ID, path], headers, HTTPClient.METHOD_POST, JSON.stringify(body))
	var result = await http.request_completed
	http.queue_free()
	var json = JSON.parse_string(result[3].get_string_from_utf8())
	if result[1] != 200 or json == null:
		failed.emit("%s failed: %s" % [path, json.get("errorMessage", "HTTP %d" % result[1]) if json else "no response"])
		return null
	return json["data"]


func _custom_id() -> String:
	var cfg := ConfigFile.new()
	cfg.load("user://player.cfg")
	var id: String = cfg.get_value("playfab", "custom_id", "")
	if id == "":
		id = "%d-%d" % [Time.get_unix_time_from_system(), randi()]
		cfg.set_value("playfab", "custom_id", id)
		cfg.save("user://player.cfg")
	return id
