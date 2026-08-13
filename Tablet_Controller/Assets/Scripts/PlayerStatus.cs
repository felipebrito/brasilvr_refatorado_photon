using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscSimpl.Examples;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

public class PlayerStatus : MonoBehaviour
{

    public ProgressBar videoTime;
    public TextMeshProUGUI userID;
    public TextMeshProUGUI videoTitle;
    public TextMeshProUGUI videoCurrentTime;
    public TextMeshProUGUI videoTotalTime;

    public Button button; // ativar ou desativar o botão do usuário

    public SwitchManager conected;
    public TextMeshProUGUI conectedText;
    public GameObject contectado, desconectado,videoPreview;
    public Slider timelineSlider;
    float totalTime;
    float currentTime;
    public int user;

    public void Start()
    {
        desconectado.SetActive(true);
        contectado.SetActive(false);
        //button.interactable = false; // Liberado para pre-load

    }


    public void SetTitleName(string name)
    {
        videoTitle.text = name;
    }

    public void SetUserON()
    {
        contectado.SetActive(true);

        // button.interactable = true;
        Animator anim = contectado.GetComponent<Animator>();

        anim.Play("In");
        desconectado.SetActive(false);
        contectado.SetActive(true);
        conected.SetOn();

        CanvasGroup cg = contectado.GetComponent<CanvasGroup>();
        if (cg == null) cg = contectado.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
    }

    public void SetUserOFF()
    {
        ////button.interactable = false; // Liberado para pre-load
        contectado.SetActive(true);
        Animator anim = contectado.GetComponent<Animator>();
        anim.Play("Out");
        conected.SetOff();
        desconectado.SetActive(true);
        //contectado.SetActive(false);

        CanvasGroup cg = contectado.GetComponent<CanvasGroup>();
        if (cg == null) cg = contectado.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void SetVideoTotalTime(string value)
    {
        float valor = float.Parse(value);
        totalTime = valor;
        timelineSlider.maxValue = totalTime;
    }
    public void SetVideoTime(string value)
    {
        float valor = float.Parse(value);
        currentTime = valor;
        timelineSlider.value = currentTime;
        videoTime.minValue = 0;
        videoTime.valueLimit = totalTime;
        videoTime.maxValue = totalTime;
        videoTime.currentPercent = currentTime;


    }


   
}
