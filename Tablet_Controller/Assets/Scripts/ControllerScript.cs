using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class ControllerScript : MonoBehaviourPunCallbacks
{
    private const string LOG_FORMAT = "[ControllerScript] {0}";

    [Header("Photon")]
    [SerializeField] private string fixedRegion = "sa";
    [SerializeField] private string fixedAppVersion = "1";
    [SerializeField] private string roomName = "GameRoom";

    [SerializeField] private Transform uiListParent; // Transform para a lista na UI
    [SerializeField] private GameObject uiListItemPrefab; // Prefab do item na lista
    [SerializeField] private List<PlayerStatus> playerStatuses; // Lista de PlayerStatus (4 slots)
    [SerializeField] private List<Evereal.VRVideoPlayer.VRVideoPlayer> videoPlayers; // Lista de VideoPlayers (4 Players)
    
    // Mapeia o ActorNumber (ID real do Photon) para o índice do slot na UI (0 a 3)
    private Dictionary<int, int> playerStatusMap = new Dictionary<int, int>(); 

    public TMP_InputField mensagemParaTodos;
    public TMP_InputField mensagemParaUm;
    public bool sendToAll = false;

    public int selectedPlayerID = -1; // Armazena o userID que o óculos espera (ActorNumber - 1)
    
    [Header("Info Individual")]
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI videoTitleText;
    public TextMeshProUGUI videoTime;
    public TextMeshProUGUI videoTimeTotal;
    public GameObject playButton, pauseButton;

    public bool isVideoOpened;
    public int lengthLimit = -1;

    bool isUpdatingSlider;

    void Start()
    {
        if (PhotonNetwork.PhotonServerSettings == null)
        {
            Debug.LogErrorFormat(LOG_FORMAT, "PhotonServerSettings não encontrado.");
            return;
        }

        // Force the same cloud region on every install/device.
        if (!string.IsNullOrWhiteSpace(fixedRegion))
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = fixedRegion.Trim().ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(fixedAppVersion))
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = fixedAppVersion.Trim();
        }

        // Ignore any previous best-region cache from old installs.
        ServerSettings.ResetBestRegionCodeInPreferences();

        // Conecta ao Photon e tenta entrar ou criar a sala
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        if (videoPlayers == null) return;
        
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            var vp = videoPlayers[i];
            if (vp != null && vp.texture != null)
            {
                // Tenta aplicar na RawImage da UI, se existir
                if (playerStatuses != null && i < playerStatuses.Count && playerStatuses[i] != null && playerStatuses[i].videoPreview != null)
                {
                    var ri = playerStatuses[i].videoPreview.GetComponent<UnityEngine.UI.RawImage>();
                    if (ri != null) ri.texture = vp.texture;
                }

                // Aplica a textura na esfera (MeshRenderer) dentro do componente
                var meshRenderers = vp.GetComponentsInChildren<MeshRenderer>();
                foreach (var mr in meshRenderers)
                {
                    if (mr != null && mr.material != null)
                    {
                        mr.material.mainTexture = vp.texture;
                    }
                }
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado ao Photon Master.");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 7 };
        // Mapeia para JoinOrCreateRoom para dar suporte a ambas as ordens de inicialização
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Sala criada ou entrada: " + PhotonNetwork.CurrentRoom.Name);

        // O Controlador precisa registrar jogadores que já estavam na sala antes dele entrar
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.IsLocal)
            {
                if (!playerStatusMap.ContainsKey(player.ActorNumber))
                {
                    int playerIndex = GetAvailablePlayerIndex();
                    if (playerIndex >= 0)
                    {
                        Debug.Log($"Jogador já na sala: {player.ActorNumber}, atribuído ao índice {playerIndex}");
                        playerStatuses[playerIndex].SetUserON();
                        playerStatusMap[player.ActorNumber] = playerIndex;
                        UpdatePlayerStatusUI();
                    }
                    else
                    {
                        Debug.LogWarning("Não há slots disponíveis para jogadores pré-existentes.");
                    }
                }
            }
        }

        photonView.RPC("ReceiveOnlineCheckCommand", RpcTarget.OthersBuffered);
        photonView.RPC("NotifyRoomCreated", RpcTarget.All);
    }

    private int GetAvailablePlayerIndex()
    {
        for (int i = 0; i < playerStatuses.Count; i++)
        {
            if (playerStatuses[i] != null && playerStatuses[i].conected != null && !playerStatuses[i].conected.isOn)
            {
                return i;
            }
        }
        return -1;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // O controlador não se adiciona à própria lista
        if (newPlayer.IsLocal) return;

        int playerIndex = GetAvailablePlayerIndex();
        if (playerIndex >= 0)
        {
            Debug.Log($"Jogador entrou na sala: {newPlayer.ActorNumber}, atribuído ao índice {playerIndex}");
            playerStatuses[playerIndex].SetUserON();
            playerStatusMap[newPlayer.ActorNumber] = playerIndex;
            UpdatePlayerStatusUI();
        }
        else
        {
            Debug.LogWarning("Não há slots disponíveis para novos jogadores.");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerStatusMap.TryGetValue(otherPlayer.ActorNumber, out int playerIndex))
        {
            Debug.Log($"Jogador saiu da sala: {otherPlayer.ActorNumber}, removido do índice {playerIndex}");
            playerStatuses[playerIndex].SetUserOFF();
            playerStatusMap.Remove(otherPlayer.ActorNumber);
            UpdatePlayerStatusUI();
        }
    }

    public void SelectPlayer(int slotNumber)
    {
        sendToAll = false;
        int slotIndex = slotNumber - 1; // Converte para índice 0-based

        // Busca qual ActorNumber está associado a este slot
        int actorNumber = -1;
        foreach (var kvp in playerStatusMap)
        {
            if (kvp.Value == slotIndex)
            {
                actorNumber = kvp.Key;
                break;
            }
        }

        if (actorNumber != -1)
        {
            // O óculos espera o userID como (ActorNumber - 1)
            selectedPlayerID = actorNumber - 1; 
            Debug.Log($"Player {slotNumber} selecionado. ID esperado pelo Óculos (userID): {selectedPlayerID}");
        }
        else
        {
            selectedPlayerID = -1;
            Debug.LogWarning($"Nenhum jogador ativo no slot {slotIndex}.");
        }
    }

    // Recebe atualizações de status enviadas pelos óculos
    [PunRPC]
    public void UpdateStatus(int targetUserID, string status)
    {
        // O óculos envia targetUserID como (ActorNumber - 1)
        int actorNumber = targetUserID + 1;
        if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
        {
            if (!TryGetPlayerStatus(playerIndex, out var playerStatus))
                return;

            if (status == "online")
            {
                playerStatus.SetUserON();
            }
            else
            {
                playerStatus.SetUserOFF();
            }
            UpdatePlayerStatusUI();
        }
    }

    public void SendNextVideo()
    {
        // O óculos mapeia o comando de ir para o próximo vídeo com o RPC "UpdateVideoData" (1 parâmetro)
        if (sendToAll)
        {
            photonView.RPC("UpdateVideoData", RpcTarget.All, -1);
        }
        else if (selectedPlayerID >= 0)
        {
            photonView.RPC("UpdateVideoData", RpcTarget.All, selectedPlayerID);
        }
    }

    public void SendPrevVideo()
    {
        if (sendToAll)
        {
            photonView.RPC("ReceivePrevCommand", RpcTarget.All, -1);
        }
        else if (selectedPlayerID >= 0)
        {
            photonView.RPC("ReceivePrevCommand", RpcTarget.All, selectedPlayerID);
        }
    }

    [PunRPC]
    public void NotifyRoomCreated()
    {
        Debug.Log("Sala criada.");
    }

    public void SendPlayCommand()
    {
        if (sendToAll)
        {
            photonView.RPC("ReceivePlayCommand", RpcTarget.All, -1);
        }
        else if (selectedPlayerID >= 0)
        {
            photonView.RPC("ReceivePlayCommand", RpcTarget.All, selectedPlayerID);
        }
    }

    public void SendPauseCommand()
    {
        if (sendToAll)
        {
            photonView.RPC("ReceivePauseCommand", RpcTarget.All, -1);
        }
        else if (selectedPlayerID >= 0)
        {
            photonView.RPC("ReceivePauseCommand", RpcTarget.All, selectedPlayerID);
        }
    }

    public void OpenCloseVideo(bool value)
    {
        isVideoOpened = value;
    }

    // Recebe dados de vídeo do óculos (atualiza o preview local no celular)
    [PunRPC]
    public void UpdateVideoData33(int userID, string title, string url, bool isPlaying, double currentTime, double totalTime)
    {
        // O óculos envia userID como (ActorNumber - 1)
        int actorNumber = userID + 1;

        if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
        {
            if (!TryGetVideoPlayer(playerIndex, out var videoPlayer) || !TryGetPlayerStatus(playerIndex, out var playerStatus))
                return;

            string fileName = System.IO.Path.GetFileName(url);
            string correctUrl = "Videos/" + fileName;

            if (isPlaying)
            {
                if (videoPlayer.videoUrl != correctUrl)
                    videoPlayer.Load(correctUrl, true);
                else if (!videoPlayer.isPlaying)
                    videoPlayer.Play();
            }
            else
            {
                if (videoPlayer.isPlaying)
                    videoPlayer.Pause();
            }

            if (!isPlaying || Mathf.Abs((float)(videoPlayer.time - currentTime)) > 1.5f)
            {
                videoPlayer.time = currentTime;
            }
            
            isUpdatingSlider = true;
            if (playerStatus.timelineSlider != null)
            {
                playerStatus.timelineSlider.maxValue = (float)totalTime;
                playerStatus.timelineSlider.value = (float)currentTime;
            }
            playerStatus.SetVideoTime(currentTime.ToString());
            playerStatus.SetVideoTotalTime(totalTime.ToString());
            playerStatus.SetTitleName(title);
            if (playerStatus.contectado != null)
                playerStatus.contectado.SetActive(true);

            if (selectedPlayerID == userID)
            {
                if (playerName != null) playerName.text = "Player " + (playerIndex + 1);
                if (videoTitleText != null) videoTitleText.text = title;
                if (playButton != null) playButton.SetActive(!isPlaying);
                if (pauseButton != null) pauseButton.SetActive(isPlaying);
            }

            isUpdatingSlider = false;
            SetTime(currentTime, playerStatus.videoCurrentTime);
            SetTime(totalTime, playerStatus.videoTotalTime);
        }
    }

    // Mantido por compatibilidade
    [PunRPC]
    public void UpdateVideoData(int targetUserID)
    {
    }

    public void OnTimelineSliderChanged()
    {
        if (isUpdatingSlider) return;

        // O óculos espera o userID como (ActorNumber - 1)
        if (selectedPlayerID >= 0)
        {
            int actorNumber = selectedPlayerID + 1;
            if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
            {
                if (!TryGetPlayerStatus(playerIndex, out var playerStatus) || playerStatus.timelineSlider == null)
                    return;
                if (!TryGetVideoPlayer(playerIndex, out var videoPlayer))
                    return;

                float newTime = playerStatus.timelineSlider.value;
                photonView.RPC("SyncVideoTime", RpcTarget.All, selectedPlayerID, newTime);
                videoPlayer.time = newTime;
                SetTime(newTime, videoTime);
            }
        }
    }

    public void SendSelectVideoCommand(string videoUrl)
    {
        if (sendToAll)
        {
            photonView.RPC("ReceiveSelectVideoCommand", RpcTarget.All, -1, videoUrl);
            foreach (var videoPlayer in videoPlayers)
            {
                if (videoPlayer != null)
                    videoPlayer.Load(videoUrl, true);
            }
        }
        else if (selectedPlayerID >= 0)
        {
            int actorNumber = selectedPlayerID + 1;
            if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
            {
                if (!TryGetVideoPlayer(playerIndex, out var videoPlayer) || !TryGetPlayerStatus(playerIndex, out var playerStatus))
                    return;

                videoPlayer.Load(videoUrl, true);
                if (playerStatus.videoPreview != null) playerStatus.videoPreview.SetActive(true);
                if (playerStatus.timelineSlider != null) playerStatus.timelineSlider.maxValue = (float)videoPlayer.length;
                photonView.RPC("ReceiveSelectVideoCommand", RpcTarget.All, selectedPlayerID, videoUrl);
            }
        }
    }

    public void SetAllPlayers(bool value)
    {
        sendToAll = value;
    }

    public void EnviarMensagemParaTodos()
    {
        photonView.RPC("ReceiveMessage", RpcTarget.All, -1, mensagemParaTodos.text);
    }

    public void EnviarMensagemParaUm()
    {
        if (selectedPlayerID >= 0)
        {
            photonView.RPC("ReceiveMessage", RpcTarget.All, selectedPlayerID, mensagemParaUm.text);
        }
    }

    [PunRPC]
    public void SyncVideoTime(int targetUserID, float newTime)
    {
    }

    public void SetTime(double time, TextMeshProUGUI textObject)
    {
        int hours = (int)Mathf.Floor((float)time / 3600);
        int minutes = (int)Mathf.Floor((float)time / 60);
        int seconds = (int)Mathf.Floor((float)time % 60);

        string timeText = string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00"));
        if (hours > 0)
        {
            timeText = string.Format("{0}:{1}", hours, timeText);
        }

        SetText(timeText, textObject);
    }

    public void SetText(string text, TextMeshProUGUI textObject)
    {
        if (textObject == null)
            return;

        if (lengthLimit > 0 && text.Length > lengthLimit)
        {
            text = text.Substring(0, lengthLimit) + "...";
        }

        textObject.text = text;
    }

    private bool TryGetPlayerStatus(int index, out PlayerStatus playerStatus)
    {
        playerStatus = null;
        if (index < 0 || index >= playerStatuses.Count)
            return false;

        playerStatus = playerStatuses[index];
        if (playerStatus == null)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "PlayerStatus ausente no índice {0}.", index);
            return false;
        }

        return true;
    }

    private bool TryGetVideoPlayer(int index, out Evereal.VRVideoPlayer.VRVideoPlayer videoPlayer)
    {
        videoPlayer = null;
        if (index < 0 || index >= videoPlayers.Count)
            return false;

        videoPlayer = videoPlayers[index];
        if (videoPlayer == null)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "VRVideoPlayer ausente no índice {0}.", index);
            return false;
        }

        return true;
    }

    private void UpdatePlayerStatusUI()
    {
        for (int i = 0; i < playerStatuses.Count; i++)
        {
            var playerStatus = playerStatuses[i];
            if (playerStatus == null)
                continue;
            
            // Verifica se há algum jogador ativo mapeado para este slot
            int activeActorNumber = -1;
            foreach (var kvp in playerStatusMap)
            {
                if (kvp.Value == i)
                {
                    activeActorNumber = kvp.Key;
                    break;
                }
            }

            if (activeActorNumber != -1)
            {
                playerStatus.SetUserON();
                playerStatus.SetTitleName($"Player {i + 1}: Online");
            }
            else
            {
                playerStatus.SetUserOFF();
                playerStatus.SetTitleName("Offline");
            }
        }
    }

    [PunRPC]
    public void ReceivePlayCommand(int targetUserID)
    {
        if (targetUserID == -1)
        {
            foreach (var videoPlayer in videoPlayers)
            {
                if (videoPlayer != null)
                    videoPlayer.Play();
            }
            return;
        }

        int actorNumber = targetUserID + 1;
        if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
        {
            if (!TryGetVideoPlayer(playerIndex, out var videoPlayer) || !TryGetPlayerStatus(playerIndex, out var playerStatus))
                return;

            videoPlayer.Play();
            if (playerStatus.contectado != null)
                playerStatus.contectado.SetActive(true);
            playerStatus.SetTitleName("Player " + (playerIndex + 1));
            playerStatus.SetVideoTime(videoPlayer.time.ToString());
            playerStatus.SetVideoTotalTime(videoPlayer.length.ToString());
        }
    }

    [PunRPC]
    public void ReceivePauseCommand(int targetUserID)
    {
        if (targetUserID == -1)
        {
            foreach (var videoPlayer in videoPlayers)
            {
                if (videoPlayer != null)
                    videoPlayer.Pause();
            }
            return;
        }

        int actorNumber = targetUserID + 1;
        if (playerStatusMap.TryGetValue(actorNumber, out int playerIndex))
        {
            if (TryGetVideoPlayer(playerIndex, out var videoPlayer))
                videoPlayer.Pause();
        }
    }

    [PunRPC]
    public void ReceivePrevCommand(int targetUserID)
    {
    }

    [PunRPC]
    public void ReceiveSelectVideoCommand(int targetUserID, string videoUrl)
    {
    }

    [PunRPC]
    public void ReceiveMessage(int targetUserID, string message)
    {
    }
}
