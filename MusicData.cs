using Terraria;

internal class MusicData
{
	internal int music;
	internal int musicbox;
	internal string mod;
	internal string name;

	public MusicData(int music, int musicbox, string mod = "Terraria", string name = "Unknown")
	{
		this.music = music;
		this.musicbox = musicbox;
		this.mod = mod;
		if (mod == "Terraria") {
			string itemNameValue = Lang.GetItemNameValue(musicbox);
			this.name = itemNameValue.Substring(itemNameValue.IndexOf("(") + 1).Replace(")", "");
			// TODO: [1.4] Add check for Otherworldly music boxes
		}
		else {
			this.name = name;
		}
	}
}
