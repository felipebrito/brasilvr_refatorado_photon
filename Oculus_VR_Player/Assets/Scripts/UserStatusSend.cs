using System.Collections;
using UnityEngine;
using Photon.Pun;
using Evereal.VRVideoPlayer;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine.Android;
using ExitGames.Client.Photon;

public class UserStatusSend : MonoBehaviourPunCallbacks
{
    [Header("Photon")]
    [SerializeField] private string fixedRegion = "sa";
    [SerializeField] private string fixedAppVersion = "1";
    [SerializeField] private string roomName = "RiR-23";

    private int _cachedSlot = -1;
    public int userID
    {
        get
        {
            if (_cachedSlot >= 0) return _cachedSlot;
            string appId = Application.identifier;
            if (!string.IsNullOrEmpty(appId))
            {
                char lastChar = appId[appId.Length - 1];
                if (char.IsDigit(lastChar))
                {
                    _cachedSlot = Mathf.Max(0, int.Parse(lastChar.ToString()) - 1);
                    return _cachedSlot;
                }
            }
            return 0;
        }
    }
    public VRVideoPlayer vrVideoPlayer;
    public VideoPlayerCtrl videoPlayerCtrl;
    public TextMeshPro Mensagem;
    public GameObject aviso;
    public GameObject ambiente;
    public GameObject sphere;

    private Coroutine reconnectRoutine;

    void Start()
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        }
#endif

        roomName = "RiR-23";
        Application.runInBackground = true;
        PhotonNetwork.KeepAliveInBackground = 60f;
        OVRManager.HMDMounted += OnHeadsetMounted;
        OVRManager.HMDUnmounted += OnHeadsetUnmounted;
        ConfigurePhotonAndConnect();
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    private void OnDestroy()
    {
        OVRManager.HMDMounted -= OnHeadsetMounted;
        OVRManager.HMDUnmounted -= OnHeadsetUnmounted;
    }

    private void ConfigurePhotonAndConnect()
    {
        if (PhotonNetwork.PhotonServerSettings == null)
        {
            Debug.LogError("PhotonServerSettings not found.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(fixedRegion))
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = fixedRegion.Trim().ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(fixedAppVersion))
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = fixedAppVersion.Trim();
        }

        ServerSettings.ResetBestRegionCodeInPreferences();
        PhotonNetwork.ConnectUsingSettings();
    }

    private void JoinOrCreateGameRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 7,
            CleanupCacheOnLeave = true,
            EmptyRoomTtl = 0
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master.");
        JoinOrCreateGameRoom();
    }

    public override void OnJoinedRoom()
    {
        int slotIndex = 0; // Default (Player 1)
        string appId = Application.identifier;
        if (!string.IsNullOrEmpty(appId))
        {
            char lastChar = appId[appId.Length - 1];
            if (char.IsDigit(lastChar))
            {
                int headsetNumber = int.Parse(lastChar.ToString());
                slotIndex = Mathf.Max(0, headsetNumber - 1);
            }
        }
        
        Debug.Log($"Assigning SlotIndex {slotIndex} based on package {appId}");

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new ExitGames.Client.Photon.Hashtable { { "SlotIndex", slotIndex } }
        );

        TrySendStatus("online");

        // Initialize timestamp to current time so old cached room properties are NOT auto-played on connect
        lastProcessedTime = (float)PhotonNetwork.Time;

        if (reconnectRoutine != null)
        {
            StopCoroutine(reconnectRoutine);
            reconnectRoutine = null;
        }

        StartCoroutine(SendVideoDataRoutine());
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable properties)
    {
        try
        {
            CheckRoomPropertiesForVideo(properties);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in OnRoomPropertiesUpdate: " + ex.ToString());
        }
    }

    private string lastProcessedVideo = "";
    private float lastProcessedTime = 0f;

    private IEnumerator DelayedCheckRoomProperties(ExitGames.Client.Photon.Hashtable properties)
    {
        yield return new WaitForSeconds(1.5f);
        CheckRoomPropertiesForVideo(properties);
    }

    private void CheckRoomPropertiesForVideo(ExitGames.Client.Photon.Hashtable properties)
    {
        int currentUserId = userID;

        Debug.Log($"CheckRoomPropertiesForVideo - My UserID/Slot: {currentUserId} (App: {Application.identifier})");
        foreach (var key in properties.Keys)
        {
            Debug.Log("Property updated: " + key + " = " + properties[key]);
        }

        if (properties.ContainsKey("Video_" + currentUserId))
        {
            string newVideoUrl = (string)properties["Video_" + currentUserId];
            Debug.Log("Matched Video for UserID " + currentUserId + ": " + newVideoUrl);
            ReceiveSelectVideoCommand(currentUserId, newVideoUrl);
        }
        else if (properties.ContainsKey("GlobalVideo"))
        {
            string newVideoUrl = (string)properties["GlobalVideo"];
            Debug.Log("Matched GlobalVideo: " + newVideoUrl);
            ReceiveSelectVideoCommand(-1, newVideoUrl);
        }

        if (properties.ContainsKey("Command_" + currentUserId))
        {
            string cmd = (string)properties["Command_" + currentUserId];
            Debug.Log($"Matched Command for UserID {currentUserId}: {cmd}");
            if (cmd == "pause") ReceivePauseCommand(currentUserId);
            else if (cmd == "play") ReceivePlayCommand(currentUserId);
            else if (cmd == "stop") ReceiveStopCommand(currentUserId);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[UserStatusSend] Left room. Auto-recovering connection...");
        EnsureConnection();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[UserStatusSend] OnJoinRoomFailed: {returnCode} - {message}. Retrying in 1s...");
        Invoke(nameof(JoinOrCreateGameRoom), 1f);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[UserStatusSend] OnCreateRoomFailed: {returnCode} - {message}. Retrying in 1s...");
        Invoke(nameof(JoinOrCreateGameRoom), 1f);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // If another player leaves (likely the tablet master), we should stop the video
        if (vrVideoPlayer != null)
        {
            vrVideoPlayer.Stop();
        }
        if (sphere != null)
        {
            sphere.SetActive(false);
        }
    }

    [PunRPC]
    public void NotifyRoomCreated()
    {
        Debug.Log("Room created, trying to join.");
        if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateGameRoom();
        }
    }

    [PunRPC]
    public void ReceiveOnlineCheckCommand()
    {
        TrySendStatus("online");
    }

    [PunRPC]
    public void UpdateStatus(int targetUserID, string status)
    {
        if (targetUserID == userID)
        {
            Debug.Log($"Status received for user {userID}: {status}");
        }
    }

    private float nextConnectionCheckTime = 0f;

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[UserStatusSend] OnApplicationPause: {pauseStatus}");
        if (!pauseStatus)
        {
            EnsureConnection();
        }
        else
        {
            TrySendStatus("offline");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"[UserStatusSend] OnApplicationFocus: {hasFocus}");
        if (hasFocus)
        {
            EnsureConnection();
        }
    }

    private void OnHeadsetMounted()
    {
        Debug.Log("Headset mounted.");
        EnsureConnection();
    }

    private int notInRoomTicks = 0;

    public void EnsureConnection()
    {
        Debug.Log($"[UserStatusSend] EnsureConnection - State: {PhotonNetwork.NetworkClientState}");

        if (PhotonNetwork.InRoom)
        {
            notInRoomTicks = 0;
            TrySendStatus("online");
            return;
        }

        if (PhotonNetwork.NetworkClientState == ClientState.Disconnected || 
            PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
        {
            notInRoomTicks = 0;
            Debug.Log("Photon disconnected. Reconnecting...");
            ConfigurePhotonAndConnect();
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer || 
                 PhotonNetwork.IsConnectedAndReady)
        {
            notInRoomTicks = 0;
            Debug.Log("Connected to master, joining room: " + roomName);
            JoinOrCreateGameRoom();
        }
        else
        {
            notInRoomTicks++;
            if (notInRoomTicks >= 2)
            {
                Debug.LogWarning($"[UserStatusSend] Stuck in state {PhotonNetwork.NetworkClientState}. Forcing disconnect to reset.");
                notInRoomTicks = 0;
                PhotonNetwork.Disconnect();
            }
        }
    }

    private void OnHeadsetUnmounted()
    {
        TrySendStatus("offline");

        if (vrVideoPlayer != null)
        {
            vrVideoPlayer.Stop();
        }

        if (sphere != null)
        {
            sphere.SetActive(false);
        }

        Debug.Log("Headset unmounted.");
    }

    private bool isExplicitlyPaused = false;

    void Update()
    {
        try 
        {
            if (Time.unscaledTime >= nextConnectionCheckTime)
            {
                nextConnectionCheckTime = Time.unscaledTime + 2f;
                EnsureConnection();
            }

            if (vrVideoPlayer == null)
            {
                return;
            }

            bool isPlaying = vrVideoPlayer.isPlaying;
            bool showSphere = isPlaying || isExplicitlyPaused;

            if (aviso != null)
            {
                aviso.SetActive(!showSphere);
            }

            if (ambiente != null)
            {
                ambiente.SetActive(!showSphere);
            }

            if (sphere != null)
            {
                sphere.SetActive(showSphere);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in Update: " + ex.ToString());
        }
    }

    [PunRPC]
    public void ReceivePlayCommand(int targetUserID)
    {
        Debug.Log($"[UserStatusSend] ReceivePlayCommand - target: {targetUserID}, my userID: {userID}");
        if (targetUserID == userID || targetUserID == -1)
        {
            isExplicitlyPaused = false;
            if (vrVideoPlayer != null)
            {
                vrVideoPlayer.Play();
            }
        }
    }

    [PunRPC]
    public void UpdateVideoData(int targetUserID)
    {
        if (targetUserID == userID || targetUserID == -1)
        {
            videoPlayerCtrl.NextVideo();
        }
    }

    [PunRPC]
    public void ReceivePrevCommand(int targetUserID)
    {
        if (targetUserID == userID || targetUserID == -1)
        {
            videoPlayerCtrl.PrevVideo();
        }
    }

    [PunRPC]
    public void ReceivePauseCommand(int targetUserID)
    {
        Debug.Log($"[UserStatusSend] ReceivePauseCommand - target: {targetUserID}, my userID: {userID}");
        if (targetUserID == userID || targetUserID == -1)
        {
            isExplicitlyPaused = true;
            if (vrVideoPlayer != null)
            {
                vrVideoPlayer.Pause();
            }
        }
    }

    [PunRPC]
    public void ReceiveStopCommand(int targetUserID)
    {
        Debug.Log($"[UserStatusSend] ReceiveStopCommand - target: {targetUserID}, my userID: {userID}");
        if (targetUserID == userID || targetUserID == -1)
        {
            isExplicitlyPaused = false;
            if (vrVideoPlayer != null)
            {
                vrVideoPlayer.Stop();
            }
        }
    }

    [PunRPC]
    public void ReceiveSelectVideoCommand(int targetUserID, string videoUrl)
    {
        Debug.Log($"ReceiveSelectVideoCommand: {videoUrl} (target: {targetUserID}, my userID: {userID})");
        if (targetUserID == userID || targetUserID == -1)
        {
            string originalFileName = System.IO.Path.GetFileName(videoUrl);
            string ptFileName = originalFileName;
            
            // Map common names to _PT versions
            if (ptFileName.Contains("Amazonia")) ptFileName = "Amazonia_PT.mp4";
            else if (ptFileName.Contains("Lencois")) ptFileName = "Lencois_PT.mp4";
            else if (ptFileName.Contains("Noronha")) ptFileName = "Noronha_PT.mp4";
            else if (ptFileName.Contains("Pantanal")) ptFileName = "Pantanal_PT.mp4";
            else if (ptFileName.Contains("Rio")) ptFileName = "Rio_PT.mp4";

            string appDataPath = $"/sdcard/Android/data/{Application.identifier}/files";
            string appDataPathPT = System.IO.Path.Combine(appDataPath, ptFileName);
            string persistentPathPT = System.IO.Path.Combine(Application.persistentDataPath, ptFileName);
            string downloadPathPT = System.IO.Path.Combine("/storage/emulated/0/Download", ptFileName);
            string downloadPathOrig = System.IO.Path.Combine("/storage/emulated/0/Download", originalFileName);
            string persistentPathOrig = System.IO.Path.Combine(Application.persistentDataPath, originalFileName);

            Debug.Log("persistentDataPath = " + Application.persistentDataPath);
            Debug.Log("Procurando video em: " + appDataPathPT);

            string resolvedUrl = originalFileName; // default to original for StreamingAssets fallback
            Evereal.VRVideoPlayer.VideoSource sourceType = Evereal.VRVideoPlayer.VideoSource.FROM_STREAMING_ASSETS;

            if (System.IO.File.Exists(appDataPathPT))
            {
                resolvedUrl = appDataPathPT;
                sourceType = Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL;
                Debug.Log("Loading video from app data (PT): " + resolvedUrl);
            }
            else if (System.IO.File.Exists(persistentPathPT))
            {
                resolvedUrl = persistentPathPT;
                sourceType = Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL;
                Debug.Log("Loading video from persistentDataPath (PT): " + resolvedUrl);
            }
            else if (System.IO.File.Exists(downloadPathPT))
            {
                resolvedUrl = downloadPathPT;
                sourceType = Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL;
                Debug.Log("Loading video from Download directory (PT): " + resolvedUrl);
            }
            else if (System.IO.File.Exists(downloadPathOrig))
            {
                resolvedUrl = downloadPathOrig;
                sourceType = Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL;
                Debug.Log("Loading video from Download directory (Orig): " + resolvedUrl);
            }
            else if (System.IO.File.Exists(persistentPathOrig))
            {
                resolvedUrl = persistentPathOrig;
                sourceType = Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL;
                Debug.Log("Loading video from persistentDataPath (Orig): " + resolvedUrl);
            }
            else
            {
                Debug.Log("Loading video from default StreamingAssets source: " + resolvedUrl);
            }

            vrVideoPlayer.SetSource(sourceType);

            try
            {
                vrVideoPlayer.Load(resolvedUrl, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading video: " + ex.ToString());
            }
            
            if (sphere != null) sphere.SetActive(true);
        }
    }

    [PunRPC]
    public void ReceiveMessage(int targetUserID, string message)
    {
        if (targetUserID == userID || targetUserID == -1)
        {
            if (Mensagem != null)
            {
                Mensagem.gameObject.SetActive(true);
                Mensagem.text = message;
            }

            StartCoroutine(DesativarMensagem());
        }
    }

    private IEnumerator SendVideoDataRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (vrVideoPlayer != null && vrVideoPlayer.isPlaying)
            {
                SendVideoData();
            }
        }
    }

    private void SendVideoData()
    {
        if (vrVideoPlayer == null || !PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
        {
            return;
        }

        string videoName = vrVideoPlayer.GetFileName();
        double currentTime = vrVideoPlayer.time;
        double totalTime = vrVideoPlayer.length;
        string url = vrVideoPlayer.videoUrl;
        bool isPlaying = vrVideoPlayer.isPlaying;

        photonView.RPC("UpdateVideoData33", RpcTarget.Others, userID, videoName, url, isPlaying, currentTime, totalTime);
    }

    [PunRPC]
    public void UpdateVideoData(int targetUserID, string videoName, string url, bool isPlaying, double currentTime, double totalTime)
    {
        Debug.Log($"User {targetUserID}: {videoName} at {currentTime}/{totalTime}");
    }

    [PunRPC]
    public void UpdateVideoData33(int targetUserID, string videoName, string url, bool isPlaying, double currentTime, double totalTime)
    {
        // Handled by tablet
    }

    [PunRPC]
    public void SyncVideoTime(int targetUserID, float newTime)
    {
        if (targetUserID == userID || targetUserID == -1)
        {
            vrVideoPlayer.time = newTime;
            Debug.Log($"Syncing video time to {newTime}s for user {userID}");
        }
    }

    private IEnumerator ReconnectWithDelay()
    {
        yield return new WaitForSeconds(2f);
        reconnectRoutine = null;
        ConfigurePhotonAndConnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (reconnectRoutine != null)
        {
            StopCoroutine(reconnectRoutine);
        }

        reconnectRoutine = StartCoroutine(ReconnectWithDelay());
    }

    private IEnumerator DesativarMensagem()
    {
        yield return new WaitForSeconds(5);

        if (Mensagem != null)
        {
            Mensagem.gameObject.SetActive(false);
        }
    }

    private void TrySendStatus(string status)
    {
        if (photonView != null && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            photonView.RPC("UpdateStatus", RpcTarget.Others, userID, status);
        }
    }
}
