using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class AntichamberModule : MonoBehaviour
{
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMSelectable GunButton;
	public KMSelectable ArrowLeft;
	public KMSelectable ArrowRight;
	public KMSelectable Submit;
	public KMAudio KMAudio;
	public TextMesh RoomLabel;
	public GameObject Gun;
	public Sprite[] guns;
	public string[] rooms;
	bool? unicornRule;
	bool? lastDigitRule;
	bool? doubleOhRule;
	bool? duplicateRule;

    static int ModuleIdCounter = 1;
    int ModuleId;

	int gun = 0;
	int roomindex = 0;
	int determine = 0;

    readonly string[] gunColors = new string[] {"Blue", "Green", "Yellow", "Red" };

    private int AdjustNumber(int value, int adjustWith)
	{
		if (value <= 0)
		{
			while (value <= 0)
			{
				value += adjustWith;
			}
		}
		else if (value > adjustWith)
		{
			while (value > adjustWith)
			{
				value -= adjustWith;
			}
		}

		return value;
	}

	void Awake()
	{
		ModuleId = ModuleIdCounter++;
    }

    void Start()
	{
        gun = UnityEngine.Random.Range(0, 4);
		roomindex = UnityEngine.Random.Range(0, 13);
		GunButton.OnInteract += GunButtonCode;
		ArrowLeft.OnInteract += LeftButtonCode;
		ArrowRight.OnInteract += RightButtonCode;

        Submit.OnInteract += SubmitEvent;
		Gun.GetComponent<SpriteRenderer>().sprite = guns[gun];
		RoomLabel.text = rooms[roomindex];

		//Count Vanilla Ports
        int vanillaports = new Port[] { Port.Parallel, Port.Serial, Port.DVI, Port.StereoRCA, Port.RJ45, Port.PS2 }.Sum(p => BombInfo.GetPortCount(p));

        //Count Modded Ports
        int moddedports = new Port[] { Port.CompositeVideo, Port.ComponentVideo, Port.USB, Port.HDMI, Port.VGA, Port.AC, Port.PCMCIA }.Sum(p => BombInfo.GetPortCount(p));

		int holders = BombInfo.GetBatteryHolderCount();
		int batteries = BombInfo.GetBatteryCount();

		determine = (vanillaports - moddedports) * (batteries + holders);
		determine = this.AdjustNumber(determine, 4);

        Log(string.Format("Vanilla Ports: {0}", vanillaports));
		Log(string.Format("Modded Ports: {0}", moddedports));
		Log(string.Format("Batteries: {0}", batteries));
		Log(string.Format("Battery holders: {0}", holders));
	}

	protected bool GunButtonCode()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, GunButton.transform);
		GunButton.AddInteractionPunch();
		gun++;
		if (gun >= 4) gun = 0;
		Gun.GetComponent<SpriteRenderer>().sprite = guns[gun];
		return false;
	}

	protected bool LeftButtonCode()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ArrowLeft.transform);
		ArrowLeft.AddInteractionPunch();
		roomindex--;
		if (roomindex < 0) roomindex = 12;
		RoomLabel.text = rooms[roomindex];
		return false;
	}

	protected bool RightButtonCode()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ArrowRight.transform);
		ArrowRight.AddInteractionPunch();
		roomindex++;
		if (roomindex > 12) roomindex = 0;
		RoomLabel.text = rooms[roomindex];
		return false;
	}

	protected bool SubmitEvent()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Submit.transform);
		Submit.AddInteractionPunch();
		string correctRoom = GetCorrectRoom().ToLower();
		string expectedRoom = rooms[roomindex].ToLower();

        Log(string.Format("Submitted the {0} gun in {1}", gunColors[gun], expectedRoom));
		Log(string.Format("Expected the {0} gun in {1}", gunColors[determine - 1], correctRoom));



		if (gunColors[determine - 1] == gunColors[gun] && expectedRoom == correctRoom)
			ModuleCompleted();

		else
            ModuleFail();

		return false;
	}

	private string GetCorrectRoom()
	{
		string serialNumber = BombInfo.GetSerialNumber();

        if (unicornRule == null)
			unicornRule = new char[] { '1', '3' }.All(c => serialNumber.Contains(c));

        if ((bool)unicornRule)
			return new string[] { "Logic 101", "Learning To Draw", "I Like To Move It", "I Can Do Anything" }[determine - 1];

        string[] solvedModuleNames = BombInfo.GetSolvedModuleNames().ToArray();

        if (solvedModuleNames.Length >= 3)
			return "Climbing The Tower";

		if(lastDigitRule == null)
			lastDigitRule = BombInfo.GetSerialNumberNumbers().Last() > 7;

        if ((bool)lastDigitRule)
			return "The Highest Point";

		if (new string[] { "3D Maze", "3D Tunnels", "Mouse In The Maze", "Maze", "Morse-A-Maze" }.Any(name => solvedModuleNames.Contains(name)))
			return "Impossible Paths";

		if (BombInfo.GetStrikes() > 0)
			return "Failing Forward";

        string[] solveableModuleNames = BombInfo.GetSolvableModuleNames().ToArray();
        string[] unsolvedModuleNames = solveableModuleNames.Except(solvedModuleNames).ToArray();

        if (new string[] { "Password", "Extended Password", "Binary Puzzle", "Symbolic Password" }.Any(name => unsolvedModuleNames.Contains(name)))
			return "Connecting The Pieces";

		if (solvedModuleNames.Length == 0)
			return "Taking Baby Steps";

		if (doubleOhRule == null)
			doubleOhRule = new string[] { "Double-Oh", "Cursed Double-Oh" }.Any(name => solveableModuleNames.Contains(name));

		if ((bool)doubleOhRule)
			return "Three Paths Of Sight";

		if (duplicateRule == null)
			duplicateRule = solveableModuleNames.GroupBy(x => x).Any(group => group.Count() > 1);

        if ((bool)duplicateRule)
            return "Deja Vu";

		return "Window Of Opportunity";
    }

	protected bool FailHandlePassGun()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, GunButton.transform);
		GunButton.AddInteractionPunch();
		return false;
	}

	protected bool FailHandlePassLeft()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ArrowLeft.transform);
		ArrowLeft.AddInteractionPunch();
		return false;
	}

	protected bool FailHandlePassRight()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ArrowRight.transform);
		ArrowRight.AddInteractionPunch();
		return false;
	}

	protected bool FailHandlePassSubmit()
	{
		KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Submit.transform);
		Submit.AddInteractionPunch();
		return false;
	}

	protected bool ModuleCompleted()
	{
		BombModule.HandlePass();
		GunButton.OnInteract = FailHandlePassGun;
		ArrowLeft.OnInteract = FailHandlePassLeft;
		ArrowRight.OnInteract = FailHandlePassRight;
		Submit.OnInteract = FailHandlePassSubmit;
		return false;
	}

	protected bool ModuleFail()
	{
		BombModule.HandleStrike();
		return false;
	}

	private void Log(string s)
	{ 
		Debug.LogFormat("[Antichamber #{0}] {1}", ModuleId, s);
    }

	//twitch plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} gun <red/blue/green/yellow> [Selects the specified gun] | !{0} room <room> [Selects the specified room] | !{0} submit [Presses the submit button]";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			Submit.OnInteract();
			yield break;
		}
		if (Regex.IsMatch(parameters[0], @"^\s*gun\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length == 2)
			{
				string[] gunTypes = { "blue", "green", "yellow", "red" };
				if (gunTypes.Contains(parameters[1].ToLower()))
				{
					yield return null;
					while (gun != Array.IndexOf(gunTypes, parameters[1].ToLower()))
					{
						GunButton.OnInteract();
						yield return new WaitForSeconds(0.1f);
					}
				}
			}
			yield break;
		}
		if (Regex.IsMatch(parameters[0], @"^\s*room\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length >= 2)
			{
				string room = "";
				for (int i = 1; i < parameters.Length; i++)
				{
					room += parameters[i].ToLower() + " ";
				}
				room = room.Trim();
				if (rooms.Select(s => s.ToLower()).ToArray().Contains(room))
				{
					yield return null;
					int gotoind = Array.IndexOf(rooms.Select(s => s.ToLower()).ToArray(), room);
					int curind = roomindex;
					int ct1 = 0, ct2 = 0;
					while (curind != gotoind)
					{
						curind++;
						ct1++;
						if (curind > 12)
							curind = 0;
					}
					curind = roomindex;
					while (curind != gotoind)
					{
						curind--;
						ct2++;
						if (curind < 0)
							curind = 12;
					}
					if (ct1 < ct2)
					{
						for (int i = 0; i < ct1; i++)
						{
							ArrowRight.OnInteract();
							yield return new WaitForSeconds(0.1f);
						}
					}
					else
					{
						for (int i = 0; i < ct2; i++)
						{
							ArrowLeft.OnInteract();
							yield return new WaitForSeconds(0.1f);
						}
					}
				}
			}
			yield break;
		}
	}
}