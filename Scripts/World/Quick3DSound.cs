using Godot;
using System;

public partial class Quick3DSound : AudioStreamPlayer3D
{
	[Export] public AudioStream[] sounds;
	[Export] public bool randomizePitch;
	public override void _Ready()
	{
		Stream = sounds[new Random().Next(0, sounds.Length)];
		Play();
		Finished += Die;
	}


	public void Die()
	{
		QueueFree();
	}
}
