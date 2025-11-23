using System.Collections;
using System.Text.Json;

using FrooxEngine;
using FrooxEngine.UIX;

using HarmonyLib;

using ResoniteModLoader;

using SkyFrost.Base;

namespace ContactNotes;
public class ContactNotes : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "ContactNotes";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/ResoniteContactNotes-RML";

	private const string defaultText = "You can write notes in here when you are focused on a contact!\nThey are persistent, but don't sync across devices.";

	private const string contactsSaveLocation = "./ContactNotes/Contacts.json";

	private static TextField notesField;

	private static string focusedContact;

	private static bool loaded = false;

	private static Dictionary<string, string> messages;

	public override void OnEngineInit() {
		Harmony harmony = new("com.__Choco__.ContactNotes");
		harmony.PatchAll();

		//Currently all syncing is done locally on a json file
		//this is secure and works well, but people will probably be lookign for cloud var syncing
		//Feel free to PR this, just please include a setting to disable it for security reasons.
		if (File.Exists(contactsSaveLocation))
			messages = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(contactsSaveLocation));

		if (messages is null)
			messages = new Dictionary<string, string>();
	}
	 

	[HarmonyPatch(typeof(ContactsDialog), "OnAttach")]
	class ContactsDialog_OnAttach_InjectComponents {
		static void Postfix(ContactsDialog __instance) {
			FrooxEngine.Userspace.UserspaceWorld.RunInUpdates(1, () => {
				Traverse<UIBuilder> sessionsUiField = Traverse.Create(__instance).Field<UIBuilder>("sessionsUi");
				UIBuilder sessionsUi = sessionsUiField.Value;
				RectTransform newSessionsSpace;
				RectTransform newNotesSpace;
				//Not quite sure why the proportions are wrong
				//Please PR if you figure out how to make the notes text box larger
				sessionsUi.SplitVertically(.25f, out newNotesSpace, out newSessionsSpace);
				UIBuilder newSessionsUi = new UIBuilder(newSessionsSpace);
				UIBuilder newNotesUi = new UIBuilder(newNotesSpace);
				
				notesField = newNotesUi.TextField(defaultText, undo: true, parseRTF: true);
				
				focusedContact = "null";
				__instance.Engine.OnShutdown += onShutdownSyncText;
			});
			loaded = true;
			Msg("ContactNotes Loaded.");//Moved the loaded message down here because this is where it really finishes loading.
		}

		static void onShutdownSyncText() {
			Msg("Delegate from ContactNotes: AutoSaving current contact.");
			messages[focusedContact] = notesField.TargetString;
			Directory.CreateDirectory("./ContactNotes");
			FileStream saveFile = File.Create(contactsSaveLocation);
			string notesJSON = JsonSerializer.Serialize(messages);
			byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(notesJSON);
			saveFile.Write(fileBytes);
			saveFile.Close();
			Msg("AutoSave Complete!");
		}
	}

	[HarmonyPatch(typeof(ContactsDialog), "UpdateSelectedContact")]
	class ContactsDialog_SelectedContact_OnChanged {
		public static void Postfix(ContactsDialog __instance) {
			if (!loaded)
				return;
			messages[focusedContact] = notesField.TargetString;
			focusedContact = __instance is null ? "null" : __instance.SelectedContact.ContactUserId;
			
			if (!messages.ContainsKey(focusedContact))
				messages.Add(focusedContact, "");

			notesField.TargetString = messages[focusedContact];
		}
	}
}
