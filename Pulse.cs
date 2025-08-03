using Godot;
using System;

public partial class Pulse : Node3D
{
    [Export] public float PulseScale = 1.1f;         
    [Export] public float PulseDuration = 0.5f;      // seconds
    [Export] public float PulseInterval = 2.0f;      // seconds
    [Export] public float RotationSpeed = 0f;     // Degrees per second

    private Vector3 _baseScale;
    private Timer _timer;

    public override void _Ready()
    {
        _baseScale = Scale;

        _timer = new Timer
        {
            WaitTime = PulseInterval,
            OneShot = false,
            Autostart = true
        };
        AddChild(_timer);
        _timer.Timeout += OnPulseTimeout;
    }

    public override void _Process(double delta)
    {
        RotateY(Mathf.DegToRad(RotationSpeed * (float)delta));
    }

    private void OnPulseTimeout()
    {
        var tween = CreateTween();

        // Scale up
        tween.TweenProperty(this, "scale", _baseScale * PulseScale, PulseDuration / 2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        // Scale back down
        tween.TweenProperty(this, "scale", _baseScale, PulseDuration / 2)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
    }
}
