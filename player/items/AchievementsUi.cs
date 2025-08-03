using Godot;
using System;

public partial class AchievementsUi : CanvasLayer
{
	[Export] private float Duration = 2.5f;
    [Export] private float BobHeight = 10f;
    [Export] private float BobSpeed = 1.5f;

    private VBoxContainer _container;
    private Label _templateLabel;

    AudioStreamPlayer audioStreamPlayer;
    AudioStreamPlayer audioStreamPlayer2;

    public override void _Ready()
    {
        _container = GetNode<VBoxContainer>("Control/VBoxContainer");
        _templateLabel = _container.GetNode<Label>("TemplateLabel");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("RewardPlayer");
        audioStreamPlayer2 = GetNode<AudioStreamPlayer>("CompletePlayer");
    }

    public void ShowAchievement(string message)
    {
        audioStreamPlayer.Play();
        if (Inventory.items.Count == 6)
        {
            audioStreamPlayer2.Play();
        }
        Label label = (Label)_templateLabel.Duplicate();
        label.Text = message;
        label.Visible = true;
        label.Modulate = new Color(1, 1, 1, 1); // Fully visible
        _container.AddChild(label);

        AnimateLabel(label);
    }

    private void AnimateLabel(Label label)
    {
        var tween = GetTree().CreateTween();

        // Bobbing up and down (oscillate y translation)
        Vector2 originalPosition = label.Position;
        Vector2 bobUp = originalPosition - new Vector2(0, BobHeight);
        Vector2 bobDown = originalPosition + new Vector2(0, BobHeight);

        tween.TweenProperty(label, "position", bobUp, BobSpeed / 2).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(label, "position", bobDown, BobSpeed).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(label, "position", originalPosition, BobSpeed / 2).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        // Wait a moment
        // tween.TweenInterval(Duration);

        // Fade out
        tween.TweenProperty(label, "modulate:a", 0.0f, 1.0f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        tween.TweenCallback(Callable.From(() => label.QueueFree()));
    }
}
