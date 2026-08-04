using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using OscSimpl;

public class ControllerScriptOffline : MonoBehaviour
{
    private const string LOG_FORMAT = "[ControllerScriptOffline] {0}";

    [Header("OSC Settings")]
    public int oscReceivePort = 7000;
    public int oscSendPort = 7001;
    private OscIn oscIn;
    
    // Mapeamento dinâmico de IPs dos óculos para poder responder individualmente
    // Chave: SlotIndex (0 a 3), Valor: Endereço IP do óculos
    private Dictionary<int, string> oculusIPs = new Dictionary<int, string>();

    [Header("UI References")]
    [SerializeField] private List<PlayerStatus> playerStatuses; // Lista de PlayerStatus (4 slots)
    [SerializeField] private List<Evereal.VRVideoPlayer.VRVideoPlayer> videoPlayers; // Lista de VideoPlayers (4 Players)

    public TMP_InputField mensagemParaTodos;
    public TMP_InputField mensagemParaUm;
    public bool sendToAll = false;

    public int selectedPlayerID = -1; // Slot Index (0 a 3)

    [Header("Info Individual")]
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI videoTitleText;
    public TextMeshProUGUI videoTime;
    public TextMeshProUGUI videoTimeTotal;
    public GameObject playButton, pauseButton;

    public bool isVideoOpened;
    public int lengthLimit = -1;

    bool isUpdatingSlider;
    
    // Lista de clientes ativos (SlotIndex, está logado)
    private Dictionary<int, bool> activePlayers = new Dictionary<int, bool>();

    void Start()
    {
        // Setup OscIn (para receber msgs dos óculos)
        oscIn = gameObject.AddComponent<OscIn>();
        oscIn.Open(oscReceivePort);

        // Mapeamentos de comandos OSC recebidos
        oscIn.Map("/vr/status", OnReceiveStatus);
        oscIn.Map("/vr/videoData", OnReceiveVideoData);

        UpdatePlayerStatusUI();
    }
    
    void OnDestroy()
    {
        if (oscIn != null) oscIn.Close();
    }

    void Update()
    {
        if (videoPlayers == null) return;
        
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            var vp = videoPlayers[i];
            if (vp != null && vp.texture != null)
            {
                if (playerStatuses != null && i < playerStatuses.Count && playerStatuses[i] != null && playerStatuses[i].videoPreview != null)
                {
                    var ri = playerStatuses[i].videoPreview.GetComponent<UnityEngine.UI.RawImage>();
                    if (ri != null) ri.texture = vp.texture;
                }

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

    // Função auxiliar para enviar OSC
    private void SendOscMessage(string address, params object[] args)
    {
        OscMessage msg = new OscMessage(address);
        foreach(var arg in args)
        {
            if (arg is int) msg.Add((int)arg);
            else if (arg is float) msg.Add((float)arg);
            else if (arg is string) msg.Add((string)arg);
            else if (arg is bool) msg.Add((bool)arg);
        }

        if (sendToAll)
        {
            // Broadcast para todos os IPs conhecidos
            foreach (var kvp in oculusIPs)
            {
                OscOut tempOut = gameObject.AddComponent<OscOut>();
                tempOut.Open(oscSendPort, kvp.Value);
                tempOut.Send(msg);
                Destroy(tempOut, 0.1f);
            }
            // Fallback: se não tiver IPs, enviaria pra broadcast 255.255.255.255, 
            // mas o OscSimpl já deve gerenciar o IP de destino se configurado.
            if (oculusIPs.Count == 0)
            {
                OscOut tempOut = gameObject.AddComponent<OscOut>();
                tempOut.Open(oscSendPort, "255.255.255.255");
                tempOut.Send(msg);
                Destroy(tempOut, 0.1f);
            }
        }
        else if (selectedPlayerID >= 0)
        {
            // Envia só pro selecionado
            string ip = "255.255.255.255"; // Broadcast fallback
            if (oculusIPs.ContainsKey(selectedPlayerID))
                ip = oculusIPs[selectedPlayerID];
                
            OscOut tempOut = gameObject.AddComponent<OscOut>();
            tempOut.Open(oscSendPort, ip);
            tempOut.Send(msg);
            Destroy(tempOut, 0.1f);
        }
    }

    // --- RECEIVERS DE OSC DOS ÓCULOS ---

    // Ex: Óculos envia "/vr/status" [int slotIndex] [string status] [string (opcional) ip]
    private void OnReceiveStatus(OscMessage message)
    {
        if (!message.TryGet(0, out int slotIndex)) return;
        string status = "";
        if (!message.TryGet(1, ref status)) return;

        string ip = "";
        if (message.TryGet(2, ref ip))
        {
            oculusIPs[slotIndex] = ip;
        }

        if (slotIndex >= 0 && slotIndex < playerStatuses.Count)
        {
            if (status == "online")
            {
                activePlayers[slotIndex] = true;
                playerStatuses[slotIndex].SetUserON();
            }
            else
            {
                activePlayers[slotIndex] = false;
                playerStatuses[slotIndex].SetUserOFF();
            }
            UpdatePlayerStatusUI();
        }
    }

    // Ex: "/vr/videoData" [int slotIndex] [string title] [string url] [int isPlaying] [float currentTime] [float totalTime]
    private void OnReceiveVideoData(OscMessage message)
    {
        if (!message.TryGet(0, out int slotIndex)) return;
        if (slotIndex < 0 || slotIndex >= playerStatuses.Count) return;
        
        string title = "";
        message.TryGet(1, ref title);
        string url = "";
        message.TryGet(2, ref url);
        message.TryGet(3, out int isPlayingInt);
        bool isPlaying = isPlayingInt == 1;
        message.TryGet(4, out float currentTime);
        message.TryGet(5, out float totalTime);

        if (!TryGetVideoPlayer(slotIndex, out var videoPlayer) || !TryGetPlayerStatus(slotIndex, out var playerStatus))
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
            playerStatus.timelineSlider.maxValue = totalTime;
            playerStatus.timelineSlider.value = currentTime;
        }
        playerStatus.SetVideoTime(currentTime.ToString());
        playerStatus.SetVideoTotalTime(totalTime.ToString());
        playerStatus.SetTitleName(title);
        
        if (playerStatus.contectado != null)
            playerStatus.contectado.SetActive(true);

        if (selectedPlayerID == slotIndex)
        {
            if (playerName != null) playerName.text = "Player " + (slotIndex + 1);
            if (videoTitleText != null) videoTitleText.text = title;
            if (playButton != null) playButton.SetActive(!isPlaying);
            if (pauseButton != null) pauseButton.SetActive(isPlaying);
        }

        isUpdatingSlider = false;
        SetTime(currentTime, playerStatus.videoCurrentTime);
        SetTime(totalTime, playerStatus.videoTotalTime);
    }

    // --- INTERAÇÕES DE UI DO TABLET ---

    public void SelectPlayer(int slotNumber)
    {
        sendToAll = false;
        int slotIndex = slotNumber - 1; // 0-based

        if (activePlayers.ContainsKey(slotIndex) && activePlayers[slotIndex])
        {
            selectedPlayerID = slotIndex;
            Debug.Log($"Player {slotNumber} selecionado. ID offline: {selectedPlayerID}");
        }
        else
        {
            selectedPlayerID = -1;
            Debug.LogWarning($"Nenhum jogador online no slot {slotIndex}.");
        }
    }

    public void SendPlayCommand()
    {
        SendOscMessage("/tablet/play");
    }

    public void SendPauseCommand()
    {
        SendOscMessage("/tablet/pause");
    }

    public void SendNextVideo()
    {
        SendOscMessage("/tablet/nextVideo");
    }

    public void SendPrevVideo()
    {
        SendOscMessage("/tablet/prevVideo");
    }

    public void SendSelectVideoCommand(string videoUrl)
    {
        // Envia o comando para o óculos
        SendOscMessage("/tablet/selectVideo", videoUrl);

        // Atualiza UI local
        if (sendToAll)
        {
            foreach (var videoPlayer in videoPlayers)
            {
                if (videoPlayer != null) videoPlayer.Load(videoUrl, true);
            }
        }
        else if (selectedPlayerID >= 0)
        {
            if (!TryGetVideoPlayer(selectedPlayerID, out var videoPlayer) || !TryGetPlayerStatus(selectedPlayerID, out var playerStatus))
                return;

            videoPlayer.Load(videoUrl, true);
            if (playerStatus.videoPreview != null) playerStatus.videoPreview.SetActive(true);
            if (playerStatus.timelineSlider != null) playerStatus.timelineSlider.maxValue = (float)videoPlayer.length;
        }
    }

    public void OnTimelineSliderChanged()
    {
        if (isUpdatingSlider) return;
        if (selectedPlayerID < 0) return;
        
        if (!TryGetPlayerStatus(selectedPlayerID, out var playerStatus) || playerStatus.timelineSlider == null)
            return;
        if (!TryGetVideoPlayer(selectedPlayerID, out var videoPlayer))
            return;

        float newTime = playerStatus.timelineSlider.value;
        SendOscMessage("/tablet/syncTime", newTime);
        
        videoPlayer.time = newTime;
        SetTime(newTime, videoTime);
    }

    public void OpenCloseVideo(bool value)
    {
        isVideoOpened = value;
    }

    public void SetAllPlayers(bool value)
    {
        sendToAll = value;
    }

    public void EnviarMensagemParaTodos()
    {
        SendOscMessage("/tablet/message", mensagemParaTodos.text);
    }

    public void EnviarMensagemParaUm()
    {
        if (selectedPlayerID >= 0)
        {
            SendOscMessage("/tablet/message", mensagemParaUm.text);
        }
    }

    // --- HELPERS ---

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
        if (textObject == null) return;
        if (lengthLimit > 0 && text.Length > lengthLimit)
        {
            text = text.Substring(0, lengthLimit) + "...";
        }
        textObject.text = text;
    }

    private bool TryGetPlayerStatus(int index, out PlayerStatus playerStatus)
    {
        playerStatus = null;
        if (index < 0 || index >= playerStatuses.Count) return false;
        playerStatus = playerStatuses[index];
        return playerStatus != null;
    }

    private bool TryGetVideoPlayer(int index, out Evereal.VRVideoPlayer.VRVideoPlayer videoPlayer)
    {
        videoPlayer = null;
        if (index < 0 || index >= videoPlayers.Count) return false;
        videoPlayer = videoPlayers[index];
        return videoPlayer != null;
    }

    private void UpdatePlayerStatusUI()
    {
        for (int i = 0; i < playerStatuses.Count; i++)
        {
            var playerStatus = playerStatuses[i];
            if (playerStatus == null) continue;
            
            if (activePlayers.ContainsKey(i) && activePlayers[i])
            {
                playerStatus.SetTitleName($"Player {i + 1}: Online");
            }
            else
            {
                playerStatus.SetUserOFF();
                playerStatus.SetTitleName("Offline");
            }
        }
    }
}
