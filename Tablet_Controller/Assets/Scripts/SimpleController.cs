using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class SimpleController : MonoBehaviourPunCallbacks
{
    private const string LOG_FORMAT = "[SimpleController] {0}";

    [Header("Photon Settings")]
    [SerializeField] private string fixedRegion = "sa";
    [SerializeField] private string fixedAppVersion = "1";
    [SerializeField] private string roomName = "RiR-23";

    public Text statusHeader;
    public Text[] playerStatusTexts = new Text[4];
    public Image[] playerStatusBadges = new Image[4];
    public GameObject[] playerPlayPauseBtns = new GameObject[4];
    public GameObject[] playerStopBtns = new GameObject[4];
    public Text[] playerPlayPauseTexts = new Text[4];

    [System.Serializable]
    public class RowElements
    {
        public Button[] buttons = new Button[5];
        public Image[] btnBackgrounds = new Image[5];
        public Image[] btnFrames = new Image[5];
        public Image[] btnFills = new Image[5];
        public Text[] btnTexts = new Text[5];
    }
    public RowElements[] playerRows = new RowElements[4];

    public Evereal.VRVideoPlayer.VRVideoPlayer localPreviewPlayer;

    private readonly string[] videoKeywords = { "amazonia", "lencois", "noronha", "pantanal", "rio" };
    private readonly float[] defaultDurations = { 200f, 160f, 195f, 185f, 225f };

    private int[] activeVideoIndex = new int[4] { -1, -1, -1, -1 };
    private float[] activeTimer = new float[4] { 0f, 0f, 0f, 0f };
    private float[] activeDuration = new float[4] { 200f, 200f, 200f, 200f };
    private bool[] playerIsOnline = new bool[4] { false, false, false, false };
    private bool[] playerIsPlaying = new bool[4] { false, false, false, false };

    // Colors
    private readonly Color colNormalBg = new Color(0.11f, 0.18f, 0.32f, 1f);      // Royal Navy
    private readonly Color colNormalFrame = new Color(0.20f, 0.38f, 0.65f, 0.8f); // Blue Frame
    private readonly Color colActiveBg = new Color(0.18f, 0.36f, 0.72f, 1f);       // Brighter Blue Active
    private readonly Color colActiveFrame = new Color(0.22f, 0.85f, 1f, 1f);       // Luminous Electric Cyan Frame
    private readonly Color colFill = new Color(0.06f, 0.82f, 0.95f, 0.75f);        // Electric Cyan Fill

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;

        ConfigureAndConnect();
    }

    void ConfigureAndConnect()
    {
        roomName = "RiR-23";

        if (PhotonNetwork.PhotonServerSettings != null)
        {
            if (!string.IsNullOrWhiteSpace(fixedRegion))
            {
                PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = fixedRegion.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(fixedAppVersion))
            {
                PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = fixedAppVersion.Trim();
            }

            ServerSettings.ResetBestRegionCodeInPreferences();
        }

        ConnectToPhoton();
    }

    void Update()
    {
        // Smoothly increment active timers every frame
        for (int i = 0; i < 4; i++)
        {
            if (playerIsPlaying[i] && activeVideoIndex[i] >= 0)
            {
                activeTimer[i] += Time.deltaTime;
                if (activeTimer[i] > activeDuration[i])
                {
                    activeTimer[i] = activeDuration[i];
                }
            }
        }

        UpdateUIStatus();
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room {roomName} successfully! Region: {PhotonNetwork.CloudRegion}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerOnlineStatuses();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerOnlineStatuses();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        RefreshPlayerOnlineStatuses();
    }

    private void RefreshPlayerOnlineStatuses()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        bool[] onlineSlots = new bool[4];

        foreach (var p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (p.IsLocal) continue;

            int slot = -1;
            if (p.CustomProperties.ContainsKey("SlotIndex"))
            {
                slot = (int)p.CustomProperties["SlotIndex"];
            }
            else
            {
                slot = p.ActorNumber - 2; 
            }

            if (slot >= 0 && slot < 4)
            {
                onlineSlots[slot] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            playerIsOnline[i] = onlineSlots[i];
        }
    }

    private void UpdateUIStatus()
    {
        if (statusHeader != null)
        {
            bool inRoom = PhotonNetwork.InRoom;
            string onlineTag = inRoom ? "<color=#22C55E>● CONECTADO</color>" : "<color=#FBBF24>○ CONECTANDO...</color>";
            statusHeader.text = $"<b><size=46><color=#38BDF8>BRASIL</color><color=#FACC15>VR</color></size></b>      <size=22>{onlineTag}</size>\n<size=18><color=#E2E8F0>PAINEL DE CONTROLE MULTI-VR</color></size>";
        }

        RefreshPlayerOnlineStatuses();

        for (int i = 0; i < 4; i++)
        {
            bool online = playerIsOnline[i];
            bool playing = playerIsPlaying[i];
            int curActive = activeVideoIndex[i];
            float curTime = activeTimer[i];
            float totalTime = activeDuration[i];

            // Badge Text & Status
            if (playerStatusTexts[i] != null)
            {
                string statusBadge = online ? "<color=#22C55E><b>● ON</b></color>" : "<color=#EF4444><b>○ OFF</b></color>";
                string timeFormatted = "";
                if (curActive >= 0 && (playing || curTime > 0))
                {
                    string curStr = $"{Mathf.FloorToInt(curTime / 60):00}:{Mathf.FloorToInt(curTime % 60):00}";
                    string totStr = $"{Mathf.FloorToInt(totalTime / 60):00}:{Mathf.FloorToInt(totalTime % 60):00}";
                    timeFormatted = $"\n<size=15><color=#67E8F9><b>{curStr} / {totStr}</b></color></size>";
                }
                playerStatusTexts[i].text = $"<b><size=46>{i + 1}</size></b>  {statusBadge}{timeFormatted}";
            }

            if (playerStatusBadges[i] != null)
            {
                playerStatusBadges[i].color = online ? new Color(0.10f, 0.25f, 0.20f, 1f) : new Color(0.20f, 0.10f, 0.12f, 1f);
            }

            // Play/Pause & Stop Controls inside Column 1
            bool showControls = online && (curActive >= 0);
            if (playerPlayPauseBtns != null && i < playerPlayPauseBtns.Length && playerPlayPauseBtns[i] != null)
            {
                if (playerPlayPauseBtns[i].activeSelf != showControls)
                {
                    playerPlayPauseBtns[i].SetActive(showControls);
                }
            }
            if (playerStopBtns != null && i < playerStopBtns.Length && playerStopBtns[i] != null)
            {
                if (playerStopBtns[i].activeSelf != showControls)
                {
                    playerStopBtns[i].SetActive(showControls);
                }
            }

            if (playerPlayPauseTexts != null && i < playerPlayPauseTexts.Length && playerPlayPauseTexts[i] != null)
            {
                playerPlayPauseTexts[i].text = playing ? "PAUSE" : "PLAY";
            }

            // 5 Buttons: Fixed grid, never shift
            if (playerRows != null && i < playerRows.Length && playerRows[i] != null)
            {
                for (int v = 0; v < 5; v++)
                {
                    bool isActiveVideo = (curActive == v);
                    Button btn = playerRows[i].buttons != null && v < playerRows[i].buttons.Length ? playerRows[i].buttons[v] : null;
                    Image bg = playerRows[i].btnBackgrounds != null && v < playerRows[i].btnBackgrounds.Length ? playerRows[i].btnBackgrounds[v] : null;
                    Image frame = playerRows[i].btnFrames != null && v < playerRows[i].btnFrames.Length ? playerRows[i].btnFrames[v] : null;
                    Image fill = playerRows[i].btnFills != null && v < playerRows[i].btnFills.Length ? playerRows[i].btnFills[v] : null;
                    Text txt = playerRows[i].btnTexts != null && v < playerRows[i].btnTexts.Length ? playerRows[i].btnTexts[v] : null;

                    if (btn != null)
                    {
                        btn.interactable = online;
                    }

                    if (bg != null)
                    {
                        bg.color = !online ? new Color(0.08f, 0.11f, 0.18f, 0.5f) : (isActiveVideo ? colActiveBg : colNormalBg);
                    }

                    if (frame != null)
                    {
                        frame.color = !online ? new Color(0.14f, 0.18f, 0.25f, 0.4f) : (isActiveVideo ? colActiveFrame : colNormalFrame);
                    }

                    if (txt != null)
                    {
                        txt.color = !online ? new Color(0.5f, 0.55f, 0.65f, 0.5f) : Color.white;
                    }

                    if (fill != null)
                    {
                        if (isActiveVideo && totalTime > 0f && online)
                        {
                            fill.fillAmount = Mathf.Clamp01(curTime / totalTime);
                            fill.color = colFill;
                        }
                        else
                        {
                            fill.fillAmount = 0f;
                        }
                    }
                }
            }
        }
    }

    public void PlayVideo(int slotIndex, string videoUrl)
    {
        if (!playerIsOnline[slotIndex])
        {
            Debug.LogWarning($"[Tablet] Slot {slotIndex + 1} está offline! Ignorando comando.");
            return;
        }

        string finalVideoName = "Videos/Amazonia_PT.mp4";
        int vidIndex = 0;

        if (videoUrl.ToLowerInvariant().Contains("noronha")) { finalVideoName = "Videos/Noronha_PT.mp4"; vidIndex = 2; }
        else if (videoUrl.ToLowerInvariant().Contains("lencois")) { finalVideoName = "Videos/Lencois_PT.mp4"; vidIndex = 1; }
        else if (videoUrl.ToLowerInvariant().Contains("pantanal")) { finalVideoName = "Videos/Pantanal_PT.mp4"; vidIndex = 3; }
        else if (videoUrl.ToLowerInvariant().Contains("rio")) { finalVideoName = "Videos/Rio_PT.mp4"; vidIndex = 4; }

        int userID = slotIndex;
        Debug.Log($"[Tablet] Disparando {finalVideoName} para Óculos {userID + 1}");

        activeVideoIndex[userID] = vidIndex;
        activeTimer[userID] = 0f;
        activeDuration[userID] = defaultDurations[vidIndex];
        playerIsPlaying[userID] = true;

        if (PhotonNetwork.CurrentRoom == null)
        {
            ConnectToPhoton();
            return;
        }

        if (localPreviewPlayer != null)
        {
            localPreviewPlayer.Load(finalVideoName, true);
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("Video_" + userID, finalVideoName);
        props.Add("Command_" + userID, "play");
        props.Add("Time_" + userID, (float)PhotonNetwork.Time);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom)
        {
            try
            {
                pv.RPC("ReceiveSelectVideoCommand", RpcTarget.All, userID, finalVideoName);
                pv.RPC("ReceivePlayCommand", RpcTarget.All, userID);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("RPC send warning: " + ex.Message);
            }
        }
    }

    public void TogglePlayPause(int slotIndex)
    {
        if (!playerIsOnline[slotIndex]) return;

        int userID = slotIndex;
        bool isPlaying = playerIsPlaying[userID];
        bool newPlaying = !isPlaying;
        playerIsPlaying[userID] = newPlaying;

        Debug.Log($"[Tablet] TogglePlayPause for Slot {userID + 1} -> {(newPlaying ? "PLAY" : "PAUSE")}");

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("Command_" + userID, newPlaying ? "play" : "pause");
        props.Add("Time_" + userID, (float)PhotonNetwork.Time);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom)
        {
            try
            {
                if (newPlaying)
                {
                    pv.RPC("ReceivePlayCommand", RpcTarget.All, userID);
                }
                else
                {
                    pv.RPC("ReceivePauseCommand", RpcTarget.All, userID);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Play/Pause RPC warning: " + ex.Message);
            }
        }
    }

    public void StopVideo(int slotIndex)
    {
        if (!playerIsOnline[slotIndex]) return;

        int userID = slotIndex;
        Debug.Log($"[Tablet] StopVideo for Slot {userID + 1}");

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("Command_" + userID, "stop");
        props.Add("Time_" + userID, (float)PhotonNetwork.Time);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom)
        {
            try
            {
                pv.RPC("ReceiveStopCommand", RpcTarget.All, userID);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Stop RPC warning: " + ex.Message);
            }
        }

        activeVideoIndex[userID] = -1;
        activeTimer[userID] = 0f;
        playerIsPlaying[userID] = false;
    }

    [PunRPC]
    public void UpdateVideoData33(int targetUserID, string videoName, string url, bool isPlaying, double currentTime, double totalTime)
    {
        if (targetUserID >= 0 && targetUserID < 4)
        {
            string rawName = (videoName ?? url ?? "").ToLowerInvariant();
            for (int v = 0; v < videoKeywords.Length; v++)
            {
                if (rawName.Contains(videoKeywords[v]))
                {
                    activeVideoIndex[targetUserID] = v;
                    break;
                }
            }

            if (totalTime > 0)
            {
                activeDuration[targetUserID] = (float)totalTime;
            }

            activeTimer[targetUserID] = (float)currentTime;
            playerIsPlaying[targetUserID] = isPlaying;
            playerIsOnline[targetUserID] = true;
        }
    }

    [PunRPC]
    public void UpdateStatus(int targetUserID, string status)
    {
        if (targetUserID >= 0 && targetUserID < 4)
        {
            playerIsOnline[targetUserID] = (status == "online");
        }
    }
}
