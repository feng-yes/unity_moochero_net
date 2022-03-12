using System.Diagnostics;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class LoginPanelController : MonoBehaviourPunCallbacks {

	public GameObject loginPanel;		//游戏登录面板
	public GameObject userMessage;		//玩家昵称信息
	public Button backButton;			//后退按钮
	public GameObject lobbyPanel;		//游戏大厅面板
	public GameObject roomPanel;		//游戏房间面板
	public Text username;				//玩家昵称文本
	public Text connectionState;		//网络连接状态

	//初始化，根据当前客户端连接状态，显示相应的游戏面板
	void Start () {
		//如果未连接Photon服务器
		if (!PhotonNetwork.IsConnected) {
			SetLoginPanelActive ();								//启用游戏登录面板
			username.text = PlayerPrefs.GetString ("Username");	//在本地保存玩家昵称
		} 
		//如果已连接Photon服务器
		else
			SetLobbyPanelActive ();	//启用游戏大厅面板
		connectionState.text = "";	//初始化网络连接状态文本信息
	}

//条件编译指令，只在Unity编辑器中（UNITY_EDITOR）编译此段代码
//#if(UNITY_EDITOR)	
	void Update(){		
		//在游戏画面左下角显示当前的网络连接状态
		connectionState.text = PhotonNetwork.NetworkClientState.ToString();
	}
//#endif

	//启用游戏登录面板
	public void SetLoginPanelActive(){
		loginPanel.SetActive (true);				//启用游戏登录面板
		userMessage.SetActive (false);				//禁用玩家昵称信息
		backButton.gameObject.SetActive (false);	//禁用后退按钮
		lobbyPanel.SetActive (false);				//禁用游戏大厅面板
		if(roomPanel!=null)
			roomPanel.SetActive (false);				//禁用游戏房间面板
	}
	//启用游戏大厅面板
	public void SetLobbyPanelActive(){				
		loginPanel.SetActive (false);				//禁用游戏登录面板
		userMessage.SetActive (true);				//启用玩家昵称信息
		backButton.gameObject.SetActive (true);		//启用后退按钮
		lobbyPanel.SetActive (true);				//启用游戏大厅面板
	}

	// step 1 : 连接服务器
	//"登录"按钮事件处理函数
	public void ClickLogInButton(){							
		SetLobbyPanelActive ();			//启用游戏大厅面板
		//客户端连接Photon服务器，游戏版本标识符为“1.0”
		if (!PhotonNetwork.IsConnected)
			// 先设置编码(4.0版本的服务器需要)
			// PhotonNetwork.NetworkingClient.LoadBalancingPeer.SerializationProtocolType = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV16;
			PhotonNetwork.ConnectUsingSettings ();		
		//如果玩家未输入昵称，这里自动为其分配一个昵称
		if (username.text == "")							
			username.text = "boy" + Random.Range (1, 9999);
		PhotonNetwork.LocalPlayer.NickName = username.text;			//设置玩家昵称
		PlayerPrefs.SetString ("Username", username.text);	//将玩家昵称保存在本地
	}
	//"退出"按钮事件处理函数
	public void ClickExitGameButton(){
		Application.Quit ();			//退出游戏应用
	}
	
	// step 2 : 连接到master，加入大厅
	/**覆写IPunCallback回调函数，当客户端连接到MasterServer时调用
	 * 加入默认游戏大厅
	 * 效果等同于勾选PhotonServerSettings中的Auto-join Lobby
	*/
	public override void OnConnectedToMaster ()
	{
		PhotonNetwork.JoinLobby();
	}

	// step 3 : 加入大厅成功，可以获取到房间列表信息等内容
	/**覆写IPunCallback回调函数，当玩家进入游戏大厅时调用
	 * 显示玩家昵称
	 */
	public override void OnJoinedLobby(){
		userMessage.GetComponentInChildren<Text> ().text
		= "欢迎，" + PhotonNetwork.LocalPlayer.NickName;
	}

	/**覆写IPunCallback回调函数，当客户端断开与Photon服务器的连接时调用
	 * 游戏画面返回游戏登录面板
	 */
	public override void OnDisconnected(DisconnectCause cause){
		SetLoginPanelActive ();
	}
}

