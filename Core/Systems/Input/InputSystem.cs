using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Input
{
	public sealed class InputSystem : ModSystem
	{
		private static MouseState mouseState;
		private static MouseState mouseStatePrev;

		public override void Load()
		{
			PostUpdateInput();
		}

		public override void PostUpdateInput()
		{
			mouseStatePrev = mouseState;
			mouseState = Mouse.GetState();
		}

		//Keyboard
		public static bool GetKey(Keys key) => !PlayerInput.WritingText && Main.hasFocus && Main.keyState.IsKeyDown(key);
		public static bool GetKeyDown(Keys key) => !PlayerInput.WritingText && Main.hasFocus && Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
		public static bool GetKeyUp(Keys key) => !PlayerInput.WritingText && Main.hasFocus && !Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyDown(key);
		//Mouse
		public static bool GetMouseButton(int button) => Main.hasFocus && GetMouseButtonState(mouseState, button);
		public static bool GetMouseButtonDown(int button) => Main.hasFocus && GetMouseButtonState(mouseState, button) && !GetMouseButtonState(mouseStatePrev, button);
		public static bool GetMouseButtonUp(int button) => Main.hasFocus && !GetMouseButtonState(mouseState, button) && GetMouseButtonState(mouseStatePrev, button);

		private static bool GetMouseButtonState(MouseState mouseState, int button)
		{
			switch (button) {
				case 0:
					return mouseState.LeftButton == ButtonState.Pressed;
				case 1:
					return mouseState.RightButton == ButtonState.Pressed;
				case 2:
					return mouseState.MiddleButton == ButtonState.Pressed;
				case 3:
					return mouseState.XButton1 == ButtonState.Pressed;
				case 4:
					return mouseState.XButton2 == ButtonState.Pressed;
			}

			return false;
		}
	}
}
