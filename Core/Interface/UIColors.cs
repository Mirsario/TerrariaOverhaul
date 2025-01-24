// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Core.Interface;

public record struct UIColors(Color Normal, Color? Hover = null, Color? Active = null);
