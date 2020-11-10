using TerrariaOverhaul.Core.Systems.Configuration;

namespace TerrariaOverhaul.Common.Systems.Camera
{
	public class CameraConfig : Config
	{
		public bool fixedCamera = true;
		public bool smoothCamera = true;
		public bool dialogueZoomIn = true;
		public bool dialogueFixCamera = true;
		public bool earthquakesScreenshake = true;
		//[AcceptedValues(0.00f,0.25f,0.50f,0.75f,1.00f)]
		public float screenShakeStrength = 1f;
	}
}
