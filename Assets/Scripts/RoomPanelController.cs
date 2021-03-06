using System.Diagnostics;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanelController : MonoBehaviourPunCallbacks {

	public GameObject lobbyPanel;		//游戏大厅面板
	public GameObject roomPanel;		//游戏房间面板
	public Button backButton;			//返回按钮
	public Text roomName;				//房间名称文本
	public GameObject[] Team1;			//队伍1面板（显示队伍1信息）
	public GameObject[] Team2;			//队伍2面板（显示队伍2信息）
	public Button readyButton;			//准备/开始游戏按钮
	public Text promptMessage;			//提示信息

	PhotonView pView;
	int teamSize;
	Text[] texts;
	ExitGames.Client.Photon.Hashtable costomProperties;

	void OnEnable () {
		base.OnEnable();
		pView = GetComponent<PhotonView>();					//获取PhotonView组件
		if(!PhotonNetwork.IsConnected)return;
		roomName.text = "房间：" + PhotonNetwork.CurrentRoom.Name;	//显示房间名称
		promptMessage.text = "";							//提示信息

		backButton.onClick.RemoveAllListeners ();			//移除返回按钮绑定的所有监听事件
		backButton.onClick.AddListener (delegate() {		//为返回按钮绑定新的监听事件
			PhotonNetwork.LeaveRoom ();						//客户端离开游戏房间
			lobbyPanel.SetActive (true);					//激活游戏大厅面板
			roomPanel.SetActive (false);					//禁用游戏房间面板
		});

		teamSize = PhotonNetwork.CurrentRoom.MaxPlayers / 2;		//计算每队人数
		DisableTeamPanel ();								//初始化队伍面板
		UpdateTeamPanel (false);							//更新队伍面板（false表示不显示本地玩家信息）

		//交替寻找两队空余位置，将玩家信息放置在对应空位置中
		for (int i = 0; i < teamSize; i++) {	
			if (!Team1 [i].activeSelf) {		//在队伍1找到空余位置
				Team1 [i].SetActive (true);		//激活对应的队伍信息UI
				texts = Team1 [i].GetComponentsInChildren<Text> ();
				texts [0].text = PhotonNetwork.NickName;				//显示玩家昵称
				if(PhotonNetwork.IsMasterClient)texts[1].text="房主";	//如果玩家是MasterClient，玩家状态显示"房主"
				else texts [1].text = "未准备";							//如果玩家不是MasterClient，玩家状态显示"未准备"
				costomProperties = new ExitGames.Client.Photon.Hashtable () {	//初始化玩家自定义属性
					{ "Team","Team1" },		//玩家队伍
					{ "TeamNum",i },		//玩家队伍序号
					{ "isReady",false },	//玩家准备状态
					{ "Score",0 }			//玩家得分
				};
				PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);	//将玩家自定义属性赋予玩家
				break;
			} else if (!Team2 [i].activeSelf) {	//在队伍2找到空余位置
				Team2 [i].SetActive (true);		//激活对应的队伍信息UI
				texts = Team2 [i].GetComponentsInChildren<Text> ();		//显示玩家昵称
				if(PhotonNetwork.IsMasterClient)texts[1].text="房主";	//如果玩家是MasterClient，玩家状态显示"房主"
				else texts [1].text = "未准备";							//如果玩家不是MasterClient，玩家状态显示"未准备"
				string Team = "Team";
				costomProperties = new Hashtable () {	//初始化玩家自定义属性
					{ Team,"Team2" },		//玩家队伍
					{ "TeamNum",i },		//玩家队伍序号
					{ "isReady",false },	//玩家准备状态
					{ "Score",0 }			//玩家得分
				};
				PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);	//将玩家自定义属性赋予玩家,异步
				break;
			}
		}
	}

	/**覆写IPunCallback回调函数，当玩家属性更改时调用
	 * 更新队伍面板中显示的玩家信息
	 */ 
	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
		DisableTeamPanel ();	//禁用队伍面板
		UpdateTeamPanel (true);	//根据当前玩家信息在队伍面板显示所有玩家信息（true表示显示本地玩家信息）
		ReadyButtonControl ();	//设置ReadyButton的按钮事件
	}

	/**覆写IPunCallback回调函数，当MasterClient更变时调用
	 * 设置ReadyButton的按钮事件
	 */
	public override void OnMasterClientSwitched (Player newMasterClient) {
		ReadyButtonControl ();
	}

	/**覆写IPunCallback回调函数，当有玩家离开房间时调用
	 * 更新队伍面板中显示的玩家信息
	 */ 
	public override void OnPlayerLeftRoom(Player otherPlayer){
		DisableTeamPanel ();	//禁用队伍面板
		UpdateTeamPanel (true);	//根据当前玩家信息在队伍面板显示所有玩家信息（true表示显示本地玩家信息）
	}

	//禁用队伍面板
	void DisableTeamPanel(){
		for (int i = 0; i < Team1.Length; i++) {
			Team1 [i].SetActive (false);
		}
		for (int i = 0; i < Team2.Length; i++) {
			Team2 [i].SetActive (false);
		}
	}

	/**在队伍面板显示玩家信息
	 * 函数参数表示是否显示本地玩家信息
	 */
	void UpdateTeamPanel(bool isUpdateSelf){
		GameObject go;
		foreach (Player p in PhotonNetwork.PlayerList) {	//获取房间里所有玩家信息
			if (!isUpdateSelf && p.IsLocal)	continue;			//判断是否更新本地玩家信息
			costomProperties = p.CustomProperties;				//获取玩家自定义属性
			if (costomProperties ["Team"].Equals ("Team1")) {	//判断玩家所属队伍
				go = Team1 [(int)costomProperties ["TeamNum"]];	//查询玩家的队伍序号
				go.SetActive (true);							//激活显示玩家信息的UI
				texts = go.GetComponentsInChildren<Text> ();	//获取显示玩家信息的Text组件
			} else {											
				go = Team2 [(int)costomProperties ["TeamNum"]];	
				go.SetActive (true);
				texts = go.GetComponentsInChildren<Text> ();
			}
			texts [0].text = p.NickName;						//显示玩家姓名
			if(p.IsMasterClient)							//如果玩家是MasterClient
				texts[1].text="房主";						//玩家状态显示"房主"
			else if ((bool)costomProperties ["isReady"]) {	//如果玩家不是MasterClient，获取玩家的准备状态isReady
				texts [1].text = "已准备";					//isReady为true，显示"已准备"
			} else
				texts [1].text = "未准备";					//isReady为false，显示"未准备"
		}
	}

	//ReadyButton按钮事件设置
	void ReadyButtonControl(){
		if (PhotonNetwork.IsMasterClient) {									//如果玩家是MasterClient
			readyButton.GetComponentInChildren<Text> ().text = "开始游戏";	//ReadyButton显示"开始游戏"
			readyButton.onClick.RemoveAllListeners ();						//移除ReadyButton所有监听事件
			readyButton.onClick.AddListener (delegate() {					//为ReadyButton绑定新的监听事件
				ClickStartGameButton ();									//开始游戏
			});
		} else {															//如果玩家不是MasterClient
			if((bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"])		//根据玩家准备状态显示对应的文本信息
				readyButton.GetComponentInChildren<Text> ().text = "取消准备";		
			else 
				readyButton.GetComponentInChildren<Text> ().text = "准备";
			readyButton.onClick.RemoveAllListeners ();						//移除ReadyButton所有监听事件
			readyButton.onClick.AddListener (delegate() {					//为ReadyButton绑定新的监听事件
				ClickReadyButton ();										//切换准备状态
			});
		}
	}

	//"切换队伍"按钮
	public void ClickSwitchButton(){
		costomProperties = PhotonNetwork.LocalPlayer.CustomProperties;	//获取玩家自定义属性
		if ((bool)costomProperties ["isReady"]) {					//如果玩家处于准备状态
			promptMessage.text="准备状态下不能切换队伍";				//提示信息显示玩家不能切换队伍
			return;													//结束函数的执行
		}
		bool isSwitched = false;		//标记玩家切换队伍是否成功，默认值表示不成功	
		if (costomProperties ["Team"].ToString ().Equals ("Team1")) {				//判断玩家队伍
			for (int i = 0; i < teamSize; i++) {									//寻找另一支队伍是否有空余位置
				if (!Team2 [i].activeSelf) {										//如果找到了空缺位置
					isSwitched = true;												//标记玩家切换队伍成功
					Team1 [(int)costomProperties ["TeamNum"]].SetActive (false);	//禁用之前显示玩家信息的UI
					texts = Team2 [i].GetComponentsInChildren<Text> ();				//获取切换队伍后，显示玩家信息的Text组件
					texts [0].text = PhotonNetwork.NickName;						//填入玩家昵称
					if(PhotonNetwork.IsMasterClient)texts[1].text="房主";			//如果玩家是MasterClient，玩家状态显示"房主"
					else texts [1].text = "未准备";									//如果玩家不是MasterClient，玩家状态显示"未准备"
					Team2 [i].SetActive (true);										//激活显示玩家信息的UI
					costomProperties = new ExitGames.Client.Photon.Hashtable ()		//重新设置玩家的自定义属性
					{ { "Team","Team2" }, { "TeamNum",i } };
					PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);	
					break;
				}
			}
		} else if (costomProperties ["Team"].ToString ().Equals ("Team2")) {		//判断玩家队伍
			for (int i = 0; i < teamSize; i++) {									//寻找另一支队伍是否有空余位置
				if (!Team1 [i].activeSelf) {										//如果找到了空缺位置
					isSwitched = true;												//标记玩家切换队伍成功
					Team2 [(int)(costomProperties ["TeamNum"])].SetActive (false);	//禁用之前显示玩家信息的UI
					texts = Team1 [i].GetComponentsInChildren<Text> ();				//获取切换队伍后，显示玩家信息的Text组件
					texts [0].text = PhotonNetwork.NickName;						//填入玩家昵称
					if(PhotonNetwork.IsMasterClient)texts[1].text="房主";			//如果玩家是MasterClient，玩家状态显示"房主"
					else texts [1].text = "未准备";									//如果玩家不是MasterClient，玩家状态显示"未准备"
					Team1 [i].SetActive (true);										//激活显示玩家信息的UI
					costomProperties = new ExitGames.Client.Photon.Hashtable ()		//重新设置玩家的自定义属性
					{ { "Team","Team1" }, { "TeamNum",i } };
					PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);
					break;
				}
			}
		}
		if (!isSwitched)
			promptMessage.text = "另一队伍已满,无法切换";	//如果玩家切换队伍失败，提示信息显示"另一队伍已满，无法切换"
		else
			promptMessage.text = "";
	}

	//准备按钮事件响应函数
	public void ClickReadyButton(){
		bool isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties ["isReady"];					//获取玩家准备状态
		costomProperties = new ExitGames.Client.Photon.Hashtable (){ { "isReady",!isReady } };	//重新设置玩家准备状态
		PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);
		Text readyButtonText = readyButton.GetComponentInChildren<Text> ();	//获取ReadyButton的按钮文本
		if(isReady)readyButtonText.text="准备";		//根据玩家点击按钮后的状态，设置按钮文本的显示
		else readyButtonText.text="取消准备";
	}

	//开始游戏按钮事件响应函数
	public void ClickStartGameButton(){
		foreach (Player p in PhotonNetwork.PlayerList) {		//遍历房间内所有玩家
			if (p.IsLocal) continue;								//不检查MasterClient房主的准备状态
			if ((bool)p.CustomProperties ["isReady"] == false) {	//如果有人未准备
				promptMessage.text = "有人未准备，游戏无法开始";		//提示信息显示"有人未准备，游戏无法开始"
				return;												//结束函数执行
			}
		}
		promptMessage.text = "";										//清空提示信息
		PhotonNetwork.CurrentRoom.IsOpen = false;								//设置房间的open属性，使游戏大厅的玩家无法加入此房间
		pView.RPC ("LoadGameScene", RpcTarget.All, "GameScene");	//调用RPC，让游戏房间内所有玩家加载场景GameScene，开始游戏
	}

	//RPC函数，玩家加载场景
	[PunRPC]
	public void LoadGameScene(string sceneName){	
		PhotonNetwork.LoadLevel (sceneName);	//加载场景名为sceneName的场景
	}
}
