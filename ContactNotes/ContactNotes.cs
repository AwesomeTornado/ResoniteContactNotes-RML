using FrooxEngine;
using FrooxEngine.UIX;

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

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<float> NotesSize = new ModConfigurationKey<float>("Notes size", "Change the size of the notes text box.", () => .5f);

	private const string defaultText = "You can write notes in here when you are focused on a contact!\nThey are persistent, and will sync across devices.\nFor additional privacy, at the cost of losing multi device syncing, turn on \"Secure Mode\" in the settings.";

	private static TextField notesField;

	private static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration();
		Harmony harmony = new("com.__Choco__.ContactNotes");
		harmony.PatchAll();
	}

	
	[HarmonyPatch(typeof(ContactsDialog), "OnAttach")]
	class ContactsDialog_OnAttach_InjectComponents {
		static void Postfix(ContactsDialog __instance) {
			Msg("Postfix from ContactNotes: OnAttach");//TODO: Remove for release
			Traverse<UIBuilder> sessionsUiField = Traverse.Create(__instance).Field<UIBuilder>("sessionsUi");
			UIBuilder sessionsUi = sessionsUiField.Value;
			RectTransform newSessionsSpace;
			RectTransform newNotesSpace;
			sessionsUi.SplitVertically(Config.GetValue(NotesSize), out newNotesSpace, out newSessionsSpace);
			UIBuilder newSessionsUi = new UIBuilder(newSessionsSpace);
			UIBuilder newNotesUi = new UIBuilder(newNotesSpace);
			notesField = newNotesUi.TextField(defaultText, undo: true, parseRTF: true);
		}
	}

	[HarmonyPatch(typeof(ContactsDialog), "SelectedContact", MethodType.Setter)]
	class ContactsDialog_SelectedContact_OnChanged{
		static void Postfix(ContactsDialog __instance) {
			Msg("Postfix from ContactNotes: OnAttach");//TODO: Remove for release
			Traverse<UIBuilder> sessionsUiField = Traverse.Create(__instance).Field<UIBuilder>("sessionsUi");
			UIBuilder sessionsUi = sessionsUiField.Value;
			RectTransform newSessionsSpace;
			RectTransform newNotesSpace;
			sessionsUi.SplitVertically(Config.GetValue(NotesSize), out newNotesSpace, out newSessionsSpace);
			UIBuilder newSessionsUi = new UIBuilder(newSessionsSpace);
			UIBuilder newNotesUi = new UIBuilder(newNotesSpace);
			
		}
	}
}
