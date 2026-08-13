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
    public Text[] playerPlayPauseTexts = new Text[4];

    [System.Serializable]
    public class RowFills
    {
        public Image[] fills = new Image[5];
    }
    public RowFills[] playerButtonFills = new RowFills[4];

    public Evereal.VRVideoPlayer.VRVideoPlayer localPreviewPlayer;

    private string[] videoKeywords = { "amazonia", "lencois", "noronha", "pantanal", "rio" };
    private Dictionary<int, string> playerCurrentVideo = new Dictionary<int, string>();
    private Dictionary<int, bool> playerIsOnline = new Dictionary<int, bool>();
    private Dictionary<int, bool> playerIsPlaying = new Dictionary<int, bool>();
    private Dictionary<int, float> playerProgress = new Dictionary<int, float>();
    private Dictionary<int, string> playerTimeStr = new Dictionary<int, string>();

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;

        for (int i = 0; i < 4; i++)
        {
            playerCurrentVideo[i] = "";
            playerIsOnline[i] = false;
            playerIsPlaying[i] = false;
            playerProgress[i] = 0f;
            playerTimeStr[i] = "";
        }

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
            bool online = playerIsOnline.ContainsKey(i) && playerIsOnline[i];
            bool playing = playerIsPlaying.ContainsKey(i) && playerIsPlaying[i];
            string curVid = playerCurrentVideo.ContainsKey(i) ? playerCurrentVideo[i].ToLowerInvariant() : "";
            float prog = playerProgress.ContainsKey(i) ? playerProgress[i] : 0f;
            string timeText = playerTimeStr.ContainsKey(i) && !string.IsNullOrEmpty(playerTimeStr[i]) ? $"\n<size=18><color=#67E8F9><b>{playerTimeStr[i]}</b></color></size>" : "";

            if (playerStatusTexts[i] != null)
            {
                string statusBadge = online ? "<color=#22C55E><b>● ON</b></color>" : "<color=#EF4444><b>○ OFF</b></color>";
                playerStatusTexts[i].text = $"<b><size=52>{i + 1}</size></b>   {statusBadge}{timeText}";
            }

            if (playerStatusBadges[i] != null)
            {
                // Vibrant background for status badge
                playerStatusBadges[i].color = online ? new Color(0.10f, 0.28f, 0.22f, 1f) : new Color(0.24f, 0.12f, 0.15f, 1f);
            }

            if (playerPlayPauseTexts != null && i < playerPlayPauseTexts.Length && playerPlayPauseTexts[i] != null)
            {
                playerPlayPauseTexts[i].text = playing ? "⏸" : "▶";
            }

            // Update Progressive Fill on the 5 buttons
            if (playerButtonFills != null && i < playerButtonFills.Length && playerButtonFills[i] != null)
            {
                for (int v = 0; v < 5; v++)
                {
                    Image fillImg = playerButtonFills[i].fills != null && v < playerButtonFills[i].fills.Length ? playerButtonFills[i].fills[v] : null;
                    if (fillImg != null)
                    {
                        bool isThisVideo = !string.IsNullOrEmpty(curVid) && curVid.Contains(videoKeywords[v]);
                        if (isThisVideo && online && prog > 0f)
                        {
                            fillImg.fillAmount = Mathf.Clamp01(prog);
                            fillImg.color = new Color(0.06f, 0.72f, 0.88f, 0.75f); // Bright electric cyan fill!
                        }
                        else
                        {
                            fillImg.fillAmount = 0f;
                        }
                    }
                }
            }
        }
    }

    public void PlayVideo(int slotIndex, string videoUrl)
    {
        string finalVideoName = "Videos/Amazonia_PT.mp4";
        if (videoUrl.ToLowerInvariant().Contains("noronha")) finalVideoName = "Videos/Noronha_PT.mp4";
        else if (videoUrl.ToLowerInvariant().Contains("lencois")) finalVideoName = "Videos/Lencois_PT.mp4";
        else if (videoUrl.ToLowerInvariant().Contains("pantanal")) finalVideoName = "Videos/Pantanal_PT.mp4";
        else if (videoUrl.ToLowerInvariant().Contains("rio")) finalVideoName = "Videos/Rio_PT.mp4";

        int userID = slotIndex;
        Debug.Log($"[Tablet] Disparando {finalVideoName} para Óculos {userID + 1}");

        playerCurrentVideo[userID] = finalVideoName;
        playerProgress[userID] = 0.01f;
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
        props.Add("Time_" + userID, (float)PhotonNetwork.Time);
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom)
        {
            try
            {
                pv.RPC("ReceiveSelectVideoCommand", RpcTarget.All, userID, finalVideoName);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("RPC send warning: " + ex.Message);
            }
        }
    }

    public void TogglePlayPause(int slotIndex)
    {
        int userID = slotIndex;
        bool isPlaying = playerIsPlaying.ContainsKey(userID) && playerIsPlaying[userID];

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.InRoom)
        {
            if (isPlaying)
            {
                pv.RPC("ReceivePauseCommand", RpcTarget.All, userID);
                playerIsPlaying[userID] = false;
            }
            else
            {
                pv.RPC("ReceivePlayCommand", RpcTarget.All, userID);
                playerIsPlaying[userID] = true;
            }
        }
    }

    public void StopVideo(int slotIndex)
    {
        int userID = slotIndex;
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

        playerCurrentVideo[userID] = "";
        playerProgress[userID] = 0f;
        playerIsPlaying[userID] = false;
        playerTimeStr[userID] = "";
    }

    [PunRPC]
    public void UpdateVideoData33(int targetUserID, string videoName, string url, bool isPlaying, double currentTime, double totalTime)
    {
        if (targetUserID >= 0 && targetUserID < 4)
        {
            string cleanName = System.IO.Path.GetFileNameWithoutExtension(videoName ?? url);
            cleanName = cleanName.Replace("_PT", "").Replace("_EN", "").Replace("_ES", "");
            
            string currentFormatted = $"{Mathf.FloorToInt((float)currentTime / 60):00}:{Mathf.FloorToInt((float)currentTime % 60):00}";
            string totalFormatted = totalTime > 0 ? $"{Mathf.FloorToInt((float)totalTime / 60):00}:{Mathf.FloorToInt((float)totalTime % 60):00}" : "";
            
            playerCurrentVideo[targetUserID] = videoName ?? url ?? "";
            playerIsOnline[targetUserID] = true;
            playerIsPlaying[targetUserID] = isPlaying;
            
            if (totalTime > 0)
            {
                playerProgress[targetUserID] = Mathf.Clamp01((float)(currentTime / totalTime));
                playerTimeStr[targetUserID] = $"{cleanName}\n{currentFormatted} / {totalFormatted}";
            }
            else
            {
                playerProgress[targetUserID] = 0f;
                playerTimeStr[targetUserID] = cleanName;
            }
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
