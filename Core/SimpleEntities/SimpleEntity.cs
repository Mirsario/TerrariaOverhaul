// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.SimpleEntities;

// Because screw dusts and bloated vanilla classes.
internal class SimpleEntity : ILoadable
{
	public bool Destroyed { get; private set; }

	public void Destroy(bool allowEffects = false)
	{
		if (!Destroyed) {
			OnDestroyed(allowEffects);

			Destroyed = true;
		}
	}

	public virtual void Load(Mod mod) { }

	public virtual void Unload() { }

	public virtual void Init() { }

	public virtual void Update() { }

	public virtual void Draw(SpriteBatch sb) { }

	protected virtual void OnDestroyed(bool allowEffects) { }

	public static T Instantiate<T>(Action<T>? preinitializer = null) where T : SimpleEntity
		=> SimpleEntitySystem.InstantiateEntity(preinitializer);
}
