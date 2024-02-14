using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSend : MonoBehaviour
{
	private static int localPort;

	public string IP;
	public int port;

	IPEndPoint remoteEndPoint;
	UdpClient client;


	string strMessage="";

	public Transform cursor;
	public Camera cam;

	public Vector3[] cameraPos = new Vector3[0];
	public Vector3[] cameraRot = new Vector3[0];


	private static void Main()
	{
		UDPSend sendObj=new UDPSend();
		sendObj.init();
	}

	byte[] noinput = new byte[20]{
		0xff, //0
		0x0f, //1
		0x0,  //2
		0x0,  //3
		0x0,  //4
		0x0,  //5
		0x0,  //6
		0x02, //7
		0xff, //8
		0xf7, //9
		0x7f, //10
		0x0,  //11
		0x81, //12
		0x0,  //13
		0x80, //14
		0x80, //15
		0x0,  //16
		0x0,  //17
		0x0,  //18
		0x0}; //19

	byte[] notouch = new byte[] {0x0,0x0,0x0,0x2};

	Dictionary<string,Renderer> __buttons = new Dictionary<string, Renderer>();
	public Renderer powerIndicator = null;
	public void Start()
	{
		Camera.main.transform.position = cameraPos[0];
		Camera.main.transform.rotation = Quaternion.Euler(cameraRot[0]);
		circlePadCenter = circlePadCursor.position;
		cStickCenter = cStickCursor.position;
		int i;
		for(i=0;i<buttons.Length;i++)
		{
			__buttons.Add(buttons[i].gameObject.name,buttons[i]);
			buttons[i].enabled=false;
		}
		powerIndicator.enabled=false;
		lastIP = IP;
		tempIPString = IP;
	}


	byte[] inputs = new byte[20];
	uint oldtouch = 0;
	public Transform touchScreen;
	Vector2 hitpos = Vector2.zero;

	//inputs[0]
	uint down =  0x80;
	uint up =    0x40;
	uint left =  0x20;
	uint right = 0x10;
	uint start =  0x8;
	uint sel =    0x4;
	uint butb =   0x2;
	uint buta =   0x1;

	//inputs[1]
	//             0x80
	//             0x40
	//             0x20
	//             0x10
	uint buty =     0x8;
	uint butx =     0x4;
	uint butl =     0x2;
	uint butr =     0x1;

	//inputs[2]
	//               0x80
	//               0x40
	//               0x20
	//               0x10
	//                0x8
	//                0x4
	//                0x2
	//                0x1

	//inputs[3]
	//                 0x80
	//                 0x40
	//                 0x20
	//                 0x10
	//                  0x8
	//                  0x4
	//                  0x2
	//                  0x1
	uint buttonbits = 0;
	uint cStickX = 0x80;
	uint cStickY = 0x80;
	public Collider bottomCollider;

	public bool bButton = false;
	public bool aButton = false;
	public bool yButton = false;
	public bool xButton = false;
	public bool startButton = false;
	public bool selectButton = false;
	public bool homeButton = false;
	public bool dPadDown = false;
	public bool dPadUp = false;
	public bool dPadRight = false;
	public bool dPadLeft = false;

	public UDPReceive _UDPReceive=null;
	public bool inited = false;

	public UIHandler bottomScreen;

	public Renderer[] buttons = new Renderer[0];
	public Transform circlePadCursor;
	Vector3 circlePadCenter = Vector3.zero;
	public float circlePadMax = 0.1f;
	public Transform cStickCursor;
	Vector3 cStickCenter = Vector3.zero;
	public float cStickMax = 0.025f;
	public uint touchX;
	public uint touchY;
	public float minTouchX=0;
	public float minTouchY=0;
	public float maxTouchX=4096; 
	public float maxTouchY=4096;
	int camIndex;
	public float camLerpSpeed = 10f;
	void Update()
	{
		bool turbo = false;
		float t = Time.deltaTime;
		turbotimer+=t;
		if(turbotimer>(turbotime*.5f))
		{
			turbo=true;
		}

		if(turbotimer>turbotime)
		{
			turbo=false;
			turbotimer-=turbotime;
		}

		if(!inited)return;
		powerIndicator.enabled=true;
		if(lastIP!=IP){init();lastIP=IP;}
		RaycastHit hit;
		Ray vRay = cam.ScreenPointToRay(Input.mousePosition);


		inputs = noinput;

		bButton = false;
		aButton = false;
		yButton = false;
		xButton = false;
		startButton = false;
		selectButton = false;
		homeButton = false;
		dPadDown = false;
		dPadUp = false;
		dPadRight = false;
		dPadLeft = false;

		if (!_UDPReceive.usewindows)
		{
			if(Input.mouseScrollDelta.y>0f)camIndex++;
			if(Input.mouseScrollDelta.y<0f)camIndex--;
			if(camIndex==cameraPos.Length)camIndex=0;
			if(camIndex<0)camIndex=cameraPos.Length-1;

			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,cameraPos[camIndex],t*camLerpSpeed);
			Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation,Quaternion.Euler(cameraRot[camIndex]),t*camLerpSpeed);

			if(Physics.Raycast(vRay, out hit))
			{

				_UDPReceive.Log("hit");
				if(hit.transform == touchScreen)
				{
					cursor.position = hit.point;

					if(Input.GetMouseButton(0))
					{
						if(hit.transform.name == "Touch Screen")
						{
							//hitpos = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
							float _touchX = minTouchX + ((hit.textureCoord.x)*(maxTouchX-minTouchX));
							float _touchY = minTouchY + ((1f-hit.textureCoord.y)*(maxTouchY-minTouchY));


							touchX = (uint)Mathf.RoundToInt(_touchX);
							touchY = (uint)Mathf.RoundToInt(_touchY);
							uint _touch = (uint)((touchX | (touchY << 12) | (0x01 << 24)));
							byte[] touch = BitConverter.GetBytes(_touch);
							touch.CopyTo(inputs, 4);

						}

					}
					else
					{
						notouch.CopyTo(inputs,4);
					}

				}
				else
				{
					notouch.CopyTo(inputs,4);
					if(Input.GetMouseButton(0))
					{
						if(hit.transform.name == "B Button")bButton=true;
						else if(hit.transform.name == "A Button")aButton=true;
						else if(hit.transform.name == "Y Button")yButton=true;
						else if(hit.transform.name == "X Button")xButton=true;
						else if(hit.transform.name == "Start Button")startButton=true;
						else if(hit.transform.name == "Select Button")selectButton=true;
						else if(hit.transform.name == "Home Button")homeButton=true;
						else if(hit.transform.name == "DPad Down")dPadDown=true;
						else if(hit.transform.name == "DPad Left")dPadLeft=true;
						else if(hit.transform.name == "DPad Right")dPadRight=true;
						else if(hit.transform.name == "DPad Up")dPadUp=true;
					}
				}
			}
			else
			{
				notouch.CopyTo(inputs,4);

			}
		}
		else
		{
			if(bottomScreen.touchPos.z > 0f)
			{
				float __touchX = minTouchX + ((bottomScreen.touchPos.x)*(maxTouchX-minTouchX));
				float __touchY = minTouchY + ((bottomScreen.touchPos.y)*(maxTouchY-minTouchY));
				touchX = (uint)Mathf.RoundToInt(__touchX);
				touchY = (uint)Mathf.RoundToInt(__touchY);
				uint _touch = (uint)(touchX | (touchY << 12) | (0x01 << 24));
				byte[] touch = BitConverter.GetBytes(_touch);
				touch.CopyTo(inputs, 4);
			}
			else
			{
				notouch.CopyTo(inputs,4);
			}
		}

		joyXf = Input.GetAxisRaw("Horizontal");
		joyX = (uint)Mathf.RoundToInt((joyXf + 1f) * 2048f);
		joyYf = Input.GetAxisRaw("Vertical");
		joyY = (uint)Mathf.RoundToInt((joyYf + 1f) * 2048f);

		circlePadCursor.position = circlePadCenter + (new Vector3(joyXf, 0f ,-joyYf) * circlePadMax);

		uint _circle = ((0xFFF - joyY) << 12) + joyX;
		byte[] circle = BitConverter.GetBytes(_circle);
		circle.CopyTo(inputs, 8);

		if (Input.anyKey)
		{
			foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
			{
				if(Input.GetKey(kcode))
				{
					key = kcode.ToString();
				}
			}
		}


		byte[] _buttons = new byte[4];
		Array.Copy(noinput,0,_buttons,0,4);


		//-1 = left
		//1 = right
		hatXf = Input.GetAxisRaw("HatHorizontal");
		if(dPadLeft)hatXf=-1f;
		if(dPadRight)hatXf=1f;
		//-1 = down
		//1 = up
		hatYf = Input.GetAxisRaw("HatVertical");
		if(dPadDown)hatYf=-1f;
		if(dPadUp)hatYf=1f;

		uint byte0 = 0xff;
		uint byte1 = 0x0f;
		if(hatYf<0f || dPadDown)
		{
			byte0 = byte0 & ~down;
			__buttons["DPad Down"].enabled = true;
		}
		else
		{
			__buttons["DPad Down"].enabled = false;
		}

		if(hatYf>0f || dPadUp)
		{
			byte0 = byte0 & ~up;
			__buttons["DPad Up"].enabled = true;
		}
		else
		{
			__buttons["DPad Up"].enabled = false;
		}

		if(hatXf<0f || dPadLeft)
		{
			byte0 = byte0 & ~left;
			__buttons["DPad Left"].enabled = true;
		}
		else
		{
			__buttons["DPad Left"].enabled = false;
		}

		if(hatXf>0f || dPadRight)
		{
			byte0 = byte0 & ~right;
			__buttons["DPad Right"].enabled = true;
		}
		else
		{
			__buttons["DPad Right"].enabled = false;
		}

		if(Input.GetButton("Select") || selectButton)
		{
			byte0 = byte0 & ~sel;
			__buttons["Select Button"].enabled = true;
		}
		else
		{
			__buttons["Select Button"].enabled = false;
		}

		if(Input.GetButton("Start") || startButton)
		{
			byte0 = byte0 & ~start;
			__buttons["Start Button"].enabled = true;
		}
		else
		{
			__buttons["Start Button"].enabled = false;
		}

		if((!turboB && (Input.GetButton("B") || bButton)) || (turboB && turbo && (Input.GetButton("B") || bButton)))
		{
			byte0 = byte0 & ~butb;
			__buttons["B Button"].enabled = true;
		}
		else
		{
			__buttons["B Button"].enabled = false;
		}
		if((!turboA && (Input.GetButton("A") || aButton)) || (turboA && turbo && (Input.GetButton("A") || aButton)))
		{
			byte0 = byte0 & ~buta;
			__buttons["A Button"].enabled = true;
		}
		else
		{
			__buttons["A Button"].enabled = false;
		}
		if((!turboY && (Input.GetButton("Y") || yButton)) || (turboY && turbo && (Input.GetButton("Y") || yButton)))
		{
			byte1 = byte1 & ~buty;
			__buttons["Y Button"].enabled = true;
		}
		else
		{
			__buttons["Y Button"].enabled = false;
		}
		if((!turboX && (Input.GetButton("X") || xButton)) || (turboX && turbo && (Input.GetButton("X") || xButton)))
		{
			byte1 = byte1 & ~butx;
			__buttons["X Button"].enabled = true;
		}
		else
		{
			__buttons["X Button"].enabled = false;
		}


		if((!turboL && Input.GetButton("L")) || (turboL && turbo && Input.GetButton("L")))
		{
			byte1 = byte1 & ~butl;
			__buttons["L Button"].enabled = true;
		}
		else
		{
			__buttons["L Button"].enabled = false;
		}
		if((!turboR && Input.GetButton("R")) || (turboR && turbo && Input.GetButton("R")))
		{
			byte1 = byte1 & ~butr;
			__buttons["R Button"].enabled = true;
		}
		else
		{
			__buttons["R Button"].enabled = false;
		}


		inputs[0]=(byte)byte0;
		inputs[1]=(byte)byte1;

		uint byte12 = 0x81;
		uint byte13 = 0x0;
		uint byte14 = 0x80;
		uint byte15 = 0x80;

		if((!turboZR && Input.GetButton("ZR")) || (turboZR && turbo && Input.GetButton("ZR")))
		{
			byte13 = byte13 | 0x02;
			__buttons["ZR Button"].enabled = true;
		}
		else
		{
			__buttons["ZR Button"].enabled = false;
		}

		if((!turboZL && Input.GetButton("ZL")) || (turboZL && turbo && Input.GetButton("ZL")))
		{
			byte13 = byte13 | 0x04;
			__buttons["ZL Button"].enabled = true;
		}
		else
		{
			__buttons["ZL Button"].enabled = false;
		}

		Vector3 cStick = new Vector3(Input.GetAxisRaw("CStickX"), Input.GetAxisRaw("CStickY"), 0f);
		cStickCursor.position = cStickCenter + (new Vector3(cStick.x,0f,cStick.y) * cStickMax);
		cStick = Quaternion.Euler(0f,0f,315) * cStick;

		cStickX = (uint)Mathf.RoundToInt((cStick.x + 1f) * 128f);
		cStickY = (uint)Mathf.RoundToInt((cStick.y + 1f) * 128f);
		byte[] cStickXBytes = BitConverter.GetBytes(cStickX);
		byte[] cStickYBytes = BitConverter.GetBytes(cStickY);

		inputs[12] = (byte)byte12;
		inputs[13] = (byte)byte13;
		inputs[14] = (byte)cStickXBytes[0];
		inputs[15] = (byte)cStickYBytes[0];

		inputs[16] = (byte)0;
		if(homeButton)
		{
			inputs[16]=(byte)0x01;
			__buttons["Home Button"].enabled = true;
		}
		else
		{
			__buttons["Home Button"].enabled = false;
		}

		sendBytes(inputs);

	}
	string key = "";

	uint joyX = 0;
	float joyXf = 0f;

	uint joyY = 0;
	float joyYf = 0f;

	float hatXf = 0f;
	float hatYf = 0f;
	// OnGUI

	string tempIPString;

	public bool turboA;
	public bool turboB;
	public bool turboX;
	public bool turboY;
	public bool turboL;
	public bool turboR;
	public bool turboZL;
	public bool turboZR;
	float turbotimer = 0f;
	float turbotime = 0.1f;

	public void init()
	{
		if(inited)return;
		print("UDPSend.init()");
		inited=true;

		lastIP=IP;

		remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
		client = new UdpClient();

		print("Sending to "+IP+" : "+port);


	}
	string lastIP = "";

	private void inputFromConsole()
	{
		try
		{
			string text;
			do
			{
				text = Console.ReadLine();

				if (text != "")
				{

					byte[] data = Encoding.UTF8.GetBytes(text);

					client.Send(data, data.Length, remoteEndPoint);
				}
			} while (text != "");
		}
		catch (Exception err)
		{
			print(err.ToString());
		}

	}

	private void sendBytes(byte[] data)
	{
		try
		{
			client.Send(data, data.Length, remoteEndPoint);
		}
		catch (Exception err)
		{
			//print(err.ToString());
		}
	}
}

