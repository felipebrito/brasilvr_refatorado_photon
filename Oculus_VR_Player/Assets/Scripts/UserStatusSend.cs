using System.Collections;
using UnityEngine;
using Photon.Pun;
using Evereal.VRVideoPlayer;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class UserStatusSend : MonoBehaviourPunCallbacks
{
    [Header("Photon")]
    [SerializeField] private string fixedRegion = "sa";
    [SerializeField] private string fixedAppVersion = "1";
    [SerializeField] private string roomName = "RiR-23";

    public int userID;
    public VRVideoPlayer vrVideoPlayer;
    public VideoPlayerCtrl videoPlayerCtrl;
    public TextMeshPro Mensagem;
    public GameObject aviso;
    public GameObject ambiente;
    public GameObject sphere;

    private Coroutine reconnectRoutine;

    void Start()
    {
        roomName = "RiR-23";

#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        }
#endif
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
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        userID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        TrySendStatus("online");

        if (reconnectRoutine != null)
        {
            StopCoroutine(reconnectRoutine);
            reconnectRoutine = null;
        }
        
        StartCoroutine(SendVideoDataRoutine());
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"Left room: {PhotonNetwork.CurrentRoom.Name}");
        TrySendStatus("offline");
        SceneManager.LoadScene(0);
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

    private void OnHeadsetMounted()
    {
        Debug.Log("Headset mounted.");

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon disconnected. Reconnecting...");
            ConfigurePhotonAndConnect();
        }
        else if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Not in a room. Trying to join or create one...");
            JoinOrCreateGameRoom();
        }
        else
        {
            TrySendStatus("online");
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

    void Update()
    {
        if (vrVideoPlayer == null)
        {
            return;
        }

        bool isPlaying = vrVideoPlayer.isPlaying;

        if (aviso != null)
        {
            aviso.SetActive(!isPlaying);
        }

        if (ambiente != null)
        {
            ambiente.SetActive(!isPlaying);
        }

        if (sphere != null)
        {
            sphere.SetActive(isPlaying);
        }
    }

    [PunRPC]
    public void ReceivePlayCommand(int targetUserID)
    {
        if (targetUserID == userID || targetUserID == -1)
        {
            vrVideoPlayer.Play();

            if (sphere != null)
            {
                sphere.SetActive(vrVideoPlayer.isPlaying);
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
        if (targetUserID == userID || targetUserID == -1)
        {
            if (vrVideoPlayer != null)
            {
                vrVideoPlayer.Stop();
            }
            if (sphere != null)
            {
                sphere.SetActive(false);
            }
        }
    }

    [PunRPC]
    public void ReceiveSelectVideoCommand(int targetUserID, string videoUrl)
    {
        Debug.Log("Command received: " + videoUrl);
        if (targetUserID == userID || targetUserID == -1)
        {
            string fileName = System.IO.Path.GetFileName(videoUrl);
            
            // Map common names to _PT versions
            if (fileName.Contains("Amazonia")) fileName = "Amazonia_PT.mp4";
            else if (fileName.Contains("Lencois")) fileName = "Lencois_PT.mp4";
            else if (fileName.Contains("Noronha")) fileName = "Noronha_PT.mp4";
            else if (fileName.Contains("Pantanal")) fileName = "Pantanal_PT.mp4";
            else if (fileName.Contains("Rio")) fileName = "Rio_PT.mp4";

            string resolvedUrl = fileName;

            string downloadPath = System.IO.Path.Combine("/sdcard/Download", fileName);
            string persistentPath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

            if (System.IO.File.Exists(downloadPath))
            {
                Debug.Log("Loading video from Downloads path: " + downloadPath);
                vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
                resolvedUrl = "file://" + downloadPath;
            }
            else if (System.IO.File.Exists(persistentPath))
            {
                Debug.Log("Loading video from persistent path: " + persistentPath);
                vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
                resolvedUrl = "file://" + persistentPath;
            }
            else
            {
                Debug.Log("Loading video from default StreamingAssets source: " + fileName);
                vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.FROM_STREAMING_ASSETS);
                resolvedUrl = fileName;
            }

            vrVideoPlayer.Load(resolvedUrl, true);
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
