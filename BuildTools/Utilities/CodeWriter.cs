using System;
using System.Text;

namespace TerrariaOverhaul.BuildTools.Utilities
{
	internal class CodeWriter
	{
		private readonly StringBuilder stringBuilder = new();

		private int indentation;
		private string? indentationString;
		private bool wroteLineStart;

		public override string ToString()
		{
			return stringBuilder.ToString();
		}

		public void Indent()
		{
			indentation++;
			RecalculateIndent();
		}

		public void Unindent()
		{
			indentation--;

			if (indentation < 0) {
				throw new InvalidOperationException();
			}

			RecalculateIndent();
		}

		public void Write(string? text)
		{
			if (!wroteLineStart) {
				WriteLineStart();
			}

			stringBuilder.Append(text);
		}

		public void WriteLine()
		{
			stringBuilder.AppendLine();

			wroteLineStart = false;
		}

		public void WriteLine(string? text)
		{
			if (!wroteLineStart) {
				WriteLineStart();
			}

			stringBuilder.AppendLine(text);

			wroteLineStart = false;
		}

		private void WriteLineStart()
		{
			stringBuilder.Append(indentationString);

			wroteLineStart = true;
		}

		private void RecalculateIndent()
		{
			indentationString = indentation > 0 ? new string('\t', indentation) : null;
		}
	}
}
