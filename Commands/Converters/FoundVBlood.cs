using System.Collections.Generic;
using System.Linq;
using Refined.Data;
using Stunlock.Core;
using VampireCommandFramework;

namespace Refined.Commands.Converters;
public record struct FoundVBlood(PrefabGUID Value, string Name);

public class FoundVBloodConverter : CommandArgumentConverter<FoundVBlood>
{
	public readonly static Dictionary<string, PrefabGUID> NameToVBloodPrefab = new()
	{
		{"Matka", Prefabs.CHAR_Cursed_Witch_VBlood },
		{"Terah", Prefabs.CHAR_Geomancer_Human_VBlood },
		{"Jade", Prefabs.CHAR_VHunter_Jade_VBlood    },
		{"Beatrice", Prefabs.CHAR_Villager_Tailor_VBlood },
		{"Nicholaus", Prefabs.CHAR_Undead_Priest_VBlood   },
		{"Quincey (Quincy)", Prefabs.CHAR_Bandit_Tourok_VBlood   },
		{"Ungora (Spider)", Prefabs.CHAR_Spider_Queen_VBlood    },
		{"Terrorclaw", Prefabs.CHAR_Winter_Yeti_VBlood },
		{"Lidia", Prefabs.CHAR_Bandit_Chaosarrow_VBlood   },
		{"Goreswine", Prefabs.CHAR_Undead_BishopOfDeath_VBlood    },
		{"Octavian", Prefabs.CHAR_Militia_Leader_VBlood  },
		{"Leandra", Prefabs.CHAR_Undead_BishopOfShadows_VBlood  },
		{"Rufus", Prefabs.CHAR_Bandit_Foreman_VBlood  },
		{"Keely", Prefabs.CHAR_Bandit_Frostarrow_VBlood   },
		{"Kodia the Ferocious Bear", Prefabs.CHAR_Forest_Bear_Dire_Vblood  },
		{"Christina", Prefabs.CHAR_Militia_Nun_VBlood   },
		{"Clive", Prefabs.CHAR_Bandit_Bomber_VBlood   },
		{"Gorecrusher the Behemoth", Prefabs.CHAR_Cursed_MountainBeast_VBlood    },
		{"Foulrot", Prefabs.CHAR_Undead_ZealousCultist_VBlood   },
		{"Polora", Prefabs.CHAR_Poloma_VBlood },
		{"Styx (Bat)", Prefabs.CHAR_BatVampire_VBlood  },
		{"Mairwyn the Elementalist", Prefabs.CHAR_ArchMage_VBlood    },
		{"Albert The Duke of Balaton (Frog)", Prefabs.CHAR_Cursed_ToadKing_VBlood },
		{"Talzur The Winged Horror", Prefabs.CHAR_Manticore_VBlood   },
		{"Vincent", Prefabs.CHAR_Militia_Guard_VBlood   },
		{"Raziel", Prefabs.CHAR_Militia_BishopOfDunley_VBlood  },
		{"Morian", Prefabs.CHAR_Harpy_Matriarch_VBlood },
		{"Solarus", Prefabs.CHAR_ChurchOfLight_Paladin_VBlood },
		{"Tristan", Prefabs.CHAR_VHunter_Leader_VBlood  },
		{"Errol", Prefabs.CHAR_Bandit_StoneBreaker_VBlood },
		{"Azariel", Prefabs.CHAR_ChurchOfLight_Cardinal_VBlood   },
		{"Willfred", Prefabs.CHAR_WerewolfChieftain_Human },
		{"Alpha Wolf", Prefabs.CHAR_Forest_Wolf_VBlood },
		{"Meredith", Prefabs.CHAR_Militia_Longbowman_LightArrow_Vblood   },
		{"Nibbles the Putrid Rat", Prefabs.CHAR_Vermin_DireRat_VBlood  },
		{"Frostmaw", Prefabs.CHAR_Wendigo_VBlood },
		{"Grayson", Prefabs.CHAR_Bandit_Stalker_VBlood  },
		{"Adam", Prefabs.CHAR_Gloomrot_Monster_VBlood },
		{"Voltatia", Prefabs.CHAR_Gloomrot_RailgunSergeant_VBlood    },
		{"Ziva", Prefabs.CHAR_Gloomrot_Iva_VBlood },
		{"Angram", Prefabs.CHAR_Gloomrot_Purifier_VBlood    },
		{"Henry Blackbrew", Prefabs.CHAR_Gloomrot_TheProfessor_VBlood    },
		{"Domina", Prefabs.CHAR_Gloomrot_Voltage_VBlood },
		{"Cyril", Prefabs.CHAR_Undead_CursedSmith_VBlood   },
		{"Ben The Old Wanderer", Prefabs.CHAR_Villager_CursedWanderer_VBlood  },
		{"Baron du Bouchon", Prefabs.CHAR_ChurchOfLight_Sommelier_VBlood  },
		{"Sir Magnus the Overseer", Prefabs.CHAR_ChurchOfLight_Overseer_VBlood   },
		{"Maja", Prefabs.CHAR_Militia_Scribe_VBlood   },
		{"Bane", Prefabs.CHAR_Undead_Infiltrator_VBlood   },
		{"Grethel the Glassblower", Prefabs.CHAR_Militia_Glassblower_VBlood  },
		{"Kriig", Prefabs.CHAR_Undead_Leader_Vblood  },
		{"Finn", Prefabs.CHAR_Bandit_Fisherman_VBlood },
		{"Elena", Prefabs.CHAR_Vampire_IceRanger_VBlood },
		{"Valencia", Prefabs.CHAR_Vampire_BloodKnight_VBlood },
		{"Cassius", Prefabs.CHAR_Vampire_HighLord_VBlood },
		{"Dracula", Prefabs.CHAR_Vampire_Dracula_VBlood },
		{"Simon Belmont", Prefabs.CHAR_VHunter_CastleMan }
	};

	public readonly static Dictionary<PrefabGUID, string> VBloodPrefabToName = NameToVBloodPrefab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

	public override FoundVBlood Parse(ICommandContext ctx, string input)
	{
		if (Core.PrefabService.TryGetItem(input, out var prefab) && VBloodPrefabToName.TryGetValue(prefab, out var name))
		{
			return new FoundVBlood(prefab, name);
		}

		var matches = NameToVBloodPrefab.Where(kvp => kvp.Key.ToLower().Replace(" ", "").Contains(input.Replace(" ", "").ToLower()));

		if (matches.Count() == 1)
		{
			var theMatch = matches.First();

			return new FoundVBlood(theMatch.Value, theMatch.Key);
		}

		if (matches.Count() > 1)
		{
			throw ctx.Error($"Multiple bosses found matching {input}. Please be more specific.\n" + string.Join("\n", matches.Select(x => x.Key)));
		}

		throw ctx.Error("Could not find boss");
	}

	static public bool Parse(string input, out FoundVBlood foundVBlood)
	{
		var matches = NameToVBloodPrefab.Where(kvp => kvp.Key.ToLower().Replace(" ", "").Contains(input.Replace(" ", "").ToLower()));

		if (matches.Count() == 1)
		{
			var theMatch = matches.First();

			foundVBlood = new FoundVBlood(theMatch.Value, theMatch.Key);

			return true;
		}

		foundVBlood = new FoundVBlood();

		return false;
	}
}
