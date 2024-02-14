using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class OptionsMenu : MonoBehaviour {

	public UDPReceive blah = null;

	void Awake() {
		bool motionBlur = false;
		bool filmic = false;
		bool antialiasing = false;
		bool bloom = false;
		bool.TryParse(PlayerPrefs.GetString("motionblur"), out motionBlur);
		bool.TryParse(PlayerPrefs.GetString("filmic"), out filmic);
		bool.TryParse(PlayerPrefs.GetString("antialias"), out antialiasing);
		bool.TryParse(PlayerPrefs.GetString("bloom"), out bloom);

		screenProfile.antialiasing.enabled=antialiasing;
		profile.colorGrading.enabled=filmic;
		screenProfile.bloom.enabled=bloom;
		profile.motionBlur.enabled=motionBlur;
	}
	
	bool showMenu = true;

	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(blah.receivingdata)showConnectInfo=false;
			showMenu = !showMenu;
		}
	}

	void OnGUI()
	{
		if(!showConnectInfo || blah.receivingdata)
		{
			windowRect0.width = 125;
		}
		if(showMenu)
		{
			windowRect0 = GUI.Window(0, windowRect0, DoOptionsWindow, "Options");
			if(showTurboWindow)
			{
				windowRect1 = GUI.Window(1, windowRect1, DoTurboWindow, "Turbo Options");
			}
		}
	}

	Rect windowRect0 = new Rect(200, 40, 400, 240);
	Rect windowRect1 = new Rect(200, 285, 160, 130);
	bool showTurboWindow = false;

	bool motionBlur = false;
	bool filmic = false;
	bool antialiasing = false;
	bool bloom = false;
	public PostProcessingProfile profile = null;
	public PostProcessingProfile screenProfile = null;

	string priorityMode = "1";
	string priorityFactor = "2";
	string qosValue = "100";
	string jpegQuality = "80";
	bool showConnectInfo = true;

	string ip;
	public UDPSend udpsender = null;
	void DoTurboWindow(int windowID)
	{
		udpsender.turboA = GUI.Toggle(new Rect(10,25,100,20),udpsender.turboA,"A Turbo");
		udpsender.turboB = GUI.Toggle(new Rect(10,50,100,20),udpsender.turboB,"B Turbo");
		udpsender.turboX = GUI.Toggle(new Rect(10,75,100,20),udpsender.turboX,"X Turbo");
		udpsender.turboY = GUI.Toggle(new Rect(10,100,100,20),udpsender.turboY,"Y Turbo");

		udpsender.turboL = GUI.Toggle(new Rect(75,25,100,20),udpsender.turboL,"L Turbo");
		udpsender.turboR = GUI.Toggle(new Rect(75,50,100,20),udpsender.turboR,"R Turbo");
		udpsender.turboZL = GUI.Toggle(new Rect(75,75,100,20),udpsender.turboZL,"ZL Turbo");
		udpsender.turboZR = GUI.Toggle(new Rect(75,100,100,20),udpsender.turboZR,"ZR Turbo");
		GUI.DragWindow(new Rect(0,0,10000,10000));
	}
	bool strongaa = false;

	public Camera tCam;
	public Camera bCam;

	void DoOptionsWindow(int windowID)
	{

		GUI.Label(new Rect(10, 200, 300,20),"Press ESC to open");
		GUI.Label(new Rect(10, 215, 300,20),"or close this menu");
		if(GUI.Button(new Rect(10,25,100,20),"ChangeDisplay"))
		{
			blah.ChangeDisplayButton();
		}

		motionBlur = GUI.Toggle(new Rect(10,50,100,20),motionBlur,"Motion Blur");
		filmic = GUI.Toggle(new Rect(10,75,100,20),filmic,"Filmic");
		antialiasing = GUI.Toggle(new Rect(10,100,100,20),antialiasing,"Anti-Aliasing");
		if(antialiasing)strongaa = GUI.Toggle(new Rect(10,120,100,20),strongaa,"Strong AA");
		bloom = GUI.Toggle(new Rect(10,150,100,20),bloom,"Bloom");

		if(GUI.Button(new Rect(10,175,50,20),"Turbos"))
		{
			showTurboWindow=!showTurboWindow;
		}


		PostProcessingBehaviour pbt = tCam.GetComponent<PostProcessingBehaviour>();
		AntialiasingModel.Settings antit = pbt.profile.antialiasing.settings;

		PostProcessingBehaviour pbb = tCam.GetComponent<PostProcessingBehaviour>();
		AntialiasingModel.Settings antib = pbt.profile.antialiasing.settings;
		//PostProcessingBehaviour pb = ge
		//AntialiasingModel.FxaaPreset aapreset = screenProfile.antialiasing.settings;


		if(strongaa)
		{
			antib.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
			pbb.profile.antialiasing.settings = antib;
			antit.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
			pbt.profile.antialiasing.settings = antit;
		//	aapreset = AntialiasingModel.FxaaPreset.ExtremeQuality;
		//	screenProfile.antialiasing.settings.fxaaSettings.preset = aapreset;
		}
		else
		{
			antib.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Performance;
			pbb.profile.antialiasing.settings = antib;
			antit.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Performance;
			pbt.profile.antialiasing.settings = antit;			
		//	aapreset=AntialiasingModel.FxaaPreset.Performance;
		//	screenProfile.antialiasing.settings.fxaaSettings.preset = aapreset;
		}

		if(!udpsender._UDPReceive.usewindows)
		{
			screenProfile.motionBlur.enabled = false;
			screenProfile.colorGrading.enabled = false;
			//profile.antialiasing.enabled = false;
			profile.bloom.enabled = false;

			profile.motionBlur.enabled = motionBlur;
			profile.colorGrading.enabled = filmic;
			screenProfile.antialiasing.enabled = antialiasing;
			screenProfile.bloom.enabled = bloom;
		}
		else
		{
			profile.motionBlur.enabled = false;
			profile.colorGrading.enabled = false;
			//profile.antialiasing.enabled = false;
			profile.bloom.enabled = false;

			screenProfile.motionBlur.enabled = motionBlur;
			screenProfile.colorGrading.enabled = filmic;
			screenProfile.antialiasing.enabled = antialiasing;
			screenProfile.bloom.enabled = bloom;
		}

		if(showConnectInfo && !blah.receivingdata)
		{
			if(showTryIPButton)
			{
				GUI.Label(new Rect(115,25,100,20),"PriorityMode");
				priorityMode = GUI.TextField(new Rect(200,25,100,20),priorityMode);
				byte.TryParse(priorityMode, out blah.priorityMode);
				priorityMode = blah.priorityMode.ToString();
				GUI.Label(new Rect(115,50,100,20),"PriorityFactor");
				priorityFactor = GUI.TextField(new Rect(200,50,100,20),priorityFactor);
				byte.TryParse(priorityFactor, out blah.priorityFactor);
				priorityFactor = blah.priorityFactor.ToString();
				GUI.Label(new Rect(115,75,100,20),"JPEG Quality");
				jpegQuality = GUI.TextField(new Rect(200,75,100,20),jpegQuality);
				uint.TryParse(jpegQuality, out blah.jpegQuality);
				jpegQuality = blah.jpegQuality.ToString();
				GUI.Label(new Rect(115,100,100,20),"QOS Value");
				qosValue = GUI.TextField(new Rect(200,100,100,20),qosValue);
				float.TryParse(qosValue, out blah.qosValue);
				qosValue = blah.qosValue.ToString();
				if(GUI.Button(new Rect(115,125,100,20),"Start Stream"))
				{
					showConnectInfo = false;
					blah.BeginPortScan();
				}
			}

			if(blah.ip3DS!="")
			{
				if(showTryIPButton)
				{
					blah.ip3DS=GUI.TextField(new Rect(115,150,100,20),blah.ip3DS);
					if(GUI.Button(new Rect(115,175,100,20),"Try IP"))
					{
						TryIP();
						Invoke("TryIP",5f);
						showTryIPButton = false;
					}
				}
			}
		}

		GUI.DragWindow(new Rect(0,0,10000,10000));
	}

	bool showTryIPButton = true;
	void TryIP()
	{
		blah._TalkToNTR();
		showTryIPButton=true;
	}

	void OnApplicationQuit()
	{
		PlayerPrefs.SetString("bloom",profile.bloom.enabled.ToString());
		PlayerPrefs.SetString("filmic",profile.colorGrading.enabled.ToString());
		PlayerPrefs.SetString("motionblur",profile.motionBlur.enabled.ToString());
		PlayerPrefs.SetString("antialias",screenProfile.antialiasing.enabled.ToString());

	}

}
