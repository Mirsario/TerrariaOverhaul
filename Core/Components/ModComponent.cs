using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Components
{
	public abstract class ModComponent : ModType
	{
		public virtual ModComponent Clone() => (ModComponent)MemberwiseClone();

		protected override void Register()
		{
			ModTypeLookup<ModComponent>.Register(this);
		}
	}

	public abstract class ModComponent<TEntity> : ModComponent
		where TEntity : class
	{
		protected override void Register()
		{
			base.Register();

			ModTypeLookup<ModComponent<TEntity>>.Register(this);
		}

		public virtual void OnInit(TEntity entity) { }

		public virtual void OnDispose(TEntity entity) { }
	}
}
