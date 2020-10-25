using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.SimpleEntities
{
	public class SimpleEntitySystem : ModSystem
	{
		private IDictionary<Type, LinkedList<SimpleEntity>> entitiesByType;

		public override void Load()
		{
			entitiesByType = new Dictionary<Type, LinkedList<SimpleEntity>>();

			On.Terraria.Main.DrawPlayers_AfterProjectiles += (orig, self) => {
				orig(self);

				ModContent.GetInstance<SimpleEntitySystem>().DrawEntities();
			};
		}
		public override void Unload() => entitiesByType = null;
		public override void PreUpdateEntities() => UpdateEntities();

		public void UpdateEntities()
		{
			if(!Main.dedServ && Main.gamePaused) {
				return;
			}

			foreach(var entity in EnumerateEntities()) {
				entity.Update();
			}
		}
		public void DrawEntities()
		{
			var sb = Main.spriteBatch;

			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			foreach(var entity in EnumerateEntities()) {
				entity.Draw(sb);
			}

			sb.End();
		}
		public IEnumerable<SimpleEntity> EnumerateEntities()
		{
			foreach(var entities in entitiesByType.Values) {
				var node = entities.First;

				while(node != null) {
					var entity = node.Value;

					if(entity.Destroyed) {
						var next = node.Next;

						entities.Remove(node);

						node = next;
					} else {
						yield return entity;

						node = node.Next;
					}
				}
			}
		}

		public T InstantiateEntity<T>(Action<T> preinitializer = null) where T : SimpleEntity
		{
			T instance = Activator.CreateInstance<T>();

			preinitializer?.Invoke(instance);

			instance.Init();

			if(!entitiesByType.TryGetValue(typeof(T), out var list)) {
				entitiesByType[typeof(T)] = list = new LinkedList<SimpleEntity>();
			}

			list.AddLast(instance);

			return instance;
		}
	}
}
