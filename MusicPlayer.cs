using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer
{
	[Export] public AudioStream[] musicTracks;
	[Export] public float fadeDuration = 1.0f;
	[Export] public bool loop = true;
	private int currentTrackIndex = 0;
	double timeSinceLastTrackChange = 0.0;
	public override void _Ready()
	{
		if (musicTracks.Length > 0)
		{
			currentTrackIndex = GD.RandRange(0, musicTracks.Length - 1);
			PlayTrack(currentTrackIndex);			
		}
	}
	public void PlayTrack(int trackIndex)
	{
		if (trackIndex < 0 || trackIndex >= musicTracks.Length)
		{
			GD.PrintErr("Invalid track index: " + trackIndex);
			return;
		}
		GD.Print("Playing track: " + musicTracks[trackIndex].ResourcePath);

		currentTrackIndex = trackIndex;
		Stream = musicTracks[currentTrackIndex];
		timeSinceLastTrackChange = 0.0;
		FadeIn();
	}

	public void FadeIn()
	{
		if (IsPlaying())
		{
			return;
		}

		VolumeDb = -80; // Start at silence
		Play();
		GetTree().CreateTimer(fadeDuration).Timeout += OnFadeInComplete;
	}
	private void OnFadeInComplete()
	{
		GetTree().CreateTween().TweenProperty(this, "volume_db", 0, fadeDuration).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
	}
	public override void _Process(double delta)
	{
		timeSinceLastTrackChange += delta;
		if (timeSinceLastTrackChange >= musicTracks[currentTrackIndex].GetLength())
		{
			NextTrack();
		}
	}

	public void NextTrack()
	{
		currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
		PlayTrack(currentTrackIndex);
	}

	public void PreviousTrack()
	{
		currentTrackIndex = (currentTrackIndex - 1 + musicTracks.Length) % musicTracks.Length;
		PlayTrack(currentTrackIndex);
	}
	
}
