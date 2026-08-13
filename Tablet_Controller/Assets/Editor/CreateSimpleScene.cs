using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Photon.Pun;

public class CreateSimpleScene
{
    [MenuItem("Tools/Create Simple Tablet Scene")]
    public static void CreateScene()
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        GameObject goCamera = new GameObject("Main Camera");
        Camera cam = goCamera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.06f, 0.09f, 1f); // #0a0f17 Deep Dark
        
        GameObject goEvent = new GameObject("EventSystem");
        goEvent.AddComponent<UnityEngine.EventSystems.EventSystem>();
        goEvent.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        GameObject goCanvas = new GameObject("Canvas");
        Canvas canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = goCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        goCanvas.AddComponent<GraphicRaycaster>();

        GameObject goController = new GameObject("Controller");
        PhotonView pv = goController.AddComponent<PhotonView>();
        pv.ViewID = 1;
        SimpleController sc = goController.AddComponent<SimpleController>();

        GameObject goPanel = new GameObject("Panel");
        goPanel.transform.SetParent(goCanvas.transform, false);
        RectTransform rtPanel = goPanel.AddComponent<RectTransform>();
        rtPanel.anchorMin = Vector2.zero;
        rtPanel.anchorMax = Vector2.one;
        rtPanel.offsetMin = new Vector2(25, 20);
        rtPanel.offsetMax = new Vector2(-25, -20);
        VerticalLayoutGroup vlg = goPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 14;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        
        // Header
        GameObject goHeader = new GameObject("Header");
        goHeader.transform.SetParent(goPanel.transform, false);
        LayoutElement leHeader = goHeader.AddComponent<LayoutElement>();
        leHeader.minHeight = 65;
        leHeader.preferredHeight = 65;
        leHeader.flexibleHeight = 0;
        
        Image imgHeader = goHeader.AddComponent<Image>();
        imgHeader.color = new Color(0.08f, 0.11f, 0.18f, 0.85f);
        
        GameObject goHeaderTxt = new GameObject("Text");
        goHeaderTxt.transform.SetParent(goHeader.transform, false);
        RectTransform rtHeaderTxt = goHeaderTxt.AddComponent<RectTransform>();
        rtHeaderTxt.anchorMin = Vector2.zero;
        rtHeaderTxt.anchorMax = Vector2.one;
        rtHeaderTxt.offsetMin = new Vector2(20, 0);
        rtHeaderTxt.offsetMax = new Vector2(-20, 0);
        
        Text txtStatus = goHeaderTxt.AddComponent<Text>();
        txtStatus.text = "BRASIL VR  •  PAINEL DE CONTROLE";
        txtStatus.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtStatus.fontSize = 30;
        txtStatus.color = Color.white;
        txtStatus.alignment = TextAnchor.MiddleCenter;
        txtStatus.supportRichText = true;
        sc.statusHeader = txtStatus;
        
        // 5 Videos with exact correct names and clean formatting
        string[] videoTitles = { "Amazônia", "Lençóis\nMaranhenses", "Fernando de\nNoronha", "Pantanal", "Rio de\nJaneiro" };
        string[] videoUrls = { "Videos/Amazonia.mp4", "Videos/Lencois Maranheses.mp4", "Videos/Fernando de Noronha.mp4", "Videos/Pantanal.mp4", "Videos/Rio de Janeiro.mp4" };

        for (int i = 0; i < 4; i++)
        {
            GameObject goRow = new GameObject("Row_" + (i + 1));
            goRow.transform.SetParent(goPanel.transform, false);
            LayoutElement leRow = goRow.AddComponent<LayoutElement>();
            leRow.flexibleHeight = 1;
            
            HorizontalLayoutGroup hlg = goRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 14;

            // Player Badge & Status
            GameObject goPlayerBadge = new GameObject("Badge_" + (i + 1));
            goPlayerBadge.transform.SetParent(goRow.transform, false);
            LayoutElement leBadge = goPlayerBadge.AddComponent<LayoutElement>();
            leBadge.minWidth = 260;
            leBadge.preferredWidth = 260;
            leBadge.flexibleWidth = 0;
            leBadge.flexibleHeight = 1;

            Image imgBadge = goPlayerBadge.AddComponent<Image>();
            imgBadge.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            sc.playerStatusBadges[i] = imgBadge;

            GameObject goText = new GameObject("Text");
            goText.transform.SetParent(goPlayerBadge.transform, false);
            RectTransform rtText = goText.AddComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero;
            rtText.anchorMax = Vector2.one;
            rtText.offsetMin = new Vector2(16, 8);
            rtText.offsetMax = new Vector2(-16, -8);

            Text txt = goText.AddComponent<Text>();
            txt.text = $"<b><size=38>{i + 1}</size></b>   <color=#FF4D4D>○ OFFLINE</color>";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.supportRichText = true;
            sc.playerStatusTexts[i] = txt;

            // 5 Equal-Proportion Video Buttons
            for (int v = 0; v < videoTitles.Length; v++)
            {
                GameObject goBtn = new GameObject("Btn_" + v);
                goBtn.transform.SetParent(goRow.transform, false);
                LayoutElement leBtn = goBtn.AddComponent<LayoutElement>();
                leBtn.flexibleWidth = 1;
                leBtn.flexibleHeight = 1;

                Image btnImg = goBtn.AddComponent<Image>();
                btnImg.color = new Color(0.11f, 0.18f, 0.30f, 1f); // Sleek deep navy
                
                Button btn = goBtn.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = new Color(0.11f, 0.18f, 0.30f, 1f);
                cb.highlightedColor = new Color(0.18f, 0.28f, 0.48f, 1f);
                cb.pressedColor = new Color(0.02f, 0.65f, 0.42f, 1f); // Emerald Green Feedback
                cb.selectedColor = new Color(0.14f, 0.22f, 0.36f, 1f);
                cb.colorMultiplier = 1f;
                cb.fadeDuration = 0.1f;
                btn.colors = cb;

                GameObject goBtnTxt = new GameObject("Text");
                goBtnTxt.transform.SetParent(goBtn.transform, false);
                Text btxt = goBtnTxt.AddComponent<Text>();
                btxt.text = videoTitles[v];
                btxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                btxt.fontSize = 26;
                btxt.fontStyle = FontStyle.Bold;
                btxt.color = new Color(0.95f, 0.97f, 1f, 1f);
                btxt.alignment = TextAnchor.MiddleCenter;
                btxt.lineSpacing = 1.15f;
                
                RectTransform btxtRt = btxt.GetComponent<RectTransform>();
                btxtRt.anchorMin = Vector2.zero;
                btxtRt.anchorMax = Vector2.one;
                btxtRt.offsetMin = new Vector2(6, 6);
                btxtRt.offsetMax = new Vector2(-6, -6);

                int slotIndex = i;
                string targetUrl = videoUrls[v];
                
                ButtonProxy proxy = goBtn.AddComponent<ButtonProxy>();
                proxy.controller = sc;
                proxy.slotIndex = slotIndex;
                proxy.videoUrl = targetUrl;
                
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, proxy.OnClick);
            }
        }

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/SimpleTablet.unity");
    }
}
