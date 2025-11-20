using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace ContactNotes;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class ContactNotes : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "ContactNotes";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/ResoniteContactNotes-RML";

	public override void OnEngineInit() {
		Harmony harmony = new("com.__Choco__.ContactNotes");
		harmony.PatchAll();
	}

	
	[HarmonyPatch(typeof(ContactsDialog), "OnAttach")]
	class ContactsDialog_OnAttach_InjectComponents {
		static void Postfix(ContactsDialog __instance) {
			Msg("Postfix from ContactNotes");
		}
	}
}
