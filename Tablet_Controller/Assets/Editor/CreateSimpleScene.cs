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
        
        // White sprite for filled progress bar
        Sprite whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

        // Camera
        GameObject goCamera = new GameObject("Main Camera");
        Camera cam = goCamera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.06f, 0.10f, 1f); // #0a0f1a Deep Midnight
        
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
        rtRoot.offsetMin = new Vector2(25, 20);
        rtRoot.offsetMax = new Vector2(-25, -20);

        VerticalLayoutGroup vlgRoot = goRoot.AddComponent<VerticalLayoutGroup>();
        vlgRoot.childControlWidth = true;
        vlgRoot.childControlHeight = true;
        vlgRoot.childForceExpandWidth = true;
        vlgRoot.childForceExpandHeight = true;
        vlgRoot.spacing = 14;

        // ================= HEADER =================
        GameObject goHeader = new GameObject("Header");
        goHeader.transform.SetParent(goRoot.transform, false);
        LayoutElement leHeader = goHeader.AddComponent<LayoutElement>();
        leHeader.minHeight = 85;
        leHeader.preferredHeight = 85;
        leHeader.flexibleHeight = 0;
        leHeader.flexibleWidth = 1;

        Image imgHeader = goHeader.AddComponent<Image>();
        imgHeader.color = new Color(0.08f, 0.12f, 0.20f, 0.95f);

        GameObject goHeaderTxt = new GameObject("TitleText");
        goHeaderTxt.transform.SetParent(goHeader.transform, false);
        RectTransform rtHeaderTxt = goHeaderTxt.AddComponent<RectTransform>();
        rtHeaderTxt.anchorMin = Vector2.zero;
        rtHeaderTxt.anchorMax = Vector2.one;
        rtHeaderTxt.offsetMin = new Vector2(20, 5);
        rtHeaderTxt.offsetMax = new Vector2(-20, -5);

        Text txtHeader = goHeaderTxt.AddComponent<Text>();
        txtHeader.text = "<b><size=44><color=#60A5FA>BRASIL</color><color=#FCD34D>VR</color></size></b>\n<size=18><color=#94A3B8>PAINEL DE CONTROLE MULTI-VR</color></size>";
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

            // -------- BADGE & CONTROLS CONTAINER (LEFT) --------
            GameObject goBadgeBox = new GameObject("BadgeBox_" + (i + 1));
            goBadgeBox.transform.SetParent(goRow.transform, false);
            LayoutElement leBadgeBox = goBadgeBox.AddComponent<LayoutElement>();
            leBadgeBox.minWidth = 330;
            leBadgeBox.preferredWidth = 330;
            leBadgeBox.flexibleWidth = 0;
            leBadgeBox.flexibleHeight = 1;

            Image imgBadge = goBadgeBox.AddComponent<Image>();
            imgBadge.color = new Color(0.09f, 0.13f, 0.22f, 1f);
            sc.playerStatusBadges[i] = imgBadge;

            HorizontalLayoutGroup hlgBadge = goBadgeBox.AddComponent<HorizontalLayoutGroup>();
            hlgBadge.childControlWidth = true;
            hlgBadge.childControlHeight = true;
            hlgBadge.childForceExpandWidth = false;
            hlgBadge.childForceExpandHeight = true;
            hlgBadge.spacing = 8;
            hlgBadge.padding = new RectOffset(14, 12, 10, 10);

            // Left-aligned Text (Number 1, 2, 3, 4 + Status)
            GameObject goBadgeTxt = new GameObject("StatusText");
            goBadgeTxt.transform.SetParent(goBadgeBox.transform, false);
            LayoutElement leTxt = goBadgeTxt.AddComponent<LayoutElement>();
            leTxt.flexibleWidth = 1;
            leTxt.flexibleHeight = 1;

            Text txtBadge = goBadgeTxt.AddComponent<Text>();
            txtBadge.text = $"<b><size=50>{i + 1}</size></b>   <color=#FF4D4D>○ OFF</color>";
            txtBadge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBadge.fontSize = 22;
            txtBadge.color = Color.white;
            txtBadge.alignment = TextAnchor.MiddleLeft; // Aligned to Left!
            txtBadge.supportRichText = true;
            txtBadge.lineSpacing = 1.15f;
            sc.playerStatusTexts[i] = txtBadge;

            // Play/Pause Button
            GameObject goPlayBtn = new GameObject("Btn_PlayPause");
            goPlayBtn.transform.SetParent(goBadgeBox.transform, false);
            LayoutElement lePlay = goPlayBtn.AddComponent<LayoutElement>();
            lePlay.minWidth = 56;
            lePlay.preferredWidth = 56;
            lePlay.flexibleWidth = 0;
            lePlay.flexibleHeight = 1;

            Image imgPlay = goPlayBtn.AddComponent<Image>();
            imgPlay.color = new Color(0.12f, 0.35f, 0.25f, 1f); // Deep emerald

            Button btnPlay = goPlayBtn.AddComponent<Button>();
            ColorBlock cbPlay = btnPlay.colors;
            cbPlay.normalColor = new Color(0.12f, 0.35f, 0.25f, 1f);
            cbPlay.highlightedColor = new Color(0.18f, 0.55f, 0.38f, 1f);
            cbPlay.pressedColor = new Color(0.25f, 0.80f, 0.50f, 1f);
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
            txtPlay.fontSize = 28;
            txtPlay.color = Color.white;
            txtPlay.alignment = TextAnchor.MiddleCenter;
            sc.playerPlayPauseTexts[i] = txtPlay;

            ButtonProxy proxyPlay = goPlayBtn.AddComponent<ButtonProxy>();
            proxyPlay.controller = sc;
            proxyPlay.slotIndex = i;
            proxyPlay.action = ButtonProxy.ActionType.TogglePlayPause;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPlay.onClick, proxyPlay.OnClick);

            // Stop Button
            GameObject goStopBtn = new GameObject("Btn_Stop");
            goStopBtn.transform.SetParent(goBadgeBox.transform, false);
            LayoutElement leStop = goStopBtn.AddComponent<LayoutElement>();
            leStop.minWidth = 56;
            leStop.preferredWidth = 56;
            leStop.flexibleWidth = 0;
            leStop.flexibleHeight = 1;

            Image imgStop = goStopBtn.AddComponent<Image>();
            imgStop.color = new Color(0.38f, 0.14f, 0.16f, 1f); // Deep Crimson

            Button btnStop = goStopBtn.AddComponent<Button>();
            ColorBlock cbStop = btnStop.colors;
            cbStop.normalColor = new Color(0.38f, 0.14f, 0.16f, 1f);
            cbStop.highlightedColor = new Color(0.55f, 0.20f, 0.22f, 1f);
            cbStop.pressedColor = new Color(0.85f, 0.25f, 0.28f, 1f);
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
            txtStop.fontSize = 24;
            txtStop.color = Color.white;
            txtStop.alignment = TextAnchor.MiddleCenter;

            ButtonProxy proxyStop = goStopBtn.AddComponent<ButtonProxy>();
            proxyStop.controller = sc;
            proxyStop.slotIndex = i;
            proxyStop.action = ButtonProxy.ActionType.StopVideo;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStop.onClick, proxyStop.OnClick);

            // -------- 5 VIDEO BUTTONS WITH PROGRESS FILL --------
            sc.playerButtonFills[i] = new SimpleController.RowFills();

            for (int v = 0; v < videoTitles.Length; v++)
            {
                // Outer Frame
                GameObject goBtnFrame = new GameObject("BtnFrame_" + v);
                goBtnFrame.transform.SetParent(goRow.transform, false);
                LayoutElement leBtn = goBtnFrame.AddComponent<LayoutElement>();
                leBtn.flexibleWidth = 1;
                leBtn.flexibleHeight = 1;

                Image imgFrame = goBtnFrame.AddComponent<Image>();
                imgFrame.color = new Color(0.20f, 0.32f, 0.50f, 0.8f);

                // Button
                GameObject goBtn = new GameObject("Btn");
                goBtn.transform.SetParent(goBtnFrame.transform, false);
                RectTransform rtBtn = goBtn.AddComponent<RectTransform>();
                rtBtn.anchorMin = Vector2.zero;
                rtBtn.anchorMax = Vector2.one;
                rtBtn.offsetMin = new Vector2(3, 3);
                rtBtn.offsetMax = new Vector2(-3, -3);

                Image btnBg = goBtn.AddComponent<Image>();
                btnBg.color = new Color(0.10f, 0.16f, 0.28f, 1f);

                // Progressive Fill Image (Inside Button)
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
                imgFill.color = new Color(0.12f, 0.55f, 0.95f, 0.45f);
                sc.playerButtonFills[i].fills[v] = imgFill;

                Button btn = goBtn.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = new Color(0.10f, 0.16f, 0.28f, 1f);
                cb.highlightedColor = new Color(0.18f, 0.28f, 0.46f, 1f);
                cb.pressedColor = new Color(0.05f, 0.70f, 0.45f, 1f);
                cb.selectedColor = new Color(0.14f, 0.22f, 0.36f, 1f);
                cb.colorMultiplier = 1f;
                cb.fadeDuration = 0.08f;
                btn.colors = cb;

                // Button Text
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
                txtBtn.color = new Color(0.95f, 0.98f, 1f, 1f);
                txtBtn.alignment = TextAnchor.MiddleCenter;
                txtBtn.lineSpacing = 1.15f;

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
