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
        
        // Camera
        GameObject goCamera = new GameObject("Main Camera");
        Camera cam = goCamera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.06f, 0.10f, 1f); // #0a0f1a Deep Midnight
        
        // EventSystem
        GameObject goEvent = new GameObject("EventSystem");
        goEvent.AddComponent<UnityEngine.EventSystems.EventSystem>();
        goEvent.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Canvas & Responsive Scaler for Tab S6 Lite (2000x1200 / 1920x1080)
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
        rtRoot.offsetMin = new Vector2(30, 25);
        rtRoot.offsetMax = new Vector2(-30, -25);

        VerticalLayoutGroup vlgRoot = goRoot.AddComponent<VerticalLayoutGroup>();
        vlgRoot.childControlWidth = true;
        vlgRoot.childControlHeight = true;
        vlgRoot.childForceExpandWidth = true;
        vlgRoot.childForceExpandHeight = true;
        vlgRoot.spacing = 16;

        // ================= HEADER =================
        GameObject goHeader = new GameObject("Header");
        goHeader.transform.SetParent(goRoot.transform, false);
        LayoutElement leHeader = goHeader.AddComponent<LayoutElement>();
        leHeader.minHeight = 100;
        leHeader.preferredHeight = 100;
        leHeader.flexibleHeight = 0;
        leHeader.flexibleWidth = 1;

        Image imgHeader = goHeader.AddComponent<Image>();
        imgHeader.color = new Color(0.08f, 0.12f, 0.20f, 0.95f); // #141f33 Card

        GameObject goHeaderTxt = new GameObject("TitleText");
        goHeaderTxt.transform.SetParent(goHeader.transform, false);
        RectTransform rtHeaderTxt = goHeaderTxt.AddComponent<RectTransform>();
        rtHeaderTxt.anchorMin = Vector2.zero;
        rtHeaderTxt.anchorMax = Vector2.one;
        rtHeaderTxt.offsetMin = new Vector2(20, 5);
        rtHeaderTxt.offsetMax = new Vector2(-20, -5);

        Text txtHeader = goHeaderTxt.AddComponent<Text>();
        txtHeader.text = "<b><size=46><color=#60A5FA>BRASIL</color><color=#FCD34D>VR</color></size></b>\n<size=20><color=#94A3B8>PAINEL DE CONTROLE MULTI-VR</color></size>";
        txtHeader.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtHeader.fontSize = 32;
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
            hlg.spacing = 14;

            // -------- BADGE (Slot 1, 2, 3, 4) --------
            GameObject goBadgeOuter = new GameObject("Badge_" + (i + 1));
            goBadgeOuter.transform.SetParent(goRow.transform, false);
            LayoutElement leBadge = goBadgeOuter.AddComponent<LayoutElement>();
            leBadge.minWidth = 260;
            leBadge.preferredWidth = 260;
            leBadge.flexibleWidth = 0;
            leBadge.flexibleHeight = 1;

            Image imgBadge = goBadgeOuter.AddComponent<Image>();
            imgBadge.color = new Color(0.09f, 0.13f, 0.22f, 1f); // #172138
            sc.playerStatusBadges[i] = imgBadge;

            // Text inside Badge
            GameObject goBadgeTxt = new GameObject("Text");
            goBadgeTxt.transform.SetParent(goBadgeOuter.transform, false);
            RectTransform rtBadgeTxt = goBadgeTxt.AddComponent<RectTransform>();
            rtBadgeTxt.anchorMin = Vector2.zero;
            rtBadgeTxt.anchorMax = Vector2.one;
            rtBadgeTxt.offsetMin = new Vector2(15, 10);
            rtBadgeTxt.offsetMax = new Vector2(-15, -10);

            Text txtBadge = goBadgeTxt.AddComponent<Text>();
            txtBadge.text = $"<b><size=54>{i + 1}</size></b>\n<size=20><color=#FF4D4D>○ OFFLINE</color></size>";
            txtBadge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txtBadge.fontSize = 24;
            txtBadge.color = Color.white;
            txtBadge.alignment = TextAnchor.MiddleCenter;
            txtBadge.supportRichText = true;
            txtBadge.lineSpacing = 1.1f;
            sc.playerStatusTexts[i] = txtBadge;

            // -------- 5 EQUAL PROPORTIONAL BUTTONS --------
            for (int v = 0; v < videoTitles.Length; v++)
            {
                // Outer Border Frame
                GameObject goBtnFrame = new GameObject("BtnFrame_" + v);
                goBtnFrame.transform.SetParent(goRow.transform, false);
                LayoutElement leBtn = goBtnFrame.AddComponent<LayoutElement>();
                leBtn.flexibleWidth = 1;
                leBtn.flexibleHeight = 1;

                Image imgFrame = goBtnFrame.AddComponent<Image>();
                imgFrame.color = new Color(0.20f, 0.32f, 0.50f, 0.8f); // Glowing border line

                // Inner Button
                GameObject goBtn = new GameObject("Btn");
                goBtn.transform.SetParent(goBtnFrame.transform, false);
                RectTransform rtBtn = goBtn.AddComponent<RectTransform>();
                rtBtn.anchorMin = Vector2.zero;
                rtBtn.anchorMax = Vector2.one;
                rtBtn.offsetMin = new Vector2(3, 3); // 3px border
                rtBtn.offsetMax = new Vector2(-3, -3);

                Image btnBg = goBtn.AddComponent<Image>();
                btnBg.color = new Color(0.10f, 0.16f, 0.28f, 1f); // Deep rich navy card

                Button btn = goBtn.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = new Color(0.10f, 0.16f, 0.28f, 1f);
                cb.highlightedColor = new Color(0.18f, 0.28f, 0.46f, 1f);
                cb.pressedColor = new Color(0.05f, 0.70f, 0.45f, 1f); // Emerald green on press
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
                rtBtnTxt.offsetMin = new Vector2(10, 8);
                rtBtnTxt.offsetMax = new Vector2(-10, -8);

                Text txtBtn = goBtnTxt.AddComponent<Text>();
                txtBtn.text = videoTitles[v];
                txtBtn.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txtBtn.fontSize = 30;
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
                
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, proxy.OnClick);
            }
        }

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/SimpleTablet.unity");
    }
}
