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
        
        Sprite whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

        // Camera
        GameObject goCamera = new GameObject("Main Camera");
        Camera cam = goCamera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.09f, 0.16f, 1f); // #0f172a
        
        // EventSystem
        GameObject goEvent = new GameObject("EventSystem");
        goEvent.AddComponent<UnityEngine.EventSystems.EventSystem>();
        goEvent.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Canvas & Scaler for Samsung Tab S6 Lite (2000x1200)
        GameObject goCanvas = new GameObject("Canvas");
        Canvas canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = goCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2000, 1200);
        scaler.matchWidthOrHeight = 0.5f;
        goCanvas.AddComponent<GraphicRaycaster>();

        // Controller logic
        GameObject goController = new GameObject("Controller");
        PhotonView pv = goController.AddComponent<PhotonView>();
        pv.ViewID = 1;
        SimpleController sc = goController.AddComponent<SimpleController>();

        // Main Fullscreen Root Container
        GameObject goRoot = new GameObject("RootContainer");
        goRoot.transform.SetParent(goCanvas.transform, false);
        RectTransform rtRoot = goRoot.AddComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.offsetMin = new Vector2(24, 18);
        rtRoot.offsetMax = new Vector2(-24, -18);

        VerticalLayoutGroup vlgRoot = goRoot.AddComponent<VerticalLayoutGroup>();
        vlgRoot.childControlWidth = true;
        vlgRoot.childControlHeight = true;
        vlgRoot.childForceExpandWidth = true;
        vlgRoot.childForceExpandHeight = true;
        vlgRoot.spacing = 14;

        // ================= HEADER =================
        GameObject goHeaderFrame = new GameObject("HeaderFrame");
        goHeaderFrame.transform.SetParent(goRoot.transform, false);
        LayoutElement leHeader = goHeaderFrame.AddComponent<LayoutElement>();
        leHeader.minHeight = 90;
        leHeader.preferredHeight = 90;
        leHeader.flexibleHeight = 0;
        leHeader.flexibleWidth = 1;

        Image imgHeaderFrame = goHeaderFrame.AddComponent<Image>();
        imgHeaderFrame.color = new Color(0.22f, 0.45f, 0.85f, 0.6f);

        GameObject goHeader = new GameObject("Header");
        goHeader.transform.SetParent(goHeaderFrame.transform, false);
        RectTransform rtHeader = goHeader.AddComponent<RectTransform>();
        rtHeader.anchorMin = Vector2.zero;
        rtHeader.anchorMax = Vector2.one;
        rtHeader.offsetMin = new Vector2(2, 2);
        rtHeader.offsetMax = new Vector2(-2, -2);

        Image imgHeader = goHeader.AddComponent<Image>();
        imgHeader.color = new Color(0.12f, 0.18f, 0.30f, 0.98f);

        GameObject goHeaderTxt = new GameObject("TitleText");
        goHeaderTxt.transform.SetParent(goHeader.transform, false);
        RectTransform rtHeaderTxt = goHeaderTxt.AddComponent<RectTransform>();
        rtHeaderTxt.anchorMin = Vector2.zero;
        rtHeaderTxt.anchorMax = Vector2.one;
        rtHeaderTxt.offsetMin = new Vector2(20, 4);
        rtHeaderTxt.offsetMax = new Vector2(-20, -4);

        Text txtHeader = goHeaderTxt.AddComponent<Text>();
        txtHeader.text = "<b><size=46><color=#38BDF8>BRASIL</color><color=#FACC15>VR</color></size></b>      <size=22><color=#22C55E>● CONECTADO</color></size>\n<size=18><color=#E2E8F0>PAINEL DE CONTROLE MULTI-VR</color></size>";
        txtHeader.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtHeader.fontSize = 28;
        txtHeader.alignment = TextAnchor.MiddleCenter;
        txtHeader.supportRichText = true;
        sc.statusHeader = txtHeader;

        // ================= 4 PLAYER ROWS =================
        string[] videoTitles = { "Amazônia", "Lençóis\nMaranhenses", "Fernando de\nNoronha", "Pantanal", "Rio de\nJaneiro" };
        string[] videoUrls = { "Videos/Amazonia.mp4", "Videos/Lencois Maranheses.mp4", "Videos/Fernando de Noronha.mp4", "Videos/Pantanal.mp4", "Videos/Rio de Janeiro.mp4" };

        for (int i = 0; i < 4; i++)
        {
            GameObject goRow = new GameObject("Row_" + (i + 1));
            goRow.transform.SetParent(goRoot.transform, false);
            
            LayoutElement leRow = goRow.AddComponent<LayoutElement>();
            leRow.flexibleHeight = 1;
            leRow.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = goRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 12;

            // -------- COLUMN 1: NARROWER BADGE & SESSION CONTROLS --------
            GameObject goBadgeFrame = new GameObject("BadgeFrame_" + (i + 1));
            goBadgeFrame.transform.SetParent(goRow.transform, false);
            LayoutElement leBadgeBox = goBadgeFrame.AddComponent<LayoutElement>();
            leBadgeBox.minWidth = 230; // Compact width
            leBadgeBox.preferredWidth = 230;
            leBadgeBox.flexibleWidth = 0;
            leBadgeBox.flexibleHeight = 1;

            Image imgBadgeFrame = goBadgeFrame.AddComponent<Image>();
            imgBadgeFrame.color = new Color(0.20f, 0.40f, 0.70f, 0.7f);

            GameObject goBadgeBox = new GameObject("BadgeBox");
            goBadgeBox.transform.SetParent(goBadgeFrame.transform, false);
            RectTransform rtBadgeBox = goBadgeBox.AddComponent<RectTransform>();
            rtBadgeBox.anchorMin = Vector2.zero;
            rtBadgeBox.anchorMax = Vector2.one;
            rtBadgeBox.offsetMin = new Vector2(2, 2);
            rtBadgeBox.offsetMax = new Vector2(-2, -2);

            Image imgBadge = goBadgeBox.AddComponent<Image>();
            imgBadge.color = new Color(0.12f, 0.18f, 0.30f, 1f);
            sc.playerStatusBadges[i] = imgBadge;

            HorizontalLayoutGroup hlgBadge = goBadgeBox.AddComponent<HorizontalLayoutGroup>();
            hlgBadge.childControlWidth = true;
            hlgBadge.childControlHeight = true;
            hlgBadge.childForceExpandWidth = false;
            hlgBadge.childForceExpandHeight = true;
            hlgBadge.spacing = 6;
            hlgBadge.padding = new RectOffset(12, 8, 8, 8);

            // Left-aligned Text (Number 1, 2, 3, 4 + Status)
            GameObject goBadgeTxt = new GameObject("StatusText");
            goBadgeTxt.transform.SetParent(goBadgeBox.transform, false);
            LayoutElement leTxt = goBadgeTxt.AddComponent<LayoutElement>();
            leTxt.flexibleWidth = 1;
            leTxt.flexibleHeight = 1;

            Text txtBadge = goBadgeTxt.AddComponent<Text>();
            txtBadge.text = $"<b><size=52>{i + 1}</size></b>   <color=#EF4444><b>○ OFF</b></color>";
            txtBadge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBadge.fontSize = 20;
            txtBadge.color = Color.white;
            txtBadge.alignment = TextAnchor.MiddleLeft;
            txtBadge.supportRichText = true;
            txtBadge.lineSpacing = 1.15f;
            sc.playerStatusTexts[i] = txtBadge;

            // Session Controls Container (PLAY/PAUSE & STOP) - Starts inactive and only appears when playing!
            GameObject goControls = new GameObject("SessionControls");
            goControls.transform.SetParent(goBadgeBox.transform, false);
            LayoutElement leControls = goControls.AddComponent<LayoutElement>();
            leControls.minWidth = 105;
            leControls.preferredWidth = 105;
            leControls.flexibleWidth = 0;
            leControls.flexibleHeight = 1;

            HorizontalLayoutGroup hlgControls = goControls.AddComponent<HorizontalLayoutGroup>();
            hlgControls.childControlWidth = true;
            hlgControls.childControlHeight = true;
            hlgControls.childForceExpandWidth = true;
            hlgControls.childForceExpandHeight = true;
            hlgControls.spacing = 6;
            sc.playerControlContainers[i] = goControls;
            goControls.SetActive(false); // Hidden until a video is started!

            // Play/Pause Button (Large Touch Area)
            GameObject goPlayBtn = new GameObject("Btn_PlayPause");
            goPlayBtn.transform.SetParent(goControls.transform, false);
            LayoutElement lePlay = goPlayBtn.AddComponent<LayoutElement>();
            lePlay.flexibleWidth = 1;
            lePlay.flexibleHeight = 1;

            Image imgPlay = goPlayBtn.AddComponent<Image>();
            imgPlay.color = new Color(0.05f, 0.65f, 0.40f, 1f);

            Button btnPlay = goPlayBtn.AddComponent<Button>();
            ColorBlock cbPlay = btnPlay.colors;
            cbPlay.normalColor = new Color(0.05f, 0.65f, 0.40f, 1f);
            cbPlay.highlightedColor = new Color(0.10f, 0.80f, 0.50f, 1f);
            cbPlay.pressedColor = new Color(0.15f, 0.95f, 0.60f, 1f);
            btnPlay.colors = cbPlay;

            GameObject goPlayTxt = new GameObject("Text");
            goPlayTxt.transform.SetParent(goPlayBtn.transform, false);
            RectTransform rtPlayTxt = goPlayTxt.AddComponent<RectTransform>();
            rtPlayTxt.anchorMin = Vector2.zero;
            rtPlayTxt.anchorMax = Vector2.one;
            rtPlayTxt.offsetMin = Vector2.zero;
            rtPlayTxt.offsetMax = Vector2.zero;

            Text txtPlay = goPlayTxt.AddComponent<Text>();
            txtPlay.text = "▶";
            txtPlay.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtPlay.fontSize = 32;
            txtPlay.fontStyle = FontStyle.Bold;
            txtPlay.color = Color.white;
            txtPlay.alignment = TextAnchor.MiddleCenter;
            sc.playerPlayPauseTexts[i] = txtPlay;

            ButtonProxy proxyPlay = goPlayBtn.AddComponent<ButtonProxy>();
            proxyPlay.controller = sc;
            proxyPlay.slotIndex = i;
            proxyPlay.action = ButtonProxy.ActionType.TogglePlayPause;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.onClick, proxyPlay.OnClick);

            // Stop Button (Large Touch Area)
            GameObject goStopBtn = new GameObject("Btn_Stop");
            goStopBtn.transform.SetParent(goControls.transform, false);
            LayoutElement leStop = goStopBtn.AddComponent<LayoutElement>();
            leStop.flexibleWidth = 1;
            leStop.flexibleHeight = 1;

            Image imgStop = goStopBtn.AddComponent<Image>();
            imgStop.color = new Color(0.85f, 0.18f, 0.22f, 1f);

            Button btnStop = goStopBtn.AddComponent<Button>();
            ColorBlock cbStop = btnStop.colors;
            cbStop.normalColor = new Color(0.85f, 0.18f, 0.22f, 1f);
            cbStop.highlightedColor = new Color(0.95f, 0.25f, 0.30f, 1f);
            cbStop.pressedColor = new Color(1f, 0.40f, 0.45f, 1f);
            btnStop.colors = cbStop;

            GameObject goStopTxt = new GameObject("Text");
            goStopTxt.transform.SetParent(goStopBtn.transform, false);
            RectTransform rtStopTxt = goStopTxt.AddComponent<RectTransform>();
            rtStopTxt.anchorMin = Vector2.zero;
            rtStopTxt.anchorMax = Vector2.one;
            rtStopTxt.offsetMin = Vector2.zero;
            rtStopTxt.offsetMax = Vector2.zero;

            Text txtStop = goStopTxt.AddComponent<Text>();
            txtStop.text = "⏹";
            txtStop.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtStop.fontSize = 28;
            txtStop.fontStyle = FontStyle.Bold;
            txtStop.color = Color.white;
            txtStop.alignment = TextAnchor.MiddleCenter;

            ButtonProxy proxyStop = goStopBtn.AddComponent<ButtonProxy>();
            proxyStop.controller = sc;
            proxyStop.slotIndex = i;
            proxyStop.action = ButtonProxy.ActionType.StopVideo;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStop.onClick, proxyStop.OnClick);

            // -------- 5 VIDEO BUTTONS WITH DYNAMIC PROGRESSIVE FILL --------
            sc.playerRows[i] = new SimpleController.RowElements();

            for (int v = 0; v < videoTitles.Length; v++)
            {
                GameObject goBtnFrame = new GameObject("BtnFrame_" + v);
                goBtnFrame.transform.SetParent(goRow.transform, false);
                LayoutElement leBtn = goBtnFrame.AddComponent<LayoutElement>();
                leBtn.flexibleWidth = 1;
                leBtn.flexibleHeight = 1;

                Image imgFrame = goBtnFrame.AddComponent<Image>();
                imgFrame.color = new Color(0.20f, 0.38f, 0.65f, 0.8f);
                sc.playerRows[i].btnFrames[v] = imgFrame;

                // Button Body
                GameObject goBtn = new GameObject("Btn");
                goBtn.transform.SetParent(goBtnFrame.transform, false);
                RectTransform rtBtn = goBtn.AddComponent<RectTransform>();
                rtBtn.anchorMin = Vector2.zero;
                rtBtn.anchorMax = Vector2.one;
                rtBtn.offsetMin = new Vector2(3, 3);
                rtBtn.offsetMax = new Vector2(-3, -3);

                Image btnBg = goBtn.AddComponent<Image>();
                btnBg.color = new Color(0.11f, 0.18f, 0.32f, 1f);
                sc.playerRows[i].btnBackgrounds[v] = btnBg;

                // Progressive Fill Image
                GameObject goFill = new GameObject("ProgressFill");
                goFill.transform.SetParent(goBtn.transform, false);
                RectTransform rtFill = goFill.AddComponent<RectTransform>();
                rtFill.anchorMin = Vector2.zero;
                rtFill.anchorMax = Vector2.one;
                rtFill.offsetMin = Vector2.zero;
                rtFill.offsetMax = Vector2.zero;

                Image imgFill = goFill.AddComponent<Image>();
                imgFill.sprite = whiteSprite;
                imgFill.type = Image.Type.Filled;
                imgFill.fillMethod = Image.FillMethod.Horizontal;
                imgFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                imgFill.fillAmount = 0f;
                imgFill.color = new Color(0.06f, 0.82f, 0.95f, 0.75f);
                sc.playerRows[i].btnFills[v] = imgFill;

                Button btn = goBtn.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
                cb.pressedColor = new Color(0.05f, 0.90f, 0.55f, 1f);
                cb.selectedColor = Color.white;
                cb.colorMultiplier = 1f;
                cb.fadeDuration = 0.08f;
                btn.colors = cb;
                sc.playerRows[i].buttons[v] = btn;

                // Button Text (On Top of Fill)
                GameObject goBtnTxt = new GameObject("Text");
                goBtnTxt.transform.SetParent(goBtn.transform, false);
                RectTransform rtBtnTxt = goBtnTxt.AddComponent<RectTransform>();
                rtBtnTxt.anchorMin = Vector2.zero;
                rtBtnTxt.anchorMax = Vector2.one;
                rtBtnTxt.offsetMin = new Vector2(8, 8);
                rtBtnTxt.offsetMax = new Vector2(-8, -8);

                Text txtBtn = goBtnTxt.AddComponent<Text>();
                txtBtn.text = videoTitles[v];
                txtBtn.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtBtn.fontSize = 28;
                txtBtn.fontStyle = FontStyle.Bold;
                txtBtn.color = Color.white;
                txtBtn.alignment = TextAnchor.MiddleCenter;
                txtBtn.lineSpacing = 1.15f;
                sc.playerRows[i].btnTexts[v] = txtBtn;

                int slotIndex = i;
                string targetUrl = videoUrls[v];
                
                ButtonProxy proxy = goBtn.AddComponent<ButtonProxy>();
                proxy.controller = sc;
                proxy.slotIndex = slotIndex;
                proxy.videoUrl = targetUrl;
                proxy.action = ButtonProxy.ActionType.PlayVideo;
                
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, proxy.OnClick);
            }
        }

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/SimpleTablet.unity");
    }
}
