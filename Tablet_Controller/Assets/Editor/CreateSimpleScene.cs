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
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        
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
        rtPanel.offsetMin = new Vector2(30, 20);
        rtPanel.offsetMax = new Vector2(-30, -20);
        VerticalLayoutGroup vlg = goPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 15;
        
        // Header
        GameObject goStatus = new GameObject("StatusHeader");
        goStatus.transform.SetParent(goPanel.transform, false);
        LayoutElement leHeader = goStatus.AddComponent<LayoutElement>();
        leHeader.minHeight = 60;
        leHeader.preferredHeight = 60;
        leHeader.flexibleHeight = 0;
        
        Text txtStatus = goStatus.AddComponent<Text>();
        txtStatus.text = "Rede: Conectando... | Sala: RiR-23 (sa)";
        txtStatus.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtStatus.fontSize = 32;
        txtStatus.color = new Color(1f, 0.9f, 0.2f, 1f);
        txtStatus.alignment = TextAnchor.MiddleCenter;
        sc.statusHeader = txtStatus;
        
        string[] videos = { "Amazonia", "Lencois Maranheses", "Fernando de Noronha", "Pantanal", "Rio de Janeiro" };

        for (int i = 0; i < 4; i++)
        {
            GameObject goRow = new GameObject("Row_Player_" + (i + 1));
            goRow.transform.SetParent(goPanel.transform, false);
            HorizontalLayoutGroup hlg = goRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 12;

            // Player Badge & Status Text Container
            GameObject goPlayerBadge = new GameObject("Badge_Player_" + (i + 1));
            goPlayerBadge.transform.SetParent(goRow.transform, false);
            LayoutElement leBadge = goPlayerBadge.AddComponent<LayoutElement>();
            leBadge.minWidth = 320;
            leBadge.preferredWidth = 320;
            leBadge.flexibleWidth = 0;

            Image imgBadge = goPlayerBadge.AddComponent<Image>();
            imgBadge.color = new Color(0.18f, 0.2f, 0.25f, 1f);
            sc.playerStatusBadges[i] = imgBadge;

            GameObject goText = new GameObject("Text");
            goText.transform.SetParent(goPlayerBadge.transform, false);
            RectTransform rtText = goText.AddComponent<RectTransform>();
            rtText.anchorMin = Vector2.zero;
            rtText.anchorMax = Vector2.one;
            rtText.offsetMin = new Vector2(10, 5);
            rtText.offsetMax = new Vector2(-10, -5);

            Text txt = goText.AddComponent<Text>();
            txt.text = $"Player {i + 1} <color=#FF4444>○ DESCONECTADO</color>";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 26;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            sc.playerStatusTexts[i] = txt;

            // Video Buttons
            foreach (string v in videos)
            {
                GameObject goBtn = new GameObject("Btn_" + v);
                goBtn.transform.SetParent(goRow.transform, false);
                Image btnImg = goBtn.AddComponent<Image>();
                btnImg.color = new Color(0.22f, 0.35f, 0.55f, 1f);
                
                Button btn = goBtn.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = new Color(0.22f, 0.35f, 0.55f, 1f);
                cb.highlightedColor = new Color(0.35f, 0.5f, 0.75f, 1f);
                cb.pressedColor = new Color(0.15f, 0.7f, 0.4f, 1f);
                btn.colors = cb;

                GameObject goBtnTxt = new GameObject("Text");
                goBtnTxt.transform.SetParent(goBtn.transform, false);
                Text btxt = goBtnTxt.AddComponent<Text>();
                btxt.text = v.Replace(" ", "\n");
                btxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                btxt.fontSize = 24;
                btxt.color = Color.white;
                btxt.alignment = TextAnchor.MiddleCenter;
                RectTransform btxtRt = btxt.GetComponent<RectTransform>();
                btxtRt.anchorMin = Vector2.zero;
                btxtRt.anchorMax = Vector2.one;
                btxtRt.offsetMin = Vector2.zero;
                btxtRt.offsetMax = Vector2.zero;

                int slotIndex = i;
                string videoUrl = "Videos/" + v + ".mp4";
                
                ButtonProxy proxy = goBtn.AddComponent<ButtonProxy>();
                proxy.controller = sc;
                proxy.slotIndex = slotIndex;
                proxy.videoUrl = videoUrl;
                
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, proxy.OnClick);
            }
        }

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/SimpleTablet.unity");
    }
}
