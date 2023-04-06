using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using BitOperations = System.Numerics.BitOperations;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public sealed class DynamicGore : ModGore
{
	public struct TextureHandle
	{
		public ushort Id;
		public ushort Version;

		public bool IsValid => Version != 0 && textures[Id].Version == Version;
	}

	private struct TextureData
	{
		public Texture2D Reference;
		public bool AutoRemove;
		public bool DisposeOnRemoval;
		public ushort Version;
	}

	private const int BitsPerMask = sizeof(ulong) * 8;

	public new static int Type => ModContent.GoreType<DynamicGore>();

	private static TextureHandle[] goreTextureMapping = Array.Empty<TextureHandle>();
	private static TextureData[] textures = Array.Empty<TextureData>();
	private static ulong[] texturesPresenceMask = Array.Empty<ulong>();
	private static uint renderTargetCount;

	public override string Texture { get; } = $"{nameof(TerrariaOverhaul)}/Assets/Textures/Transparent";

	public override void Load()
	{
		goreTextureMapping = new TextureHandle[Main.maxGore];

		On.Terraria.Main.DrawGore += DrawGoreDetour;
	}

	public override void Unload()
	{
		if (!Program.IsMainThread) {
			Main.QueueMainThreadAction(Unload);
			return;
		}

		//TODO: Maybe make a common enumerator for this, and make it abuse TrailingZeroCount even further.
		for (int i = 0, baseIndex = 0; i < texturesPresenceMask.Length; i++, baseIndex += BitsPerMask) {
			ulong mask = texturesPresenceMask[i];

			for (int j = BitOperations.TrailingZeroCount(mask); j < BitsPerMask; j++) {
				if ((mask & (1ul << j)) != 0ul) {
					RemoveTexture(baseIndex + j);
				}
			}
		}
	}

	public static int NewGore(TextureHandle texture, IEntitySource source, Vector2 position, Vector2 velocity, float scale = 1f)
	{
		int goreIndex = Gore.NewGore(source, position, velocity, Type, scale);

		if (goreIndex >= 0 && goreIndex < Main.maxGore) {
			SetGoreTexture(goreIndex, texture);
		}

		return goreIndex;
	}

	public static void SetGoreTexture(int goreIndex, TextureHandle texture)
	{
		goreTextureMapping[goreIndex] = texture;
	}

	public static TextureHandle RegisterTexture(Texture2D texture, bool autoRemove, bool disposeOnRemoval)
	{
		ushort index = AllocateTextureIndex();
		ref var data = ref textures[index];

		renderTargetCount++;

		// Update the presence bit
		(int bitIndex, int bitShift) = Math.DivRem((int)index, BitsPerMask);

		texturesPresenceMask[bitIndex] |= 1ul << bitShift;

		// Initialize data
		data.Reference = texture;
		data.AutoRemove = autoRemove;
		data.DisposeOnRemoval = disposeOnRemoval;

		if (data.Version == 0) {
			data.Version = 1;
		}

		// Create result handle
		TextureHandle handle;
		
		handle.Id = index;
		handle.Version = data.Version;

		return handle;
	}

	public static void RemoveTexture(TextureHandle texture)
	{
		if (!texture.IsValid) {
			return;
		}

		RemoveTexture(texture.Id);
	}

	private static void RemoveTexture(int textureId)
	{
		// Update the presence bit
		(int bitIndex, int bitShift) = Math.DivRem(textureId, BitsPerMask);

		texturesPresenceMask[bitIndex] &= ~(1ul << bitShift);

		// Dispose & nullify
		ref var data = ref textures[textureId];

		if (data.DisposeOnRemoval) {
			data.Reference.Dispose();
		}

		data.Reference = null!;

		// Raise version, with overflow
		unchecked {
			data.Version++;
		}
	}

	private static ushort AllocateTextureIndex()
	{
		ushort result;

		// Claim
		if (renderTargetCount < textures.Length) {
			for (int i = 0, baseIndex = 0; i < texturesPresenceMask.Length; i++, baseIndex += BitsPerMask) {
				int freeIndex = BitOperations.TrailingZeroCount(~texturesPresenceMask[i]);

				if (freeIndex != BitsPerMask) {
					result = (ushort)(baseIndex + freeIndex);

					return result;
				}
			}

			throw new InvalidOperationException($"{nameof(renderTargetCount)} is less than {nameof(textures)}.Length, but there is no zero bits in {nameof(texturesPresenceMask)}.");
		}

		// Resize
		result = (ushort)renderTargetCount;

		int texturesLength = (int)Math.Max(16, BitOperations.RoundUpToPowerOf2(renderTargetCount + 1));
		(int lengthDiv, int lengthRem) = Math.DivRem(texturesLength, BitsPerMask);

		Array.Resize(ref textures, texturesLength);
		Array.Resize(ref texturesPresenceMask, lengthDiv + (lengthRem != 0 ? 1 : 0));

		return result;
	}

	private static void DrawGoreDetour(On.Terraria.Main.orig_DrawGore orig, Main main)
	{
		orig(main);

		Span<ulong> referencedTexturesMask = stackalloc ulong[texturesPresenceMask.Length];

		DrawDynamicGore(referencedTexturesMask);
		DisposeUnreferencedTextures(referencedTexturesMask);
	}

	private static void DrawDynamicGore(Span<ulong> referencedTexturesMask)
	{
		int type = Type;
		var sb = Main.spriteBatch;

		for (int i = 0; i < Main.maxGore; i++) {
			var gore = Main.gore[i];

			if (gore.type != type || !gore.active) {
				continue;
			}

			TextureHandle textureHandle = goreTextureMapping[i];

			if (!textureHandle.IsValid) {
				gore.active = false;
				continue;
			}

			// Mark the texture as referenced.
			(int bitIndex, int bitShift) = Math.DivRem((int)textureHandle.Id, BitsPerMask);

			referencedTexturesMask[bitIndex] |= 1ul << bitShift;

			// Draw
			Texture2D texture = textures[textureHandle.Id].Reference;

			Rectangle? sourceRectangle;
			Vector2 halfSize;

			if (gore.Frame.ColumnCount > 1 || gore.Frame.RowCount > 1) {
				sourceRectangle = gore.Frame.GetSourceRectangle(texture);
				halfSize = sourceRectangle.Value.Size() * 0.5f;
			} else {
				sourceRectangle = null;
				halfSize = texture.Size() * 0.5f;
			}

			var position = gore.position + halfSize;
			var drawPosition = position + gore.drawOffset - halfSize - Main.screenPosition;
			Color drawColor = gore.GetAlpha(Lighting.GetColor(position.ToTileCoordinates()));

			sb.Draw(texture, drawPosition, sourceRectangle, drawColor, gore.rotation, halfSize, gore.scale, 0, 0f);
		}
	}

	private static void DisposeUnreferencedTextures(ReadOnlySpan<ulong> referencedTexturesMask)
	{
		for (int i = 0, baseIndex = 0; i < referencedTexturesMask.Length; i++, baseIndex += BitsPerMask) {
			ulong mask = ~referencedTexturesMask[i] & texturesPresenceMask[i];

			for (int j = BitOperations.TrailingZeroCount(mask); j < BitsPerMask; j++) {
				if ((mask & (1ul << j)) == 0ul) {
					continue;
				}

				int index = baseIndex + j;

				if (textures[index].AutoRemove) {
					RemoveTexture(index);
				}
			}
		}
	}
}
