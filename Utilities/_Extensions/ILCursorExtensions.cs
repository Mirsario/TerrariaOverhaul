using System;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Utilities;

internal static class ILCursorExtensions
{
	public static int AddLocalVariable(this ILCursor cursor, Type type)
	{
		var context = cursor.Context;
		var body = context.Body;

		var importedType = context.Import(type);
		int localId = body.Variables.Count;

		body.Variables.Add(new VariableDefinition(importedType));

		return localId;
	}

	public static ILCursor HijackIncomingLabels(this ILCursor cursor)
	{
		var incomingLabels = cursor.IncomingLabels.ToArray();

		cursor.Emit(OpCodes.Nop);

		foreach (var incomingLabel in incomingLabels) {
			incomingLabel.Target = cursor.Prev;
		}

		return cursor;
	}

	public static Func<Instruction, bool>?[] CreateDebugInstructionPredicates(this ILCursor _, Expression<Func<Instruction, bool>>?[] expressions)
	{
		var result = new Func<Instruction, bool>?[expressions.Length];

		for (int i = 0; i < expressions.Length; i++) {
			var expression = expressions[i];

			if (expression == null) {
				continue;
			}

			string expressionText = expression.ToString();
			var predicate = expression.Compile();

			result[i] = i => {
				bool result = predicate(i);

				if (!result) {
					DebugSystem.Logger.Debug($"Expression '{expressionText}' returned false on instruction '{i.Offset:x4}' ({i.OpCode}).");
				}

				return result;
			};
		}

		return result;
	}
}
