using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using KModkit;
using RNG = UnityEngine.Random;

public class NRneedyFuse : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMNeedyModule Needy;
	public KMSelectable ModuleSelectable;
	public Transform CoverPanel;
	public KMSelectable[] fuses;
	public TextMesh Values;
	
	public TextMesh[] FuseLabels;
	public Renderer[] FuseParts;
	public Material[] OldFuseMats;
	public TextMesh ModelLabel;
	public TextMesh ServiceLabel;
	public bool moduleDebug;
	//public string audioTest;

	//Read-only Library
	private bool needyActive = false;
	private bool[] poppedFuses = {false, false, false, false, false, false};
	private int SelectedNumber;
	//private bool armed = false; //Determines if another module can be solved without causing a strike
	private int[] doorAnim = new int[] {-1, 0}; //stops at 10 and 0

	private int[] displayValues = new int[] {88, 17, 42, 37, 4, 79, 66};

	private int[] OldFuses = new int[] {0, 6};
	private string[] ServiceLibrary = new string[] {
		"Yesterday lol", //Haha get crunked
		"1st January, 1970", //Unix epoch
		"Aug 6, 1991", //The first website
		"Mon Jan 2 15:04:05 MST 2006", //01/02 03:04:05PM '06 -0700
		"November 5, 1605", //The Gunpowder Plot
		"July 7, 2005", //London Train Bombing
		"The fitness gram pacer test is a\nmultistage aerobic capacity test...", //Exactly what you think
		"2016/06/19", //First modded module
		"November 8, 2000", //Counter Strike initial release
		"July 2, 2005", //Enumclaw
		"Yo, What the fuck did you just fucking say about me, yo, you little bitch, yo?\nI'll have you know I graduated top of my class in the Navy Seals, yo, and\nI've been involved in numerous secret raids on Al-Quaeda, yo, and I have over\n300 confirmed kills, yo!", //Suck my dick yo!
		"July 20, 2069 8:37 PM ETC" //t:3141592653:f
	};
	private string[] ModelLibrary = new string[] {
		"CH0P",
		"F4L1",
		"P155",
		"R3K1",
		"3GG5",
		"R3S6",
		"C3PO",
		"BB-8",
		"D4LK",
		"E3PC"
	};
	private bool autOS = false;
	static int moduleIdCounter = 1;
	int moduleId;
	
	void Awake() {
		moduleId = moduleIdCounter++;
		//armed = true;
		GetComponent<KMSelectable>().OnDefocus += GameFixes.OnDefocus(() => {
			//Debug.Log("OnDefocus");
		});

		ModuleSelectable.OnFocus += delegate () { doorAnim[0] = 1; };
		ModuleSelectable.OnDefocus += delegate () { doorAnim[0] = -1; };
		foreach (KMSelectable NAME in fuses) {
			KMSelectable pressedObject = NAME;
			NAME.OnInteract += delegate () { toggleFuse(pressedObject); return false; };
		}
		GetComponent<KMNeedyModule>().OnNeedyActivation += NeedyStart;
		GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
		GetComponent<KMNeedyModule>().OnNeedyDeactivation += NeedyOff;
		
		OldFuses[0] = RNG.Range(0, 6);
		OldFuses[1] = RNG.Range(0, 6);
		while (OldFuses[0] == OldFuses[1]) { OldFuses[1] = RNG.Range(0, 6); }
		if (moduleDebug) {OldFuses[0] = 0; OldFuses[1] = 5;}
		for (int i = 0; i < 2; i++){
			for (int j = 0; j < 2; j++){
				//Debug.Log(OldFuses[i]*3+j);
				FuseParts[OldFuses[i]*3+j].material = OldFuseMats[0];
				FuseLabels[OldFuses[i]*2+j].color = new Color(0.14f, 0.14f, 0.14f);//242424FF
			}
			FuseParts[OldFuses[i]*3+2].material = OldFuseMats[1];
		}

		Debug.LogFormat("[Needy Fuse Box #{0}] Fuses {1} and {2} are swapped", moduleId, OldFuses[0]+1, OldFuses[1]+1);
		if (moduleDebug) {Debug.LogFormat("[Needy Fuse Box #{0}] Starting in DEBUG MODE", moduleId);}

		int GagModel = RNG.Range(0, ModelLibrary.Length);
		int GagLabel = RNG.Range(0, ServiceLibrary.Length);
		if (moduleDebug) { GagModel = 1; }
		if (moduleDebug) { GagLabel = 0; }

		ModelLabel.text = "Bomb Corp.\nModel: " + ModelLibrary[GagModel] + "\n-Service by-";
		ServiceLabel.text = ServiceLibrary[GagLabel];
		if(GagLabel == 3 || GagLabel == 6 || GagLabel == 11) { ServiceLabel.fontSize = 23; }
		if(GagLabel == 10) { ServiceLabel.fontSize = 10; }
    }

	void NeedyStart() {
		needyActive = true;
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CapacitorPop, transform);
		//int LOOPDEBUG = 0;
		int pickA = RNG.Range(0, 6);
		int pickB = RNG.Range(0, 6);
		int pickC = RNG.Range(0, 6);
		int pickD = RNG.Range(0, 6);
		int pickE = RNG.Range(0, 6);
		int pickF = RNG.Range(0, 6);
		while (pickB == pickA) {
			pickB = RNG.Range(0, 6);
			//Debug.Log("I'm Helping!");
		}
		
		while (pickC == pickA || pickC == pickB) {
			pickC = RNG.Range(0, 6);
		}
		int popCount = 0;
		string listOut = "";
		for (int i = 0; i<6; i++){
			if (i == pickA || i == pickB || i == pickC || i == pickD || i == pickE || i == pickF) {
				poppedFuses[i] = true;
				if (popCount == 0) { listOut = listOut + (i+1); } else { listOut = listOut + "-" + (i+1); }
				popCount += 1;
			} else { poppedFuses[i] = false; }
		}
		if (!autOS) {
			Debug.LogFormat("[Needy Fuse Box #{0}] /!\\ POWER SURGE /!\\ The following breakers have tripped\n{1}", moduleId, listOut);
		} else {
			Debug.LogFormat("[Needy Fuse Box #{0}] /!\\ POWER SURGE /!\\ Initiating autOS", moduleId, listOut);
		}
	}

	void OnTimerExpired() {
		Debug.LogFormat("[Needy Fuse Box #{0}] ...Power was not restored within the timeframe...Strike Recieved", moduleId);
		Needy.HandleStrike();
        Needy.HandlePass();
		needyActive = false;
		Values.text = "**";
	}

	void NeedyOff() {
		//Needy.HandlePass();
		needyActive = false;
		Values.text = "00";
	}

	void toggleFuse(KMSelectable fuse) {
		int fuseNum = Array.IndexOf(fuses, fuse);
		if (fuseNum == OldFuses[0]) {
			fuseNum = OldFuses[1];
			Audio.PlaySoundAtTransform("click17", transform);
		} else if (fuseNum == OldFuses[1]) {
			fuseNum = OldFuses[0];
			Audio.PlaySoundAtTransform("click17", transform);
		} else { Audio.PlaySoundAtTransform("click15", transform); }
		
		if (needyActive || moduleDebug) {
			//Debug.Log("I'm Helping!");
			poppedFuses[fuseNum] = !poppedFuses[fuseNum];
		}
		if (moduleDebug) {Debug.LogFormat("[Needy Fuse Box #{0}] Fuse {1} set to {2}", moduleId, fuseNum+1, poppedFuses[fuseNum]);}
		CheckSolve();
	}

	void CheckSolve() {
		if (!needyActive) { return; }
		foreach (bool FUSE in poppedFuses){
			if (FUSE) { return; }
		}
		Debug.LogFormat("[Needy Fuse Box #{0}] ...Power restored", moduleId);
		Needy.HandlePass();
        needyActive = false;
		Values.text = "00";
	}

	void Update() {
		
		if (needyActive || moduleDebug) { freezeTimer(); }

		int SLAM = 5 + (5*doorAnim[0]);
		if (doorAnim[1] != SLAM) {
			CoverPanel.Rotate(15.0f*doorAnim[0], 0.0f, 0.0f);
			doorAnim[1] += doorAnim[0];
			if (doorAnim[0] == 1 && doorAnim[1] == 1) { Audio.PlaySoundAtTransform("metal_open_01", transform); }
			if (doorAnim[0] == -1 && doorAnim[1] == 0) { Audio.PlaySoundAtTransform("metal_hit_01", transform); }
		}
	}

	void freezeTimer() {
		SelectedNumber = 0;
		int tempA = 0;
		int tempB = 0;
		for (int i = 0; i < 6; i++) {
			if (poppedFuses[i]) {
				tempA = i+1;
				tempB += 1;
				SelectedNumber += displayValues[i+1];
				if (SelectedNumber < 0) {
					SelectedNumber += 100;
				} else if (SelectedNumber > 99) {
					SelectedNumber -= 100;
				}
			}
		}
		if (tempB == 1) {
			SelectedNumber = tempA;
		}
		string PRINT = "";
		if (SelectedNumber < 10){ PRINT = "0"; }
		PRINT += SelectedNumber.ToString();
		Values.text = PRINT;
	}

			// Twitch Plays Support by Kilo Bites // Modified by Nimsay Ramsey

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} Press 1-6 to push a fuse button";
#pragma warning restore 414

	bool isValidPos(string n)
	{
		string[] valids = { "1", "2", "3", "4", "5", "6"};
		if (!valids.Contains(n))
		{
			return false;
		}
		return true;
	}

	IEnumerator ProcessTwitchCommand (string command)
	{
		yield return null;

		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		if (split[0].EqualsIgnoreCase("PRESS")) {
			//int numberClicks = 0;
			int pos = 1;
			if (split.Length != 2) {
				yield return "sendtochaterror Please specify a button to press!";
				yield break;
			} else if (!isValidPos(split[1])) {
				yield return "sendtochaterror " + split[1] + " is not a valid button!";
				yield break;
			} else {
				int.TryParse(split[1], out pos);
				pos -= 1;
				fuses[pos].OnInteract();
			}
			yield break;
		}
	}

	void TwitchHandleForcedSolve() { //Autosolver
		Debug.LogFormat("[Needy Fuse Box #{0}] Autofix software loaded...autOS should fix any future power surges automatically", moduleId);
		autOS = true;
		StartCoroutine(DealWithNeedy());
	}
	
	IEnumerator DealWithNeedy () {
		while (true) {
			while(!needyActive){
				yield return null;
			}
			for (int i = 0; i < 6; i++){
				if (!poppedFuses[i]) { continue;}
				if (i == OldFuses[0]) { fuses[OldFuses[1]].OnInteract(); } else if (i == OldFuses[1]) { fuses[OldFuses[0]].OnInteract(); } else { fuses[i].OnInteract(); }
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

}
