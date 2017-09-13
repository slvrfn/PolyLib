/* coded by sean walsh: http://www.capesean.co.za */
// This product includes color specifications and designs developed by Cynthia Brewer (http://colorbrewer.org/).
// Copyright (c) 2002 Cynthia Brewer, Mark Harrower, and The Pennsylvania State University.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LowPolyLibrary
{
	public static class ColorBru
	{
		public static List<Palette> Palettes;

		public enum Code
		{
			// ReSharper disable InconsistentNaming
			YlGn, YlGnBu, GnBu, BuGn, PuBuGn, PuBu, BuPu, RdPu, PuRd, OrRd, YlOrRd, YlOrBr,
			Purples, Blues, Greens, Oranges, Reds, Greys, PuOr, BrBG, PRGn, PiYG, RdBu, RdGy,
			RdYlBu, Spectral, RdYlGn, Accent, Dark2, Paired, Pastel1, Pastel2, Set1, Set2, Set3
			// ReSharper restore InconsistentNaming
		}

		static ColorBru()
		{
			Palettes = new List<Palette>();
			//Palettes.Add(new Palette
			//	{
			//		Code = Code.Excel1,
			//		Label = "Excel 1",
			//		Type = Type.Qualitative,
			//		HtmlCodes = new List<string[]>
			//		{
			//			new[] {"#5a9bd5","#ec7a31","#a2a9a1","#fac100","#486fca","#6faf41","#2c5c8d","#8b4e22","#636363","#9b700a","#284476","#45672b","#7fafe0","#f69551","#b4b7bc","#ffca40","#6a8dcf","#8ac163","#307dc1","#d45e1e"}
			//		}
			//	});
			Palettes.Add(new Palette
				{
					Code = Code.YlGn,
					Label = "Yellow Green",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#f7fcb9", "#addd8e", "#31a354"},
						new[] {"#f7fcb9", "#addd8e", "#31a354"},
						new[] {"#f7fcb9", "#addd8e", "#31a354"},
						new[] {"#ffffcc", "#c2e699", "#78c679", "#238443"},
						new[] {"#ffffcc", "#c2e699", "#78c679", "#31a354", "#006837"},
						new[] {"#ffffcc", "#d9f0a3", "#addd8e", "#78c679", "#31a354", "#006837"},
						new[] {"#ffffcc", "#d9f0a3", "#addd8e", "#78c679", "#41ab5d", "#238443", "#005a32"},
						new[] {"#ffffe5", "#f7fcb9", "#d9f0a3", "#addd8e", "#78c679", "#41ab5d", "#238443", "#005a32"},
						new[] {"#ffffe5", "#f7fcb9", "#d9f0a3", "#addd8e", "#78c679", "#41ab5d", "#238443", "#006837", "#004529"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.YlGnBu,
					Label = "Yellow Green Blue",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#edf8b1", "#7fcdbb", "#2c7fb8"},
						new[] {"#ffffcc", "#a1dab4", "#41b6c4", "#225ea8"},
						new[] {"#ffffcc", "#a1dab4", "#41b6c4", "#2c7fb8", "#253494"},
						new[] {"#ffffcc", "#c7e9b4", "#7fcdbb", "#41b6c4", "#2c7fb8", "#253494"},
						new[] {"#ffffcc", "#c7e9b4", "#7fcdbb", "#41b6c4", "#1d91c0", "#225ea8", "#0c2c84"},
						new[] {"#ffffd9", "#edf8b1", "#c7e9b4", "#7fcdbb", "#41b6c4", "#1d91c0", "#225ea8", "#0c2c84"},
						new[] {"#ffffd9", "#edf8b1", "#c7e9b4", "#7fcdbb", "#41b6c4", "#1d91c0", "#225ea8", "#253494", "#081d58"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.GnBu,
					Label = "Green Blue",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e0f3db", "#a8ddb5", "#43a2ca"},
						new[] {"#f0f9e8", "#bae4bc", "#7bccc4", "#2b8cbe"},
						new[] {"#f0f9e8", "#bae4bc", "#7bccc4", "#43a2ca", "#0868ac"},
						new[] {"#f0f9e8", "#ccebc5", "#a8ddb5", "#7bccc4", "#43a2ca", "#0868ac"},
						new[] {"#f0f9e8", "#ccebc5", "#a8ddb5", "#7bccc4", "#4eb3d3", "#2b8cbe", "#08589e"},
						new[] {"#f7fcf0", "#e0f3db", "#ccebc5", "#a8ddb5", "#7bccc4", "#4eb3d3", "#2b8cbe", "#08589e"},
						new[] {"#f7fcf0", "#e0f3db", "#ccebc5", "#a8ddb5", "#7bccc4", "#4eb3d3", "#2b8cbe", "#0868ac", "#084081"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.BuGn,
					Label = "Blue Green",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e5f5f9", "#99d8c9", "#2ca25f"},
						new[] {"#edf8fb", "#b2e2e2", "#66c2a4", "#238b45"},
						new[] {"#edf8fb", "#b2e2e2", "#66c2a4", "#2ca25f", "#006d2c"},
						new[] {"#edf8fb", "#ccece6", "#99d8c9", "#66c2a4", "#2ca25f", "#006d2c"},
						new[] {"#edf8fb", "#ccece6", "#99d8c9", "#66c2a4", "#41ae76", "#238b45", "#005824"},
						new[] {"#f7fcfd", "#e5f5f9", "#ccece6", "#99d8c9", "#66c2a4", "#41ae76", "#238b45", "#005824"},
						new[] {"#f7fcfd", "#e5f5f9", "#ccece6", "#99d8c9", "#66c2a4", "#41ae76", "#238b45", "#006d2c", "#00441b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PuBuGn,
					Label = "Purple Blue Green",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#ece2f0", "#a6bddb", "#1c9099"},
						new[] {"#f6eff7", "#bdc9e1", "#67a9cf", "#02818a"},
						new[] {"#f6eff7", "#bdc9e1", "#67a9cf", "#1c9099", "#016c59"},
						new[] {"#f6eff7", "#d0d1e6", "#a6bddb", "#67a9cf", "#1c9099", "#016c59"},
						new[] {"#f6eff7", "#d0d1e6", "#a6bddb", "#67a9cf", "#3690c0", "#02818a", "#016450"},
						new[] {"#fff7fb", "#ece2f0", "#d0d1e6", "#a6bddb", "#67a9cf", "#3690c0", "#02818a", "#016450"},
						new[] {"#fff7fb", "#ece2f0", "#d0d1e6", "#a6bddb", "#67a9cf", "#3690c0", "#02818a", "#016c59", "#014636"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PuBu,
					Label = "Purple Blue",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#ece7f2", "#a6bddb", "#2b8cbe"},
						new[] {"#f1eef6", "#bdc9e1", "#74a9cf", "#0570b0"},
						new[] {"#f1eef6", "#bdc9e1", "#74a9cf", "#2b8cbe", "#045a8d"},
						new[] {"#f1eef6", "#d0d1e6", "#a6bddb", "#74a9cf", "#2b8cbe", "#045a8d"},
						new[] {"#f1eef6", "#d0d1e6", "#a6bddb", "#74a9cf", "#3690c0", "#0570b0", "#034e7b"},
						new[] {"#fff7fb", "#ece7f2", "#d0d1e6", "#a6bddb", "#74a9cf", "#3690c0", "#0570b0", "#034e7b"},
						new[] {"#fff7fb", "#ece7f2", "#d0d1e6", "#a6bddb", "#74a9cf", "#3690c0", "#0570b0", "#045a8d", "#023858"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.BuPu,
					Label = "Blue Purple",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e0ecf4", "#9ebcda", "#8856a7"},
						new[] {"#edf8fb", "#b3cde3", "#8c96c6", "#88419d"},
						new[] {"#edf8fb", "#b3cde3", "#8c96c6", "#8856a7", "#810f7c"},
						new[] {"#edf8fb", "#bfd3e6", "#9ebcda", "#8c96c6", "#8856a7", "#810f7c"},
						new[] {"#edf8fb", "#bfd3e6", "#9ebcda", "#8c96c6", "#8c6bb1", "#88419d", "#6e016b"},
						new[] {"#f7fcfd", "#e0ecf4", "#bfd3e6", "#9ebcda", "#8c96c6", "#8c6bb1", "#88419d", "#6e016b"},
						new[] {"#f7fcfd", "#e0ecf4", "#bfd3e6", "#9ebcda", "#8c96c6", "#8c6bb1", "#88419d", "#810f7c", "#4d004b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.RdPu,
					Label = "Red Purple",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fde0dd", "#fa9fb5", "#c51b8a"},
						new[] {"#feebe2", "#fbb4b9", "#f768a1", "#ae017e"},
						new[] {"#feebe2", "#fbb4b9", "#f768a1", "#c51b8a", "#7a0177"},
						new[] {"#feebe2", "#fcc5c0", "#fa9fb5", "#f768a1", "#c51b8a", "#7a0177"},
						new[] {"#feebe2", "#fcc5c0", "#fa9fb5", "#f768a1", "#dd3497", "#ae017e", "#7a0177"},
						new[] {"#fff7f3", "#fde0dd", "#fcc5c0", "#fa9fb5", "#f768a1", "#dd3497", "#ae017e", "#7a0177"},
						new[] {"#fff7f3", "#fde0dd", "#fcc5c0", "#fa9fb5", "#f768a1", "#dd3497", "#ae017e", "#7a0177", "#49006a"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PuRd,
					Label = "Purple Red",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e7e1ef", "#c994c7", "#dd1c77"},
						new[] {"#f1eef6", "#d7b5d8", "#df65b0", "#ce1256"},
						new[] {"#f1eef6", "#d7b5d8", "#df65b0", "#dd1c77", "#980043"},
						new[] {"#f1eef6", "#d4b9da", "#c994c7", "#df65b0", "#dd1c77", "#980043"},
						new[] {"#f1eef6", "#d4b9da", "#c994c7", "#df65b0", "#e7298a", "#ce1256", "#91003f"},
						new[] {"#f7f4f9", "#e7e1ef", "#d4b9da", "#c994c7", "#df65b0", "#e7298a", "#ce1256", "#91003f"},
						new[] {"#f7f4f9", "#e7e1ef", "#d4b9da", "#c994c7", "#df65b0", "#e7298a", "#ce1256", "#980043", "#67001f"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.OrRd,
					Label = "Orange Red",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fee8c8", "#fdbb84", "#e34a33"},
						new[] {"#fef0d9", "#fdcc8a", "#fc8d59", "#d7301f"},
						new[] {"#fef0d9", "#fdcc8a", "#fc8d59", "#e34a33", "#b30000"},
						new[] {"#fef0d9", "#fdd49e", "#fdbb84", "#fc8d59", "#e34a33", "#b30000"},
						new[] {"#fef0d9", "#fdd49e", "#fdbb84", "#fc8d59", "#ef6548", "#d7301f", "#990000"},
						new[] {"#fff7ec", "#fee8c8", "#fdd49e", "#fdbb84", "#fc8d59", "#ef6548", "#d7301f", "#990000"},
						new[] {"#fff7ec", "#fee8c8", "#fdd49e", "#fdbb84", "#fc8d59", "#ef6548", "#d7301f", "#b30000", "#7f0000"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.YlOrRd,
					Label = "Yellow Orange Red",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#ffeda0", "#feb24c", "#f03b20"},
						new[] {"#ffffb2", "#fecc5c", "#fd8d3c", "#e31a1c"},
						new[] {"#ffffb2", "#fecc5c", "#fd8d3c", "#f03b20", "#bd0026"},
						new[] {"#ffffb2", "#fed976", "#feb24c", "#fd8d3c", "#f03b20", "#bd0026"},
						new[] {"#ffffb2", "#fed976", "#feb24c", "#fd8d3c", "#fc4e2a", "#e31a1c", "#b10026"},
						new[] {"#ffffcc", "#ffeda0", "#fed976", "#feb24c", "#fd8d3c", "#fc4e2a", "#e31a1c", "#b10026"},
						new[] {"#ffffcc", "#ffeda0", "#fed976", "#feb24c", "#fd8d3c", "#fc4e2a", "#e31a1c", "#bd0026", "#800026"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.YlOrBr,
					Label = "Yellow Orange Brown",
					Type = Type.SequentialMultipleHues,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fff7bc", "#fec44f", "#d95f0e"},
						new[] {"#ffffd4", "#fed98e", "#fe9929", "#cc4c02"},
						new[] {"#ffffd4", "#fed98e", "#fe9929", "#d95f0e", "#993404"},
						new[] {"#ffffd4", "#fee391", "#fec44f", "#fe9929", "#d95f0e", "#993404"},
						new[] {"#ffffd4", "#fee391", "#fec44f", "#fe9929", "#ec7014", "#cc4c02", "#8c2d04"},
						new[] {"#ffffe5", "#fff7bc", "#fee391", "#fec44f", "#fe9929", "#ec7014", "#cc4c02", "#8c2d04"},
						new[] {"#ffffe5", "#fff7bc", "#fee391", "#fec44f", "#fe9929", "#ec7014", "#cc4c02", "#993404", "#662506"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Purples,
					Label = "Purples",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#efedf5", "#bcbddc", "#756bb1"},
						new[] {"#f2f0f7", "#cbc9e2", "#9e9ac8", "#6a51a3"},
						new[] {"#f2f0f7", "#cbc9e2", "#9e9ac8", "#756bb1", "#54278f"},
						new[] {"#f2f0f7", "#dadaeb", "#bcbddc", "#9e9ac8", "#756bb1", "#54278f"},
						new[] {"#f2f0f7", "#dadaeb", "#bcbddc", "#9e9ac8", "#807dba", "#6a51a3", "#4a1486"},
						new[] {"#fcfbfd", "#efedf5", "#dadaeb", "#bcbddc", "#9e9ac8", "#807dba", "#6a51a3", "#4a1486"},
						new[] {"#fcfbfd", "#efedf5", "#dadaeb", "#bcbddc", "#9e9ac8", "#807dba", "#6a51a3", "#54278f", "#3f007d"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Blues,
					Label = "Blues",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#deebf7", "#9ecae1", "#3182bd"},
						new[] {"#eff3ff", "#bdd7e7", "#6baed6", "#2171b5"},
						new[] {"#eff3ff", "#bdd7e7", "#6baed6", "#3182bd", "#08519c"},
						new[] {"#eff3ff", "#c6dbef", "#9ecae1", "#6baed6", "#3182bd", "#08519c"},
						new[] {"#eff3ff", "#c6dbef", "#9ecae1", "#6baed6", "#4292c6", "#2171b5", "#084594"},
						new[] {"#f7fbff", "#deebf7", "#c6dbef", "#9ecae1", "#6baed6", "#4292c6", "#2171b5", "#084594"},
						new[] {"#f7fbff", "#deebf7", "#c6dbef", "#9ecae1", "#6baed6", "#4292c6", "#2171b5", "#08519c", "#08306b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Greens,
					Label = "Greens",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e5f5e0", "#a1d99b", "#31a354"},
						new[] {"#edf8e9", "#bae4b3", "#74c476", "#238b45"},
						new[] {"#edf8e9", "#bae4b3", "#74c476", "#31a354", "#006d2c"},
						new[] {"#edf8e9", "#c7e9c0", "#a1d99b", "#74c476", "#31a354", "#006d2c"},
						new[] {"#edf8e9", "#c7e9c0", "#a1d99b", "#74c476", "#41ab5d", "#238b45", "#005a32"},
						new[] {"#f7fcf5", "#e5f5e0", "#c7e9c0", "#a1d99b", "#74c476", "#41ab5d", "#238b45", "#005a32"},
						new[] {"#f7fcf5", "#e5f5e0", "#c7e9c0", "#a1d99b", "#74c476", "#41ab5d", "#238b45", "#006d2c", "#00441b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Oranges,
					Label = "Oranges",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fee6ce", "#fdae6b", "#e6550d"},
						new[] {"#feedde", "#fdbe85", "#fd8d3c", "#d94701"},
						new[] {"#feedde", "#fdbe85", "#fd8d3c", "#e6550d", "#a63603"},
						new[] {"#feedde", "#fdd0a2", "#fdae6b", "#fd8d3c", "#e6550d", "#a63603"},
						new[] {"#feedde", "#fdd0a2", "#fdae6b", "#fd8d3c", "#f16913", "#d94801", "#8c2d04"},
						new[] {"#fff5eb", "#fee6ce", "#fdd0a2", "#fdae6b", "#fd8d3c", "#f16913", "#d94801", "#8c2d04"},
						new[] {"#fff5eb", "#fee6ce", "#fdd0a2", "#fdae6b", "#fd8d3c", "#f16913", "#d94801", "#a63603", "#7f2704"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Reds,
					Label = "Reds",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fee0d2", "#fc9272", "#de2d26"},
						new[] {"#fee5d9", "#fcae91", "#fb6a4a", "#cb181d"},
						new[] {"#fee5d9", "#fcae91", "#fb6a4a", "#de2d26", "#a50f15"},
						new[] {"#fee5d9", "#fcbba1", "#fc9272", "#fb6a4a", "#de2d26", "#a50f15"},
						new[] {"#fee5d9", "#fcbba1", "#fc9272", "#fb6a4a", "#ef3b2c", "#cb181d", "#99000d"},
						new[] {"#fff5f0", "#fee0d2", "#fcbba1", "#fc9272", "#fb6a4a", "#ef3b2c", "#cb181d", "#99000d"},
						new[] {"#fff5f0", "#fee0d2", "#fcbba1", "#fc9272", "#fb6a4a", "#ef3b2c", "#cb181d", "#a50f15", "#67000d"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Greys,
					Label = "Greys",
					Type = Type.SequentialSingleHue,
					HtmlCodes = new List<string[]>
					{
						new[] {"#f0f0f0", "#bdbdbd", "#636363"},
						new[] {"#f7f7f7", "#cccccc", "#969696", "#525252"},
						new[] {"#f7f7f7", "#cccccc", "#969696", "#636363", "#252525"},
						new[] {"#f7f7f7", "#d9d9d9", "#bdbdbd", "#969696", "#636363", "#252525"},
						new[] {"#f7f7f7", "#d9d9d9", "#bdbdbd", "#969696", "#737373", "#525252", "#252525"},
						new[] {"#ffffff", "#f0f0f0", "#d9d9d9", "#bdbdbd", "#969696", "#737373", "#525252", "#252525"},
						new[] {"#ffffff", "#f0f0f0", "#d9d9d9", "#bdbdbd", "#969696", "#737373", "#525252", "#252525", "#000000"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PuOr,
					Label = "Purple Orange",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#f1a340", "#f7f7f7", "#998ec3"},
						new[] {"#e66101", "#fdb863", "#b2abd2", "#5e3c99"},
						new[] {"#e66101", "#fdb863", "#f7f7f7", "#b2abd2", "#5e3c99"},
						new[] {"#b35806", "#f1a340", "#fee0b6", "#d8daeb", "#998ec3", "#542788"},
						new[] {"#b35806", "#f1a340", "#fee0b6", "#f7f7f7", "#d8daeb", "#998ec3", "#542788"},
						new[] {"#b35806", "#e08214", "#fdb863", "#fee0b6", "#d8daeb", "#b2abd2", "#8073ac", "#542788"},
						new[] {"#b35806", "#e08214", "#fdb863", "#fee0b6", "#f7f7f7", "#d8daeb", "#b2abd2", "#8073ac", "#542788"},
						new[] {"#7f3b08", "#b35806", "#e08214", "#fdb863", "#fee0b6", "#d8daeb", "#b2abd2", "#8073ac", "#542788", "#2d004b"},
						new[] {"#7f3b08", "#b35806", "#e08214", "#fdb863", "#fee0b6", "#f7f7f7", "#d8daeb", "#b2abd2", "#8073ac", "#542788", "#2d004b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.BrBG,
					Label = "Brown Blue Green",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#d8b365", "#f5f5f5", "#5ab4ac"},
						new[] {"#a6611a", "#dfc27d", "#80cdc1", "#018571"},
						new[] {"#a6611a", "#dfc27d", "#f5f5f5", "#80cdc1", "#018571"},
						new[] {"#8c510a", "#d8b365", "#f6e8c3", "#c7eae5", "#5ab4ac", "#01665e"},
						new[] {"#8c510a", "#d8b365", "#f6e8c3", "#f5f5f5", "#c7eae5", "#5ab4ac", "#01665e"},
						new[] {"#8c510a", "#bf812d", "#dfc27d", "#f6e8c3", "#c7eae5", "#80cdc1", "#35978f", "#01665e"},
						new[] {"#8c510a", "#bf812d", "#dfc27d", "#f6e8c3", "#f5f5f5", "#c7eae5", "#80cdc1", "#35978f", "#01665e"},
						new[] {"#543005", "#8c510a", "#bf812d", "#dfc27d", "#f6e8c3", "#c7eae5", "#80cdc1", "#35978f", "#01665e", "#003c30"},
						new[] {"#543005", "#8c510a", "#bf812d", "#dfc27d", "#f6e8c3", "#f5f5f5", "#c7eae5", "#80cdc1", "#35978f", "#01665e", "#003c30"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PRGn,
					Label = "Purple Green",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#af8dc3", "#f7f7f7", "#7fbf7b"},
						new[] {"#7b3294", "#c2a5cf", "#a6dba0", "#008837"},
						new[] {"#7b3294", "#c2a5cf", "#f7f7f7", "#a6dba0", "#008837"},
						new[] {"#762a83", "#af8dc3", "#e7d4e8", "#d9f0d3", "#7fbf7b", "#1b7837"},
						new[] {"#762a83", "#af8dc3", "#e7d4e8", "#f7f7f7", "#d9f0d3", "#7fbf7b", "#1b7837"},
						new[] {"#762a83", "#9970ab", "#c2a5cf", "#e7d4e8", "#d9f0d3", "#a6dba0", "#5aae61", "#1b7837"},
						new[] {"#762a83", "#9970ab", "#c2a5cf", "#e7d4e8", "#f7f7f7", "#d9f0d3", "#a6dba0", "#5aae61", "#1b7837"},
						new[] {"#40004b", "#762a83", "#9970ab", "#c2a5cf", "#e7d4e8", "#d9f0d3", "#a6dba0", "#5aae61", "#1b7837", "#00441b"},
						new[] {"#40004b", "#762a83", "#9970ab", "#c2a5cf", "#e7d4e8", "#f7f7f7", "#d9f0d3", "#a6dba0", "#5aae61", "#1b7837", "#00441b"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.PiYG,
					Label = "Pink Green",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e9a3c9", "#f7f7f7", "#a1d76a"},
						new[] {"#d01c8b", "#f1b6da", "#b8e186", "#4dac26"},
						new[] {"#d01c8b", "#f1b6da", "#f7f7f7", "#b8e186", "#4dac26"},
						new[] {"#c51b7d", "#e9a3c9", "#fde0ef", "#e6f5d0", "#a1d76a", "#4d9221"},
						new[] {"#c51b7d", "#e9a3c9", "#fde0ef", "#f7f7f7", "#e6f5d0", "#a1d76a", "#4d9221"},
						new[] {"#c51b7d", "#de77ae", "#f1b6da", "#fde0ef", "#e6f5d0", "#b8e186", "#7fbc41", "#4d9221"},
						new[] {"#c51b7d", "#de77ae", "#f1b6da", "#fde0ef", "#f7f7f7", "#e6f5d0", "#b8e186", "#7fbc41", "#4d9221"},
						new[] {"#8e0152", "#c51b7d", "#de77ae", "#f1b6da", "#fde0ef", "#e6f5d0", "#b8e186", "#7fbc41", "#4d9221", "#276419"},
						new[] {"#8e0152", "#c51b7d", "#de77ae", "#f1b6da", "#fde0ef", "#f7f7f7", "#e6f5d0", "#b8e186", "#7fbc41", "#4d9221", "#276419"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.RdBu,
					Label = "Red Blue",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#ef8a62", "#f7f7f7", "#67a9cf"},
						new[] {"#ca0020", "#f4a582", "#92c5de", "#0571b0"},
						new[] {"#ca0020", "#f4a582", "#f7f7f7", "#92c5de", "#0571b0"},
						new[] {"#b2182b", "#ef8a62", "#fddbc7", "#d1e5f0", "#67a9cf", "#2166ac"},
						new[] {"#b2182b", "#ef8a62", "#fddbc7", "#f7f7f7", "#d1e5f0", "#67a9cf", "#2166ac"},
						new[] {"#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#d1e5f0", "#92c5de", "#4393c3", "#2166ac"},
						new[] {"#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#f7f7f7", "#d1e5f0", "#92c5de", "#4393c3", "#2166ac"},
						new[] {"#67001f", "#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#d1e5f0", "#92c5de", "#4393c3", "#2166ac", "#053061"},
						new[] {"#67001f", "#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#f7f7f7", "#d1e5f0", "#92c5de", "#4393c3", "#2166ac", "#053061"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.RdGy,
					Label = "Red Grey",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#ef8a62", "#ffffff", "#999999"},
						new[] {"#ca0020", "#f4a582", "#bababa", "#404040"},
						new[] {"#ca0020", "#f4a582", "#ffffff", "#bababa", "#404040"},
						new[] {"#b2182b", "#ef8a62", "#fddbc7", "#e0e0e0", "#999999", "#4d4d4d"},
						new[] {"#b2182b", "#ef8a62", "#fddbc7", "#ffffff", "#e0e0e0", "#999999", "#4d4d4d"},
						new[] {"#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#e0e0e0", "#bababa", "#878787", "#4d4d4d"},
						new[] {"#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#ffffff", "#e0e0e0", "#bababa", "#878787", "#4d4d4d"},
						new[] {"#67001f", "#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#e0e0e0", "#bababa", "#878787", "#4d4d4d", "#1a1a1a"},
						new[] {"#67001f", "#b2182b", "#d6604d", "#f4a582", "#fddbc7", "#ffffff", "#e0e0e0", "#bababa", "#878787", "#4d4d4d", "#1a1a1a"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.RdYlBu,
					Label = "Red Yellow Blue",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fc8d59", "#ffffbf", "#91bfdb"},
						new[] {"#d7191c", "#fdae61", "#abd9e9", "#2c7bb6"},
						new[] {"#d7191c", "#fdae61", "#ffffbf", "#abd9e9", "#2c7bb6"},
						new[] {"#d73027", "#fc8d59", "#fee090", "#e0f3f8", "#91bfdb", "#4575b4"},
						new[] {"#d73027", "#fc8d59", "#fee090", "#ffffbf", "#e0f3f8", "#91bfdb", "#4575b4"},
						new[] {"#d73027", "#f46d43", "#fdae61", "#fee090", "#e0f3f8", "#abd9e9", "#74add1", "#4575b4"},
						new[] {"#d73027", "#f46d43", "#fdae61", "#fee090", "#ffffbf", "#e0f3f8", "#abd9e9", "#74add1", "#4575b4"},
						new[] {"#a50026", "#d73027", "#f46d43", "#fdae61", "#fee090", "#e0f3f8", "#abd9e9", "#74add1", "#4575b4", "#313695"},
						new[] {"#a50026", "#d73027", "#f46d43", "#fdae61", "#fee090", "#ffffbf", "#e0f3f8", "#abd9e9", "#74add1", "#4575b4", "#313695"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Spectral,
					Label = "Spectral",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fc8d59", "#ffffbf", "#99d594"},
						new[] {"#d7191c", "#fdae61", "#abdda4", "#2b83ba"},
						new[] {"#d7191c", "#fdae61", "#ffffbf", "#abdda4", "#2b83ba"},
						new[] {"#d53e4f", "#fc8d59", "#fee08b", "#e6f598", "#99d594", "#3288bd"},
						new[] {"#d53e4f", "#fc8d59", "#fee08b", "#ffffbf", "#e6f598", "#99d594", "#3288bd"},
						new[] {"#d53e4f", "#f46d43", "#fdae61", "#fee08b", "#e6f598", "#abdda4", "#66c2a5", "#3288bd"},
						new[] {"#d53e4f", "#f46d43", "#fdae61", "#fee08b", "#ffffbf", "#e6f598", "#abdda4", "#66c2a5", "#3288bd"},
						new[] {"#9e0142", "#d53e4f", "#f46d43", "#fdae61", "#fee08b", "#e6f598", "#abdda4", "#66c2a5", "#3288bd", "#5e4fa2"},
						new[] {"#9e0142", "#d53e4f", "#f46d43", "#fdae61", "#fee08b", "#ffffbf", "#e6f598", "#abdda4", "#66c2a5", "#3288bd", "#5e4fa2"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.RdYlGn,
					Label = "Red Yellow Green",
					Type = Type.Diverging,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fc8d59", "#ffffbf", "#91cf60"},
						new[] {"#d7191c", "#fdae61", "#a6d96a", "#1a9641"},
						new[] {"#d7191c", "#fdae61", "#ffffbf", "#a6d96a", "#1a9641"},
						new[] {"#d73027", "#fc8d59", "#fee08b", "#d9ef8b", "#91cf60", "#1a9850"},
						new[] {"#d73027", "#fc8d59", "#fee08b", "#ffffbf", "#d9ef8b", "#91cf60", "#1a9850"},
						new[] {"#d73027", "#f46d43", "#fdae61", "#fee08b", "#d9ef8b", "#a6d96a", "#66bd63", "#1a9850"},
						new[] {"#d73027", "#f46d43", "#fdae61", "#fee08b", "#ffffbf", "#d9ef8b", "#a6d96a", "#66bd63", "#1a9850"},
						new[] {"#a50026", "#d73027", "#f46d43", "#fdae61", "#fee08b", "#d9ef8b", "#a6d96a", "#66bd63", "#1a9850", "#006837"},
						new[] {"#a50026", "#d73027", "#f46d43", "#fdae61", "#fee08b", "#ffffbf", "#d9ef8b", "#a6d96a", "#66bd63", "#1a9850", "#006837"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Accent,
					Label = "Accent",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#7fc97f", "#beaed4", "#fdc086"},
						new[] {"#7fc97f", "#beaed4", "#fdc086", "#ffff99"},
						new[] {"#7fc97f", "#beaed4", "#fdc086", "#ffff99", "#386cb0"},
						new[] {"#7fc97f", "#beaed4", "#fdc086", "#ffff99", "#386cb0", "#f0027f"},
						new[] {"#7fc97f", "#beaed4", "#fdc086", "#ffff99", "#386cb0", "#f0027f", "#bf5b17"},
						new[] {"#7fc97f", "#beaed4", "#fdc086", "#ffff99", "#386cb0", "#f0027f", "#bf5b17", "#666666"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Dark2,
					Label = "Dark",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#1b9e77", "#d95f02", "#7570b3"},
						new[] {"#1b9e77", "#d95f02", "#7570b3", "#e7298a"},
						new[] {"#1b9e77", "#d95f02", "#7570b3", "#e7298a", "#66a61e"},
						new[] {"#1b9e77", "#d95f02", "#7570b3", "#e7298a", "#66a61e", "#e6ab02"},
						new[] {"#1b9e77", "#d95f02", "#7570b3", "#e7298a", "#66a61e", "#e6ab02", "#a6761d"},
						new[] {"#1b9e77", "#d95f02", "#7570b3", "#e7298a", "#66a61e", "#e6ab02", "#a6761d", "#666666"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Paired,
					Label = "Paired",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#a6cee3", "#1f78b4", "#b2df8a"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f", "#ff7f00"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f", "#ff7f00", "#cab2d6"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f", "#ff7f00", "#cab2d6", "#6a3d9a"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f", "#ff7f00", "#cab2d6", "#6a3d9a", "#ffff99"},
						new[] {"#a6cee3", "#1f78b4", "#b2df8a", "#33a02c", "#fb9a99", "#e31a1c", "#fdbf6f", "#ff7f00", "#cab2d6", "#6a3d9a", "#ffff99", "#b15928"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Pastel1,
					Label = "Pastel 1",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4", "#fed9a6"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4", "#fed9a6", "#ffffcc"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4", "#fed9a6", "#ffffcc", "#e5d8bd"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4", "#fed9a6", "#ffffcc", "#e5d8bd", "#fddaec"},
						new[] {"#fbb4ae", "#b3cde3", "#ccebc5", "#decbe4", "#fed9a6", "#ffffcc", "#e5d8bd", "#fddaec", "#f2f2f2"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Pastel2,
					Label = "Pastel 2",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8"},
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8", "#f4cae4"},
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8", "#f4cae4", "#e6f5c9"},
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8", "#f4cae4", "#e6f5c9", "#fff2ae"},
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8", "#f4cae4", "#e6f5c9", "#fff2ae", "#f1e2cc"},
						new[] {"#b3e2cd", "#fdcdac", "#cbd5e8", "#f4cae4", "#e6f5c9", "#fff2ae", "#f1e2cc", "#cccccc"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Set1,
					Label = "Set 1",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#e41a1c", "#377eb8", "#4daf4a"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00", "#ffff33"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00", "#ffff33", "#a65628"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00", "#ffff33", "#a65628", "#f781bf"},
						new[] {"#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00", "#ffff33", "#a65628", "#f781bf", "#999999"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Set2,
					Label = "Set 2",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#66c2a5", "#fc8d62", "#8da0cb"},
						new[] {"#66c2a5", "#fc8d62", "#8da0cb", "#e78ac3"},
						new[] {"#66c2a5", "#fc8d62", "#8da0cb", "#e78ac3", "#a6d854"},
						new[] {"#66c2a5", "#fc8d62", "#8da0cb", "#e78ac3", "#a6d854", "#ffd92f"},
						new[] {"#66c2a5", "#fc8d62", "#8da0cb", "#e78ac3", "#a6d854", "#ffd92f", "#e5c494"},
						new[] {"#66c2a5", "#fc8d62", "#8da0cb", "#e78ac3", "#a6d854", "#ffd92f", "#e5c494", "#b3b3b3"}
					}
				});
			Palettes.Add(new Palette
				{
					Code = Code.Set3,
					Label = "Set 3",
					Type = Type.Qualitative,
					HtmlCodes = new List<string[]>
					{
						new[] {"#8dd3c7", "#ffffb3", "#bebada"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69", "#fccde5"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69", "#fccde5", "#d9d9d9"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69", "#fccde5", "#d9d9d9", "#bc80bd"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69", "#fccde5", "#d9d9d9", "#bc80bd", "#ccebc5"},
						new[] {"#8dd3c7", "#ffffb3", "#bebada", "#fb8072", "#80b1d3", "#fdb462", "#b3de69", "#fccde5", "#d9d9d9", "#bc80bd", "#ccebc5", "#ffed6f"}
					}
				});
		}

		public static List<string[]> GetHtmlCodes(Code code, bool reversed = false)
		{
			var palletteCodes = Palettes.Single(c => c.Code == code).HtmlCodes;
			if (reversed)
			{
				var reversedPalletteCodes = new List<string[]>();
				foreach (var codes in palletteCodes)
					reversedPalletteCodes.Add(codes.AsEnumerable().Reverse().ToArray());
				palletteCodes = reversedPalletteCodes;
			}
			return palletteCodes;
		}

		public static string[] GetHtmlCodes(Code code, byte numberOfColors, bool reversed = false)
		{
			var codes = Palettes.Single(c => c.Code == code).HtmlCodes.Single(p => p.Length == numberOfColors);
			if (reversed) codes = codes.Reverse().ToArray();
			return codes;
		}

//		public static List<Color[]> GetColors(Code code, bool reversed = false)
//		{
//			var colors = new List<Color[]>();
//			foreach (var range in GetHtmlCodes(code, reversed).AsEnumerable())
//			{
//				var c = new Color[range.Length];
//				for (var i = 0; i < range.Length; i++)
//					c[i] = ColorTranslator.FromHtml(range[i]);
//				colors.Add(c);
//			}
//			return colors;
//		}

//		public static Color[] GetColors(Code code, byte numberOfColors, bool reversed = false)
//		{
//			var colors = new List<Color>();
//			foreach (var color in GetHtmlCodes(code, numberOfColors))
//				colors.Add(ColorTranslator.FromHtml(color));
//			return colors.ToArray();
//		}
//
//		public static List<Color[]> GetColors(string colorConfig, Code defaultColorCode = Code.Set3, bool reversed = false)
//		{
//			var code = defaultColorCode;
//			if (colorConfig.Contains(":"))
//			{
//				var configs = colorConfig.Split(':');
//				var codeConfig = configs[0];
//				if (Enum.IsDefined(typeof(Code), codeConfig))
//				{
//					code = (Code)Enum.Parse(typeof(Code), codeConfig, true);
//					if (configs.Length == 3 && configs[2] == "1") reversed = true;
//				}
//			}
//			return GetColors(code, reversed);
//		}

		public class Palette
		{
			public Code Code;
			public string Label;
			public Type Type;
			public List<string[]> HtmlCodes;
		};

		public enum Type
		{
			SequentialMultipleHues,
			SequentialSingleHue,
			Diverging,
			Qualitative
		}
	}
}