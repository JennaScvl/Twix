
/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class UDPReceive : MonoBehaviour {
	
	Thread receiveThread;

	UdpClient client;

	public Canvas canvas;
	public Image topScreenImage;
	public Image bottomScreenImage;

	public int port;
	public Texture2D topScreen = null;
	public Texture2D bottomScreen = null;
	public Texture2D tempTexture = null;
	public Texture2D defaultTex = null;
	public Material[] screens = new Material[0];
	byte screen = 0;

	public bool receivingdata = false;
	float lastreceiveddata = -10f;

	byte[] topScreenBytes = new byte[0];
	byte[] bottomScreenBytes = new byte[0];

	private static void Main()
	{
		UDPReceive receiveObj=new UDPReceive();
		receiveObj.init();

		string text="";
		do
		{
			text = Console.ReadLine();
		}
		while(!text.Equals("exit"));
	}
	int ipcount = 0;
	string ipprefix = "";
	public UDPReceive udpreceive;

	public void Start()
	{
		ip3DS = PlayerPrefs.GetString("last3DSIP");

		udpreceive = this;
		logQueue = new Queue<DEBUG_MSG>();
		topScreen = new Texture2D(400,240);
		bottomScreen = new Texture2D(320,240);
		tempTexture = new Texture2D(8,8);
		windowRect0.width=topWindowWidth;
		windowRect0.height=(topWindowWidth/1.666666666666f)+17f;
		windowRect1.width=bottomWindowWidth;
		windowRect1.height=(bottomWindowWidth/1.3333333333333f)+17f;

		init();

		//topScreenImage.material.mainTexture = topScreen;
		//bottomScreenImage.material.mainTexture = bottomScreen;
		canvas.enabled = false;
	}

	void CheckForReceivingData()
	{
		if(lastreceiveddata > 5f)
		{
			Debug.Log("Too much time since last frame, beginning port scan");
			BeginPortScan();
		}
	}

	public void BeginPortScan()
	{
		log =	"Scanning for 3DS and bootNTR\n" +
				"If the following number stops\n" +
				"counting for too long please\n" +
				"make sure your 3DS is set up\n" +
				"correctly and restart Twix.";
		string hostName = Dns.GetHostName(); 
		string myIP = "";
		int i;
		int j;
		IPAddress[] ip = Array.FindAll(Dns.GetHostByName(hostName).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
		char[] splitter = new char[] {'.'};
		string[] _ip = ip[0].ToString().Split(splitter);
		ipprefix = _ip[0]+"."+_ip[1]+"."+_ip[2]+".";
		StartCoroutine("ScanPorts");

	}

	IEnumerator ScanPorts()
	{
		string ip = "";
		count = 0;
		while(ipcount<256)
		{
			ip = ipprefix+ipcount.ToString();
			ScanPort(ip,8000);
			ipcount++;
			yield return new WaitForSecondsRealtime(0.01f);
		}
		ipcount=0;
	}

	void Update()
	{
		if(receivingdata && lastreceiveddata < 1f)
		{
			lock(wholeTopImage)
			{
				tempTexture.LoadImage(wholeTopImage, true);
				if(tempTexture.height==400)
				{
					topScreen.LoadImage(wholeTopImage, true);
					if(!usewindows)
					{
						screens[0].mainTexture = topScreen;
						//screens[0].SetTexture("_EmissionMap",topScreen);
					}
				}
			}
			lock(wholeBottomImage)
			{
				tempTexture.LoadImage(wholeBottomImage);
				if(tempTexture.height==320)
				{
					bottomScreen.LoadImage(wholeBottomImage, true);
					if(!usewindows)
					{
						screens[1].mainTexture = bottomScreen;
						//screens[1].material.SetTexture("_EmissionMap",bottomScreen);
					}
				}
			}
		}

		if(usewindows)
		{
			Vector2 mousepos = Input.mousePosition;
			Vector2 newmousescroll = Input.mouseScrollDelta * 5f;
			mousepos.y = Screen.height - mousepos.y;
			if(windowRect0.Contains(mousepos))
			{
				topWindowWidth+=newmousescroll.y;
				windowRect0.width=topWindowWidth;
				windowRect0.height=(topWindowWidth/1.666666666666f)+17f;
			}
			else if(windowRect1.Contains(mousepos))
			{
				bottomWindowWidth+=newmousescroll.y;
				windowRect1.width=bottomWindowWidth;
				windowRect1.height=(bottomWindowWidth/1.3333333333333f)+17f;
			}

			touchScreenRect = new Rect(windowRect1.x,windowRect1.y+17f,windowRect1.width,windowRect1.height-17f);
			if(touchScreenRect.Contains(mousepos))
			{

			}
		}
		lastreceiveddata+=Time.deltaTime;

	}
		
	Queue<DEBUG_MSG> logQueue = null;
	#region ThreadSafe Log
	public enum DEBUGTYPE
	{
		INFO,
		WARNING,
		ERROR,
		ASSERTION,
		EXCEPTION
	}
	public struct DEBUG_MSG
	{
		public DEBUGTYPE type;
		public string message;

		public DEBUG_MSG(DEBUGTYPE type, string message)
		{
			this.type = type;
			this.message = message;
		}
	}

	public void Log(string str)
	{
		Log(DEBUGTYPE.INFO, str);
	}

	public void Log(DEBUGTYPE type, string str)
	{
		lock(logQueue)
		{
			logQueue.Enqueue(new DEBUG_MSG(type,str));
		}
	}

	public void Log(string str, params object[] data)
	{
		Log(DEBUGTYPE.INFO, string.Format(str, data));
	}

	public void Log(DEBUGTYPE type, string str, params object[] data)
	{
		Log(type, string.Format(str, data));
	}

	public void PrintLog()
	{
		lock(logQueue)
		{
			if(logQueue.Count > 0)
			{
				DEBUG_MSG val = logQueue.Dequeue();
				if(!string.IsNullOrEmpty(val.message))
				{
					switch(val.type)
					{
						case DEBUGTYPE.WARNING:
							Debug.LogWarning(val.message);
							break;
						case DEBUGTYPE.ERROR:
							Debug.LogError(val.message);
							break;
						case DEBUGTYPE.ASSERTION:
							Debug.LogAssertion(val.message);
							break;
						case DEBUGTYPE.EXCEPTION:
							Debug.LogException(new Exception(val.message));
							break;
						default:
							Debug.Log(val.message);
							break;
					}
				}
			}
		}
	}
	#endregion

	float topWindowWidth = 400f;
	float bottomWindowWidth = 320f;
	Vector2 lastmousescroll = Vector2.zero;
	string log = "";
	public bool usewindows = false;
	void OnGUI()
	{
		GUIStyle style = new GUIStyle();

		if(count<256)
		{
			GUI.Label(new Rect(10,30,1000,1000),log+"\n"+count.ToString());
		}
   
	}

	public void ChangeDisplayButton()
	{
		usewindows=!usewindows;

		canvas.enabled = usewindows;
		if(usewindows)
		{
			Camera.main.transform.rotation = Quaternion.Euler(90f,0f,0f);
			Camera.main.transform.position = new Vector3(0f,-10f,0f);
		}
		else
		{
			Camera.main.transform.rotation = Quaternion.Euler(34.277f,0f,0f);
			Camera.main.transform.position = new Vector3(0f,0.978f,-1.242f);
		}
	}

	Rect windowRect0 = new Rect(200, 40, 400, 240);
	Rect windowRect1 = new Rect(200,300,320,240);
	public Rect touchScreenRect = new Rect();
	void TopScreenWindow(int windowID)
	{
		if(topScreen!=null)
		{
			Vector2 pivotPoint = new Vector2(windowRect0.width/2, (windowRect0.height/2)+17);
			GUIUtility.RotateAroundPivot(270, pivotPoint);
			GUI.DrawTexture(new Rect(0,17,windowRect0.height - 17,windowRect0.width),topScreen);
		}

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	void BottomScreenWindow(int windowID)
	{
		if(topScreen!=null)
		{
			Vector2 pivotPoint = new Vector2(windowRect1.width/2, (windowRect1.height/2)+17);
			GUIUtility.RotateAroundPivot(270, pivotPoint);
			GUI.DrawTexture(new Rect(0,17,windowRect1.height - 17,windowRect1.width),bottomScreen);
		}

		if(Input.GetMouseButton(1))GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}




	Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
	{
		Color32[] original = originalTexture.GetPixels32();
		Color32[] rotated = new Color32[original.Length];
		int w = originalTexture.width;
		int h = originalTexture.height;

		int iRotated, iOriginal;

		for (int j = 0; j < h; ++j)
		{
			for (int i = 0; i < w; ++i)
			{
				iRotated = (i + 1) * h - j - 1;
				iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
				rotated[iRotated] = original[iOriginal];
			}
		}

		Texture2D rotatedTexture = new Texture2D(h, w);
		rotatedTexture.SetPixels32(rotated);
		rotatedTexture.Apply();
		return rotatedTexture;
	}

	private void init()
	{
		print("UDPReceive.init()");

		print("Receiving at 127.0.0.1 : "+port);

		receiveThread = new Thread(
			new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();

	}
	string printstr = "";
	uint topPacketLength = 0;
	uint bottomPacketLength = 0;

	byte[] wholeTopImage = new byte[0];
	byte[] wholeBottomImage = new byte[0];

	byte[][] workingTopImage = new byte[2][];
	byte[][] workingBottomImage = new byte[2][];

	byte[]workingTopID = new byte[2];
	byte[]workingBottomID = new byte[2];

	byte[] wholeImage = new byte[0];
	byte[] workingimage = new byte[0];
	bool running = true;

	public UDPSend _UDPSend = null;

	byte topID = 0;
	byte bottomID = 0;

	byte LastTopID = 0;
	byte LastBottomID = 0;

	Int64 topCnt = 0;
	Int64 bottomCnt = 0;

	byte lastTopFramePkt;
	byte lastBottomFramePkt;

	public string ip3DS = "";

	private  void ReceiveData()
	{

		client = new UdpClient(port);
		while (running)
		{

			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				int offset = 4;

				byte[] data = client.Receive(ref anyIP);

				byte id = data[0];
				byte xTop = data[1];
				sbyte format = (sbyte)data[2];
				byte cnt = data[3];

				bool isTop = (xTop & 1) == 1;
				bool isFinalPkt = (xTop & 0x10) != 0;
				bool isFirstPkt = cnt == 0;



				if(isFirstPkt)
				{
					if(isTop)
					{
						topID = id;
						workingTopImage[0] = new byte[0];
						topCnt = 0;
					}
					else
					{
						bottomID = id;
						workingBottomImage[0]= new byte[0];
						bottomCnt = 0;
					}
				}
				else
				{
					if(id < (isTop?topID:bottomID))
					{
						if(id != 0)
							continue;
					}
					if((isTop?lastTopFramePkt:lastBottomFramePkt) != (cnt-1))
					{
						Log(DEBUGTYPE.WARNING, "Out of Order or Missing Pkt detected\nLast Pkt:{0} Curr Pkt:{1}", 
							isTop?lastTopFramePkt:lastBottomFramePkt, cnt);
						continue;
					}
				}

				if(isTop)
				{
					topCnt |= (1L << cnt);
					lastTopFramePkt = cnt;
				}
				else
				{
					bottomCnt |= (1L << cnt);
					lastBottomFramePkt = cnt;
				}

				if(isFinalPkt)
				{

					bool missedPkt = (isTop?topCnt:bottomCnt) != ((1L << cnt+1)-1);

					if(missedPkt==true)
					{
						if(isTop)
							workingTopImage[0] = new byte[0];
						else 
							workingBottomImage[0] = new byte[0];

						continue;
					}
				}

				if(!_UDPSend.inited)
				{
					_UDPSend.IP = anyIP.Address.ToString();
					ip3DS = _UDPSend.IP;
					_UDPSend.init();	
				}
				if((data[1]&1)==1)
				{

					byte[] concat = new byte[workingTopImage[0].Length + data.Length - offset];
					Array.Copy(workingTopImage[0],concat,workingTopImage[0].Length);
					Array.Copy(data,offset,concat,workingTopImage[0].Length,data.Length-offset);
					workingTopImage[0]= new byte[concat.Length];
					concat.CopyTo(workingTopImage[0],0);

					lock(wholeTopImage)
					{
						if(data.Length<topPacketLength)
						{
							wholeTopImage = new byte[workingTopImage[0].Length];
							workingTopImage[0].CopyTo(wholeTopImage,0);
							workingTopImage[0]= new byte[0];

						}
						topPacketLength = (uint)data.Length;
					}
				}
				else
				{
					byte[] concat = new byte[workingBottomImage[0].Length + data.Length - offset];
					Array.Copy(workingBottomImage[0],concat,workingBottomImage[0].Length);
					Array.Copy(data,offset,concat,workingBottomImage[0].Length,data.Length-offset);
					workingBottomImage[0] = new byte[concat.Length];
					concat.CopyTo(workingBottomImage[0],0);


					lock(wholeBottomImage)
					{
						if(data.Length<bottomPacketLength)
						{

							wholeBottomImage = new byte[workingBottomImage[0].Length];
							workingBottomImage[0].CopyTo(wholeBottomImage,0);
							workingBottomImage[0] = new byte[0];

						}
						bottomPacketLength = (uint)data.Length;
					}
				}
				receivingdata=true;
				lastreceiveddata = 0f;
				count=256;
			}
			catch (Exception err)
			{
				receivingdata=false;

			}
		}
	}

	void OnApplicationQuit()
	{
		running=false;
		print("OnApplicationQuit");
		receiveThread.Abort();
		if (client!=null)
			client.Close();
		PlayerPrefs.SetString("last3DSIP",ip3DS);
	}


	void OnDisable()
	{
		running=false;
		print("OnDisable");
		receiveThread.Abort();
		if (client!=null)
			client.Close();
	}

	int count = 256;
	private void ScanPort(string _ip, int port)
	{
		TcpClient client = new TcpClient();
		client.BeginConnect(IPAddress.Parse(_ip), port, new AsyncCallback(CallBack), client);
	}

	string bootNTRIP = "";
	public IEnumerator TalkToNTR(string _ip)
	{
		TcpClient tcpclient = new TcpClient();
		tcpclient.BeginConnect(IPAddress.Parse(_ip), 8000, new AsyncCallback(NTRCallback),tcpclient);
		yield return null;
	}

	public void _TalkToNTR()
	{
		TcpClient tcpclient = new TcpClient();
		tcpclient.BeginConnect(IPAddress.Parse(ip3DS), 8000, new AsyncCallback(NTRCallback),tcpclient);
	}
//
	public TcpClient ntrTCPClient = null;

	public uint jpegQuality = 80;
	public byte priorityMode = 1;
	public byte priorityFactor = 2;
	public float qosValue = 100f;

	//I know it's hacky to not duplicate the original Callback function,
	//but it doesn't work if I do that but it does work if I use a
	//duplicate, so bite me.
	private void NTRCallback(IAsyncResult result)
	{
		Debug.Log("NTRCallback");
		bool connected = false;
		using (TcpClient client = (TcpClient)result.AsyncState)
		{
			try
			{
				IPEndPoint _ip = (IPEndPoint)client.Client.RemoteEndPoint;
				Debug.Log("Second Sending Heartbeat()");
				client.GetStream().Write(Heartbeat(),0,84);
				Debug.Log("Second Sending RemotePlay()");
				client.GetStream().Write(RemotePlay(priorityMode,priorityFactor,jpegQuality,qosValue),0,84);
				client.EndConnect(result);
				connected = client.Connected;
			}
			catch (SocketException)
			{

			}
		}
		if(connected)
		{

		}
	}

	public void InitializeStream(TcpClient ntrclient)
	{
		Debug.Log("InitializeStream()");
	}

	public void StartStream(TcpClient tcpclient)
	{
		Debug.Log("StartStream()");
		tcpclient.GetStream().Write(RemotePlay(1,2,80,100),0,84);
	}

	private void CallBack(IAsyncResult result)
	{
		if(!receivingdata)
		{
			bool connected = false;
			string ip = "";

			using (TcpClient client = (TcpClient)result.AsyncState)
			{
				try
				{
					ip = client.Client.RemoteEndPoint.ToString();

					IPEndPoint _ip = (IPEndPoint)client.Client.RemoteEndPoint;
					client.GetStream().Write(Heartbeat(),0,84);
					client.GetStream().Write(RemotePlay(1,2,80,100),0,84);
					client.EndConnect(result);
					//Yes, I know I'm basically reconnecting to send the same data I just sent
					//for some reason it only works if I do it a second time, and even then,
					//only if I disconnect, then reconnectg to do it. I don't know why.
					//and frankly I don't care at this point, I'm just glad I haven't lost any
					//hair over it.
					UnityMainThreadDispatcher.Instance().Enqueue(TalkToNTR(_ip.Address.ToString()));
					connected = client.Connected;
				}
				catch (SocketException)
				{

				}
			}

			if (connected)
			{
				bootNTRIP = ip;
			}
			else
			{
			} 
		}
		count++;
		if(count==255)count++;
	}

	public byte[] RemotePlay(byte priorityMode, byte priorityFactor, uint jpegQuality, float qosValueF)
	{
		byte[] data = new byte[84];
		uint qosValue = (uint)(qosValueF * 0x20000);
		int i = 0;
		Array.Copy(BitConverter.GetBytes(0x12345678), 0, data, 0,4);
		Array.Copy(BitConverter.GetBytes(2000), 0, data, (4),4);
		Array.Copy(BitConverter.GetBytes(0), 0, data, (8),4);
		Array.Copy(BitConverter.GetBytes(901), 0, data, (12),4);
		Array.Copy(BitConverter.GetBytes(priorityFactor), 0, data, 16,1);
		Array.Copy(BitConverter.GetBytes(priorityMode), 0, data, 17, 1);
		Array.Copy(BitConverter.GetBytes(jpegQuality), 0, data, (20),4);
		Array.Copy(BitConverter.GetBytes(qosValue), 0, data, (24),4);

		return data;
	}

	public byte[] Heartbeat()
	{
		byte[]data = new byte[84];
		Array.Copy(BitConverter.GetBytes(0x12345678), 0, data, 0,4);
		Array.Copy(BitConverter.GetBytes(1000), 0, data, (4),4);
		return data;
	}


}