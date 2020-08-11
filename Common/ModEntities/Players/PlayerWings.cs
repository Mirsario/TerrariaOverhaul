namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerWings : OverhaulPlayer
	{
		public int noWingsTime;

		public override void PreUpdate()
		{
			//No Wings Time
			if(noWingsTime>0) {
				player.wingsLogic = 0;
				noWingsTime--;
			}
		}
	}
}
