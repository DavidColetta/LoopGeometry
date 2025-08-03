using Godot;
using System;

public partial class VolumeController : Control
{
	private HSlider _volumeSlider;
	private const string BusName = "SFX";

	private HSlider _musicVolumeSlider;
	private const string MusicBusName = "Music";

	public override void _Ready()
	{
		_volumeSlider = GetNode<HSlider>("MasterVolumeSlider");
		_musicVolumeSlider = GetNode<HSlider>("MusicVolumeSlider");

		// Initialize to current bus volume
		float currentVolumeDb = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(BusName));
		_volumeSlider.Value = DbToLinear(currentVolumeDb);

		float currentMusicVolumeDb = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(MusicBusName));
		_musicVolumeSlider.Value = DbToLinear(currentMusicVolumeDb);

		// Connect signal
		_volumeSlider.ValueChanged += OnVolumeChanged;
		_musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
	}

	private void OnVolumeChanged(double value)
	{
		int busIndex = AudioServer.GetBusIndex(BusName);
		AudioServer.SetBusVolumeDb(busIndex, LinearToDb((float)value));
	}

	private void OnMusicVolumeChanged(double value)
	{
		int musicBusIndex = AudioServer.GetBusIndex(MusicBusName);
		AudioServer.SetBusVolumeDb(musicBusIndex, LinearToDb((float)value));
	}

	float LinearToDb(float linear)
	{
		if (linear <= 0f)
			return -80f;
		return 20f * Mathf.Log(linear);
	}
	
	float DbToLinear(float db)
	{
		return Mathf.Pow(10f, db / 20f);
	}
}
