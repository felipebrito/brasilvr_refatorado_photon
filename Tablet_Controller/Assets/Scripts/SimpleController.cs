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

    public Evereal.VRVideoPlayer.VRVideoPlayer localPreviewPlayer;

    private Dictionary<int, string> playerCurrentVideo = new Dictionary<int, string>();
    private Dictionary<int, bool> playerIsOnline = new Dictionary<int, bool>();

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
        Debug.Log("Connected to Master. Joining room: " + roomName);
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room {roomName} successfully! Region: {PhotonNetwork.CloudRegion}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player entered: {newPlayer.ActorNumber}");
        RefreshPlayerOnlineStatuses();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player left: {otherPlayer.ActorNumber}");
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
            if (p.IsLocal) continue; // Skip tablet itself

            int slot = -1;
            if (p.CustomProperties.ContainsKey("SlotIndex"))
            {
                slot = (int)p.CustomProperties["SlotIndex"];
            }
            else
            {
                // Fallback to ActorNumber
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
            string state = PhotonNetwork.NetworkClientState.ToString();
            int count = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
            string region = PhotonNetwork.CloudRegion ?? fixedRegion;
            statusHeader.text = $"Rede: {state} | Sala: {roomName} ({region}) | Conectados: {count}";
        }

        RefreshPlayerOnlineStatuses();

        for (int i = 0; i < 4; i++)
        {
            bool online = playerIsOnline.ContainsKey(i) && playerIsOnline[i];
            
            if (playerStatusTexts[i] != null)
            {
                string videoInfo = playerCurrentVideo.ContainsKey(i) ? $" - {playerCurrentVideo[i]}" : "";
                string statusStr = online ? $"<color=#00FF66>● ONLINE</color>{videoInfo}" : "<color=#FF4444>○ DESCONECTADO</color>";
                playerStatusTexts[i].text = $"Player {i + 1} {statusStr}";
            }

            if (playerStatusBadges[i] != null)
            {
                playerStatusBadges[i].color = online ? new Color(0.1f, 0.8f, 0.2f, 1f) : new Color(0.8f, 0.2f, 0.2f, 1f);
            }
        }
    }

    public void PlayVideo(int slotIndex, string videoUrl)
    {
        string finalVideoName = "Videos/Amazonia_PT.mp4";
        if (videoUrl.Contains("Noronha")) finalVideoName = "Videos/Noronha_PT.mp4";
        else if (videoUrl.Contains("Lencois")) finalVideoName = "Videos/Lencois_PT.mp4";
        else if (videoUrl.Contains("Pantanal")) finalVideoName = "Videos/Pantanal_PT.mp4";
        else if (videoUrl.Contains("Rio")) finalVideoName = "Videos/Rio_PT.mp4";

        int userID = slotIndex;
        Debug.Log($"[Tablet] Enviando {finalVideoName} para Player {userID + 1} (Slot {userID})");

        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("Ainda não conectou na sala do Photon!");
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

    [PunRPC]
    public void UpdateVideoData33(int targetUserID, string videoName, string url, bool isPlaying, double currentTime, double totalTime)
    {
        if (targetUserID >= 0 && targetUserID < 4)
        {
            string cleanName = System.IO.Path.GetFileNameWithoutExtension(videoName ?? url);
            string timeFormatted = $"{Mathf.FloorToInt((float)currentTime / 60):00}:{Mathf.FloorToInt((float)currentTime % 60):00}";
            playerCurrentVideo[targetUserID] = $"{cleanName} [{timeFormatted}]";
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
