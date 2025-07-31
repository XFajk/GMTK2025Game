extends Node

var disable_saving: bool = false
var save_file_path = "user://game.data"
	
func load_game() -> void:
	if not FileAccess.file_exists(save_file_path):
		push_warning("No save file to load")
		return 
		
	var save_file = FileAccess.open(save_file_path, FileAccess.READ)
	var save_data = read_data(save_file)
	
	
	var savers: Array[Node] = get_tree().get_nodes_in_group("Savers")

	for saver in savers:
		if not saver is Saver:
			push_error("A non Saver Node %s in groupe Saver" % saver.get_path())
			continue
			
		var object_to_load = saver.get_parent()
		var object_data: Dictionary = save_data.get(str(object_to_load.get_path()))
		
		if object_data != null:
			saver.LoadIn(object_data)
		else: 
			push_warning("No data for Object: %s" % str(object_to_load.get_path()))
		
		
func save_game() -> void:
	if disable_saving:
		print("Saving is disabled")
		return
	
	var save_file = FileAccess.open(save_file_path, FileAccess.READ)
	var save_data = read_data(save_file)
	
	var savers: Array[Node] = get_tree().get_nodes_in_group("Savers")
	
	for saver in savers:
		if not saver is Saver:
			push_error("A non Saver Node %s in groupe Saver" % saver.get_path())
			continue
		
		var object_to_save = saver.get_parent()
		var object_data: Dictionary = saver.SaveOut()
		
		save_data[str(object_to_save.get_path())] = object_data
		
	save_file = FileAccess.open(save_file_path, FileAccess.WRITE)
		
	write_data(save_file, save_data)
		
func read_data(file: FileAccess) -> Dictionary:
	if file == null:
		return {}
	
	var json = JSON.new()
	var parse_result: Error = json.parse(Marshalls.base64_to_utf8(file.get_as_text()))
	
	if not parse_result == OK:
		print("JSON Parse Error: ", json.get_error_message(), " in ", file.get_as_text(), " at line ", json.get_error_line())
		return {}
	else:
		return json.data

func delete_game() -> void:
	if FileAccess.file_exists(save_file_path):
		var file = FileAccess.open(save_file_path, FileAccess.WRITE)
		if file != null:
			write_data(file, {})  # Overwrite with empty data
			print("Game data has been cleared.")
		else:
			push_error("Failed to open save file for clearing.")
	else:
		print("No save file to delete.")
	
func write_data(file: FileAccess, data: Dictionary) -> void:
	var json_string: String = Marshalls.utf8_to_base64(JSON.stringify(data))
	file.store_string(json_string)

func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		print("Quit requested — saving game…")
		save_game()

