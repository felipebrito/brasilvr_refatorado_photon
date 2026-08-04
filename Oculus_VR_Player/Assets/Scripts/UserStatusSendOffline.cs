using System.Collections;
using UnityEngine;
using Evereal.VRVideoPlayer;
using TMPro;
using OscSimpl;
using System.Net;
using System.Net.Sockets;

public class UserStatusSendOffline : MonoBehaviour
{
    [Header("OSC Settings")]
    public int oscReceivePort = 7001; // Porta que o tablet envia comandos
    public int oscSendPort = 7000;    // Porta que o tablet escuta status
    private OscIn oscIn;
    private OscOut oscOut;

    [Header("References")]
    public int slotIndex;
    public VRVideoPlayer vrVideoPlayer;
    public VideoPlayerCtrl videoPlayerCtrl;
    public TextMeshPro Mensagem;
    public GameObject aviso;
    public GameObject ambiente;
    public GameObject sphere;

    private string localIP;

    void Start()
    {
        slotIndex = PlayerPrefs.GetInt("VRSlot", 0);
        localIP = GetLocalIPAddress();

        // Configura recebimento
        oscIn = gameObject.AddComponent<OscIn>();
        oscIn.Open(oscReceivePort);

        oscIn.Map("/tablet/play", OfflineReceivePlayCommand);
        oscIn.Map("/tablet/pause", OfflineReceivePauseCommand);
        oscIn.Map("/tablet/nextVideo", OfflineReceiveNextVideo);
        oscIn.Map("/tablet/prevVideo", OfflineReceivePrevVideo);
        oscIn.MapString("/tablet/selectVideo", OfflineReceiveSelectVideoCommand);
        oscIn.MapFloat("/tablet/syncTime", OfflineReceiveSyncTime);
        oscIn.MapString("/tablet/message", OfflineReceiveMessage);

        // Configura envio (Broadcast inicial)
        oscOut = gameObject.AddComponent<OscOut>();
        oscOut.Open(oscSendPort, "255.255.255.255");

        OVRManager.HMDMounted += OnHeadsetMounted;
        OVRManager.HMDUnmounted += OnHeadsetUnmounted;

        StartCoroutine(SendStatusRoutine());
        StartCoroutine(SendVideoDataRoutine());
    }

    private void OnDestroy()
    {
        OVRManager.HMDMounted -= OnHeadsetMounted;
        OVRManager.HMDUnmounted -= OnHeadsetUnmounted;
        if (oscIn != null) oscIn.Close();
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    private void OnHeadsetMounted()
    {
        SendStatus("online");
    }

    private void OnHeadsetUnmounted()
    {
        SendStatus("offline");
        if (vrVideoPlayer != null) vrVideoPlayer.Stop();
        if (sphere != null) sphere.SetActive(false);
    }

    private IEnumerator SendStatusRoutine()
    {
        while (true)
        {
            SendStatus("online"); // Reforça que está online a cada 2 segundos
            yield return new WaitForSeconds(2f);
        }
    }

    private void SendStatus(string status)
    {
        if (oscOut != null && oscOut.isOpen)
        {
            OscMessage msg = new OscMessage("/vr/status");
            msg.Add(slotIndex);
            msg.Add(status);
            msg.Add(localIP);
            oscOut.Send(msg);
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
        if (vrVideoPlayer == null || oscOut == null || !oscOut.isOpen) return;

        string videoName = vrVideoPlayer.GetFileName();
        float currentTime = (float)vrVideoPlayer.time;
        float totalTime = (float)vrVideoPlayer.length;
        string url = vrVideoPlayer.videoUrl;
        int isPlaying = vrVideoPlayer.isPlaying ? 1 : 0;

        OscMessage msg = new OscMessage("/vr/videoData");
        msg.Add(slotIndex);
        msg.Add(videoName);
        msg.Add(url);
        msg.Add(isPlaying);
        msg.Add(currentTime);
        msg.Add(totalTime);
        
        oscOut.Send(msg);
    }

    // --- OSC RECEIVERS ---

    private void OfflineReceivePlayCommand(OscMessage message)
    {
        if (vrVideoPlayer != null)
        {
            vrVideoPlayer.Play();
            if (sphere != null) sphere.SetActive(true);
        }
    }

    private void OfflineReceivePauseCommand(OscMessage message)
    {
        if (vrVideoPlayer != null) vrVideoPlayer.Stop(); // Seguindo a lógica do antigo (Stop) ou Pause()
        if (sphere != null) sphere.SetActive(false);
    }

    private void OfflineReceiveNextVideo(OscMessage message)
    {
        if (videoPlayerCtrl != null) videoPlayerCtrl.NextVideo();
    }

    private void OfflineReceivePrevVideo(OscMessage message)
    {
        if (videoPlayerCtrl != null) videoPlayerCtrl.PrevVideo();
    }

    private void OfflineReceiveSelectVideoCommand(string videoUrl)
    {
        string fileName = System.IO.Path.GetFileName(videoUrl);
        
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
            vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
            resolvedUrl = "file://" + downloadPath;
        }
        else if (System.IO.File.Exists(persistentPath))
        {
            vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
            resolvedUrl = "file://" + persistentPath;
        }
        else
        {
            vrVideoPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.FROM_STREAMING_ASSETS);
            resolvedUrl = fileName;
        }

        vrVideoPlayer.Load(resolvedUrl, true);
        if (sphere != null) sphere.SetActive(true);
    }

    private void OfflineReceiveSyncTime(float newTime)
    {
        if (vrVideoPlayer != null)
        {
            vrVideoPlayer.time = newTime;
        }
    }

    private void OfflineReceiveMessage(string message)
    {
        if (Mensagem != null)
        {
            Mensagem.gameObject.SetActive(true);
            Mensagem.text = message;
            StartCoroutine(DesativarMensagem());
        }
    }

    private IEnumerator DesativarMensagem()
    {
        yield return new WaitForSeconds(5);
        if (Mensagem != null) Mensagem.gameObject.SetActive(false);
    }
}
