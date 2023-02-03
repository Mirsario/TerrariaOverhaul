using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.ItemComponents;

/// <summary>
/// A <see cref="GlobalItem"/> that can be enabled and disabled at will.
/// <br/> Might be removed in the future in favor of TML redesigns/additions.
/// </summary>
public abstract class ItemComponent : GlobalItem
{
	private bool enabled;

	// Unfortunately, this currently has to be checked in every override. Sucks.
	public bool Enabled {
		get => enabled;
	}

	public override bool InstancePerEntity => true;

	public virtual void OnEnabled(Item item) { }

	public virtual void OnDisabled(Item item) { }

	public override GlobalItem Clone(Item item, Item itemClone)
	{
		return base.Clone(item, itemClone);
	}

	public void SetEnabled(Item item, bool value)
	{
		if (enabled == value) {
			return;
		}

		enabled = value;

		if (value) {
			OnEnabled(item);
		} else {
			OnDisabled(item);
		}
	}
}
