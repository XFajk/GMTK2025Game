extends Node

var mouse_drag_sensitivity = 0.03
var sound_volume = 0.5:
    set = set_sound_volume,
    get = get_sound_volume

var music_volume = 0.5:
    set = set_music_volume,
    get = get_music_volume

func _ready():
    AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("SFX"), sound_volume)
    AudioServer.set_bus_volume_linear(AudioServer.get_bus_index("Music"), music_volume)

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