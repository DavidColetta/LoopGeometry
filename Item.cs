using Godot;
using System;
[GlobalClass]
public partial class Item : Resource
{
	[Export] public string itemName;
	[Export(PropertyHint.MultilineText)] public string itemDescription;
	[Export] public Texture2D itemTexture;
}
