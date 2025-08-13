extends Node

var mouse_drag_sensitivity = 0.03
var sound_volume = 0.5:
	set = set_sound_volume,
	get = get_sound_volume

var music_volume = 0.5:
	set = set_music_volume,
	get = get_music_volume

var v_sync = true:
	set = set_v_sync,
	get = get_v_sync

func _ready(): 
	var saver: Saver = Saver.new()
	saver.PropertiesToSave = [
		"mouse_drag_sensitivity",
		"sound_volume",
		"music_volume",
		"v_sync"
	]
	add_child(saver)
	SaveSystem.load_game()

	AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("SFX"), sound_volume)
	AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("Music"), music_volume)
	DisplayServer.window_set_vsync_mode(DisplayServer.VSyncMode.VSYNC_ENABLED if v_sync else DisplayServer.VSyncMode.VSYNC_DISABLED)

func get_sound_volume():
	return sound_volume

func set_sound_volume(value):
	sound_volume = value
	AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("SFX"), sound_volume); 

func get_music_volume():
	return music_volume

func set_music_volume(value):
	music_volume = value
	AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("Music"), music_volume);

func get_v_sync():
	return v_sync

func set_v_sync(value):
	v_sync = value
	DisplayServer.window_set_vsync_mode(DisplayServer.VSyncMode.VSYNC_ENABLED if v_sync else DisplayServer.VSyncMode.VSYNC_DISABLED)
