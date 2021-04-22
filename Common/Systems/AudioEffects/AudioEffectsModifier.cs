namespace TerrariaOverhaul.Common.Systems.AudioEffects
{
	public struct AudioEffectsModifier
	{
		public delegate void ModifierDelegate(float intensity, ref float reverbIntensity, ref float lowPassIntensity);

		public readonly string Id;

		public ModifierDelegate Modifier { get; set; }
		public int TimeLeft { get; set; }
		public int TimeMax { get; set; }

		public AudioEffectsModifier(int timeLeft, string id, ModifierDelegate modifier) : this()
		{
			Id = id;
			TimeMax = TimeLeft = timeLeft;
			Modifier = modifier;
		}
	}
}
