namespace RainingBlood {
	public class WeatherDefExtension : Verse.DefModExtension {
		public RimWorld.ThoughtDef exposedThoughtCannibal;
	}

	[Verse.StaticConstructorOnStartup]
	public static class Patch {
		static Patch() {
			// Get the method we want to patch.
			var m = typeof(Verse.AI.Pawn_MindState).GetMethod("MindStateTick");

			// Get the method we want to run after the original.
			var post = typeof(RainingBlood.Patch).GetMethod("PostMindStateTick",
				System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public);

			// Patch stuff! The string passed to the Harmony constructor can be
			// anything, and can be used to identify/remove patches if need be.
			new HarmonyLib.Harmony("arp242.rainingblood").Patch(m,
				postfix: new HarmonyLib.HarmonyMethod(post));
		}

		// The special __instance parameter has the original class instance
		// we're extending. This is based on the argument name.
		public static void PostMindStateTick(Verse.AI.Pawn_MindState __instance) {
			var pawn = __instance.pawn;

			// Same condition as MindStateTick, but inversed for early return.
			if (Verse.Find.TickManager.TicksGame % 123 != 0 ||
				!pawn.Spawned || !pawn.RaceProps.IsFlesh || pawn.needs.mood == null)
				return;

			// Is this pawn a cannibal? If not, then there's nothing to do. You
			// can also expand this by checking for the Ideology cannibalism
			// memes, but this just checks the "cannibalism" trait on colonists.
			if (!pawn.story.traits.HasTrait(RimWorld.TraitDefOf.Cannibal))
				return;

			// Let's see if the current weather has our new exposedThoughtCannibal.
			var w = pawn.Map.weatherManager.CurWeatherLerped;
			if (!w.HasModExtension<WeatherDefExtension>())
				return;
			var t = w.GetModExtension<WeatherDefExtension>().exposedThoughtCannibal;
			if (t == null)
				return;

			// Remove any existing thought that was applied and apply our
			// cannibalistic thoughts.
			if (w.exposedThought != null)
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(w.exposedThought);
			pawn.needs.mood.thoughts.memories.TryGainMemoryFast(t);
		}
	}
}
