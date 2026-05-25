using NursingSim.Core.Events;
using NursingSim.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NursingSim.EditorTools
{
    internal static class UiBuilder
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/UI";
        private const string TogglePrefabPath = PrefabDir + "/ToggleRow.prefab";

        public static GameObject EnsureToggleRowPrefab(TMP_FontAsset font)
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir)) {
                System.IO.Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(TogglePrefabPath);
            if (existing != null) {
                var label = existing.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null && label.font == font) return existing;
                AssetDatabase.DeleteAsset(TogglePrefabPath);
            }

            var root = new GameObject("ToggleRow", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 36);

            var layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(4, 4, 2, 2);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            var toggleGo = new GameObject("Toggle", typeof(RectTransform));
            toggleGo.transform.SetParent(root.transform, false);
            var toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(28, 28);
            var toggle = toggleGo.AddComponent<Toggle>();
            var bgGo = new GameObject("Background", typeof(RectTransform));
            bgGo.transform.SetParent(toggleGo.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            StretchToParent(bgGo.GetComponent<RectTransform>());
            var checkGo = new GameObject("Checkmark", typeof(RectTransform));
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.color = new Color(0.3f, 0.85f, 0.4f, 1f);
            var checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkRect.sizeDelta = Vector2.zero;
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(360, 32);
            var text = labelGo.AddComponent<TextMeshProUGUI>();
            text.text = "항목";
            text.font = font;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            var le = labelGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minHeight = 32f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, TogglePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static GameObject CreateHudBar(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("HUD_Top", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 60f);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.padding = new RectOffset(24, 24, 8, 8);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;
            AddText(go.transform, "StepLabel", "Step -/-", font, 26, new Vector2(800, 44));
            AddText(go.transform, "TimerLabel", "00:00", font, 24, new Vector2(160, 44));
            AddText(go.transform, "ScoreLabel", "점수: 0", font, 24, new Vector2(200, 44));
            return go;
        }

        public static void BindHud(HudBinder hud, FeedbackBus bus, GameObject hudGo)
        {
            var so = new SerializedObject(hud);
            so.FindProperty("bus").objectReferenceValue = bus;
            so.FindProperty("stepLabel").objectReferenceValue = FindText(hudGo, "StepLabel");
            so.FindProperty("scoreLabel").objectReferenceValue = FindText(hudGo, "ScoreLabel");
            so.FindProperty("timerLabel").objectReferenceValue = FindText(hudGo, "TimerLabel");
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateChecklistPanel(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var go = new GameObject("Panel_Checklist", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.08f); rect.anchorMax = new Vector2(0f, 0.92f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(20f, 0f);
            rect.sizeDelta = new Vector2(460f, 0f);
            go.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.85f);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddText(go.transform, "Title", "단계", bold, 28, new Vector2(0, 40));
            AddText(go.transform, "Instruction", "지시문이 여기에 표시됩니다.", font, 20, new Vector2(0, 80));

            var pourBlock = new GameObject("PourBlock", typeof(RectTransform), typeof(VerticalLayoutGroup));
            pourBlock.transform.SetParent(go.transform, false);
            var pourLayout = pourBlock.GetComponent<VerticalLayoutGroup>();
            pourLayout.spacing = 6f;
            pourLayout.childControlWidth = true;
            pourLayout.childControlHeight = true;
            pourLayout.childForceExpandWidth = true;
            AddText(pourBlock.transform, "PourStatus", "펌프 0/0  비비기 0/0초", font, 22, new Vector2(0, 34));
            var sliderGo = CreateSlider(pourBlock.transform, "PourProgress");
            pourBlock.SetActive(false);

            var scroll = new GameObject("ItemsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(go.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            var scrollRect = scroll.GetComponent<ScrollRect>();
            var scrollRt = scroll.GetComponent<RectTransform>();
            scrollRt.sizeDelta = new Vector2(0, 480);
            var scrollLe = scroll.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 200f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            StretchToParent(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0, 0);
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.horizontal = false; scrollRect.vertical = true;

            var submit = CreateButton(go.transform, "SubmitButton", "완료", bold);

            return go;
        }

        public static void BindChecklistPanel(ChecklistPanelBinder binder, GameObject panelGo, GameObject togglePrefab)
        {
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = panelGo;
            so.FindProperty("titleLabel").objectReferenceValue = FindText(panelGo, "Title");
            so.FindProperty("instructionLabel").objectReferenceValue = FindText(panelGo, "Instruction");
            var content = panelGo.transform.Find("ItemsScroll/Viewport/Content");
            so.FindProperty("itemsContainer").objectReferenceValue = content;
            so.FindProperty("togglePrefab").objectReferenceValue = togglePrefab;
            so.FindProperty("submitButton").objectReferenceValue = panelGo.transform.Find("SubmitButton").GetComponent<Button>();
            var pour = panelGo.transform.Find("PourBlock");
            so.FindProperty("pourBlock").objectReferenceValue = pour.gameObject;
            so.FindProperty("pourStatusLabel").objectReferenceValue = FindText(pour.gameObject, "PourStatus");
            so.FindProperty("pourProgress").objectReferenceValue = pour.Find("PourProgress").GetComponent<Slider>();
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateItemPopup(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("Popup_ItemSelection", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(720, 640);
            frame.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 16);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "Title", "물품 선택", bold, 28, new Vector2(0, 42));
            AddText(frame.transform, "Instruction", "필요한 물품을 선택하세요.", font, 20, new Vector2(0, 50));

            var scroll = new GameObject("ItemsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(frame.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            var scrollRect = scroll.GetComponent<ScrollRect>();
            var scrollLe = scroll.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 320f;
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            StretchToParent(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.horizontal = false; scrollRect.vertical = true;

            var buttonRow = new GameObject("ButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(frame.transform, false);
            var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            CreateButton(buttonRow.transform, "CancelButton", "취소", bold);
            CreateButton(buttonRow.transform, "SubmitButton", "트레이에 담기", bold);

            dim.SetActive(false);
            return dim;
        }

        public static void BindItemPopup(ItemSelectionPopupBinder binder, GameObject popupGo, GameObject togglePrefab)
        {
            var frame = popupGo.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = popupGo;
            so.FindProperty("titleLabel").objectReferenceValue = FindText(frame.gameObject, "Title");
            so.FindProperty("instructionLabel").objectReferenceValue = FindText(frame.gameObject, "Instruction");
            so.FindProperty("itemsContainer").objectReferenceValue = frame.Find("ItemsScroll/Viewport/Content");
            so.FindProperty("togglePrefab").objectReferenceValue = togglePrefab;
            so.FindProperty("submitButton").objectReferenceValue = frame.Find("ButtonRow/SubmitButton").GetComponent<Button>();
            so.FindProperty("cancelButton").objectReferenceValue = frame.Find("ButtonRow/CancelButton").GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateToast(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f); rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(900f, 60f);
            var toastImg = go.GetComponent<Image>();
            toastImg.color = new Color(0.2f, 0.35f, 0.55f, 0.9f);
            toastImg.raycastTarget = false;
            var canvasGroup = go.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            var msg = AddText(go.transform, "Message", "메시지", font, 24, new Vector2(880, 52), TextAlignmentOptions.Center);
            msg.raycastTarget = false;
            return go;
        }

        public static void BindToast(ToastBinder binder, FeedbackBus bus, GameObject toastGo)
        {
            var so = new SerializedObject(binder);
            so.FindProperty("bus").objectReferenceValue = bus;
            so.FindProperty("group").objectReferenceValue = toastGo.GetComponent<CanvasGroup>();
            so.FindProperty("messageLabel").objectReferenceValue = FindText(toastGo, "Message");
            so.FindProperty("background").objectReferenceValue = toastGo.GetComponent<Image>();
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateCompletionBanner(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("CompletionBanner", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(820, 560);
            frame.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.14f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 20, 20);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "TitleHeader", "결과 리포트", bold, 32, new Vector2(0, 48), TextAlignmentOptions.Center);
            AddText(frame.transform, "Summary", "총점 0/0", bold, 26, new Vector2(0, 44), TextAlignmentOptions.Center);
            var details = AddText(frame.transform, "Details", "", font, 20, new Vector2(0, 300), TextAlignmentOptions.TopLeft);
            var le = details.gameObject.AddComponent<LayoutElement>();
            le.flexibleHeight = 1f;
            le.minHeight = 200f;
            CreateButton(frame.transform, "RestartButton", "다시하기", bold);

            dim.SetActive(false);
            return dim;
        }

        public static void BindCompletion(CompletionBannerBinder binder, FeedbackBus bus, GameObject go)
        {
            var frame = go.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("bus").objectReferenceValue = bus;
            so.FindProperty("root").objectReferenceValue = go;
            so.FindProperty("summaryLabel").objectReferenceValue = FindText(frame.gameObject, "Summary");
            so.FindProperty("detailsLabel").objectReferenceValue = FindText(frame.gameObject, "Details");
            so.FindProperty("restartButton").objectReferenceValue = frame.Find("RestartButton").GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        private const string ChoiceButtonPrefabPath = PrefabDir + "/ChoiceButtonRow.prefab";

        public static GameObject EnsureChoiceButtonPrefab(TMP_FontAsset font)
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir)) {
                System.IO.Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ChoiceButtonPrefabPath);
            if (existing != null) return existing;
            var root = new GameObject("ChoiceButtonRow", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420, 60);
            root.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.55f, 1f);
            var le = root.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.flexibleWidth = 1f;
            var labelText = AddText(root.transform, "Label", "선택지", font, 22, new Vector2(0, 0), TextAlignmentOptions.MidlineLeft);
            labelText.margin = new Vector4(16, 0, 16, 0);
            StretchToParent(labelText.rectTransform);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ChoiceButtonPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static GameObject CreateChoicePanel(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("Panel_Choice", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(720, 600);
            frame.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 16);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "Title", "선택", bold, 28, new Vector2(0, 42));
            AddText(frame.transform, "Instruction", "지시문", font, 20, new Vector2(0, 60));
            AddText(frame.transform, "Progress", "", font, 18, new Vector2(0, 26));

            var scroll = new GameObject("ChoicesScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(frame.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            var scrollRect = scroll.GetComponent<ScrollRect>();
            var scrollLe = scroll.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 360f;
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            StretchToParent(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.horizontal = false; scrollRect.vertical = true;

            dim.SetActive(false);
            return dim;
        }

        public static void BindChoicePanel(ChoicePanelBinder binder, GameObject panelGo, GameObject choicePrefab)
        {
            var frame = panelGo.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = panelGo;
            so.FindProperty("titleLabel").objectReferenceValue = FindText(frame.gameObject, "Title");
            so.FindProperty("instructionLabel").objectReferenceValue = FindText(frame.gameObject, "Instruction");
            so.FindProperty("progressLabel").objectReferenceValue = FindText(frame.gameObject, "Progress");
            so.FindProperty("choicesContainer").objectReferenceValue = frame.Find("ChoicesScroll/Viewport/Content");
            so.FindProperty("choiceButtonPrefab").objectReferenceValue = choicePrefab;
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateSequenceMiniGame(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("Panel_Sequence", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(820, 640);
            frame.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "Title", "주사 시퀀스", bold, 30, new Vector2(0, 44));
            AddText(frame.transform, "Instruction", "지시문", font, 20, new Vector2(0, 56));
            AddText(frame.transform, "ActionLabel", "현재 작업", bold, 24, new Vector2(0, 36));
            AddText(frame.transform, "Progress", "1 / 5", font, 18, new Vector2(0, 28), TextAlignmentOptions.Center);

            AddText(frame.transform, "AngleReadout", "90°", bold, 28, new Vector2(0, 36), TextAlignmentOptions.Center);
            CreateSlider(frame.transform, "AngleSlider");
            CreateSlider(frame.transform, "HoldProgress");

            var performBtn = CreateButton(frame.transform, "PerformButton", "수행", bold);
            var perfLe = performBtn.gameObject.AddComponent<LayoutElement>();
            perfLe.minHeight = 64f;

            var branch = new GameObject("BranchPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            branch.transform.SetParent(frame.transform, false);
            branch.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 0.85f);
            var branchLayout = branch.GetComponent<VerticalLayoutGroup>();
            branchLayout.padding = new RectOffset(12, 12, 10, 10);
            branchLayout.spacing = 8f;
            branchLayout.childControlWidth = true;
            branchLayout.childControlHeight = true;
            branchLayout.childForceExpandWidth = true;
            AddText(branch.transform, "BranchPrompt", "혈액이 보입니다. 어떻게 하시겠습니까?", bold, 22, new Vector2(0, 40), TextAlignmentOptions.Center);
            var branchRow = new GameObject("BranchRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            branchRow.transform.SetParent(branch.transform, false);
            var rowLayout = branchRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            CreateButton(branchRow.transform, "BranchYes", "그대로 진행", bold);
            CreateButton(branchRow.transform, "BranchNo", "처음부터 다시", bold);
            branch.SetActive(false);

            dim.SetActive(false);
            return dim;
        }

        public static void BindSequenceMiniGame(SequenceMiniGameBinder binder, GameObject panelGo)
        {
            var frame = panelGo.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = panelGo;
            so.FindProperty("titleLabel").objectReferenceValue = FindText(frame.gameObject, "Title");
            so.FindProperty("instructionLabel").objectReferenceValue = FindText(frame.gameObject, "Instruction");
            so.FindProperty("actionLabel").objectReferenceValue = FindText(frame.gameObject, "ActionLabel");
            so.FindProperty("progressLabel").objectReferenceValue = FindText(frame.gameObject, "Progress");
            so.FindProperty("angleReadout").objectReferenceValue = FindText(frame.gameObject, "AngleReadout");
            so.FindProperty("angleSlider").objectReferenceValue = frame.Find("AngleSlider").GetComponent<Slider>();
            so.FindProperty("holdProgress").objectReferenceValue = frame.Find("HoldProgress").GetComponent<Slider>();
            so.FindProperty("performButton").objectReferenceValue = frame.Find("PerformButton").GetComponent<Button>();
            var branch = frame.Find("BranchPanel");
            so.FindProperty("branchPanel").objectReferenceValue = branch.gameObject;
            so.FindProperty("branchPrompt").objectReferenceValue = FindText(branch.gameObject, "BranchPrompt");
            so.FindProperty("branchYesButton").objectReferenceValue = branch.Find("BranchRow/BranchYes").GetComponent<Button>();
            so.FindProperty("branchNoButton").objectReferenceValue = branch.Find("BranchRow/BranchNo").GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        // ---- MainMenu (Phase 3) ----

        private const string HistoryRowPrefabPath = PrefabDir + "/HistoryRow.prefab";

        public static GameObject EnsureHistoryRowPrefab(TMP_FontAsset font, TMP_FontAsset bold)
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir)) {
                System.IO.Directory.CreateDirectory(PrefabDir);
                AssetDatabase.Refresh();
            }
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(HistoryRowPrefabPath);
            if (existing != null) AssetDatabase.DeleteAsset(HistoryRowPrefabPath);

            var root = new GameObject("HistoryRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 56);
            root.GetComponent<Image>().color = new Color(0.10f, 0.14f, 0.22f, 0.92f);
            var le = root.AddComponent<LayoutElement>();
            le.minHeight = 56f;
            le.flexibleWidth = 1f;
            var layout = root.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            var time = AddText(root.transform, "TimeLabel", "2026-01-01 00:00", font, 18, new Vector2(180, 40));
            var timeLe = time.gameObject.AddComponent<LayoutElement>();
            timeLe.preferredWidth = 180f;
            timeLe.minWidth = 180f;

            var scenario = AddText(root.transform, "ScenarioLabel", "SCN_IM_INJECTION_001", font, 18, new Vector2(0, 40));
            var scLe = scenario.gameObject.AddComponent<LayoutElement>();
            scLe.flexibleWidth = 1f;

            var badge = new GameObject("CriticalBadge", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            badge.transform.SetParent(root.transform, false);
            badge.GetComponent<Image>().color = new Color(0.75f, 0.18f, 0.18f, 1f);
            var badgeLe = badge.AddComponent<LayoutElement>();
            badgeLe.preferredWidth = 140f;
            badgeLe.minWidth = 140f;
            var badgeLayout = badge.GetComponent<HorizontalLayoutGroup>();
            badgeLayout.padding = new RectOffset(8, 8, 4, 4);
            badgeLayout.childAlignment = TextAnchor.MiddleCenter;
            badgeLayout.childForceExpandHeight = true;
            badgeLayout.childForceExpandWidth = true;
            AddText(badge.transform, "Label", "★ Critical 0", bold, 16, new Vector2(120, 32), TextAlignmentOptions.Center);
            badge.SetActive(false);

            var score = AddText(root.transform, "ScoreLabel", "0 / 100", bold, 22, new Vector2(120, 40), TextAlignmentOptions.MidlineRight);
            var scoreLe = score.gameObject.AddComponent<LayoutElement>();
            scoreLe.preferredWidth = 120f;
            scoreLe.minWidth = 120f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, HistoryRowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static GameObject CreateMainMenuLayout(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var go = new GameObject("MainMenuLayout", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            StretchToParent(go.GetComponent<RectTransform>());

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            StretchToParent(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.10f, 1f);

            var title = AddText(go.transform, "Title", "간호 시뮬레이션", bold, 64, new Vector2(1200, 100), TextAlignmentOptions.Center);
            var titleRt = title.rectTransform;
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -120f);

            var card = new GameObject("ScenarioCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(VerticalLayoutGroup));
            card.transform.SetParent(go.transform, false);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(720, 280);
            card.GetComponent<Image>().color = new Color(0.13f, 0.20f, 0.35f, 1f);
            var cardLayout = card.GetComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(32, 32, 28, 28);
            cardLayout.spacing = 12f;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            AddText(card.transform, "CardTitle", "근육주사 (IM Injection)", bold, 36, new Vector2(0, 56), TextAlignmentOptions.Center);
            AddText(card.transform, "CardSubtitle", "KABONE 핵심기본간호술 #3", font, 22, new Vector2(0, 36), TextAlignmentOptions.Center);
            AddText(card.transform, "CardMeta", "난이도 중 · 약 10~20분", font, 20, new Vector2(0, 32), TextAlignmentOptions.Center);
            AddText(card.transform, "CardHint", "▶ 클릭하여 시작", bold, 22, new Vector2(0, 36), TextAlignmentOptions.Center);

            var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttons.transform.SetParent(go.transform, false);
            var btnRt = buttons.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.anchoredPosition = new Vector2(0f, 120f);
            btnRt.sizeDelta = new Vector2(720, 64);
            var btnLayout = buttons.GetComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 16f;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;
            btnLayout.childForceExpandWidth = true;
            CreateButton(buttons.transform, "SettingsButton", "설정", bold);
            CreateButton(buttons.transform, "HistoryButton", "최근 기록", bold);
            CreateButton(buttons.transform, "QuitButton", "종료", bold);

            var version = AddText(go.transform, "Version", "v0.1.0", font, 16, new Vector2(160, 28), TextAlignmentOptions.BottomRight);
            var verRt = version.rectTransform;
            verRt.anchorMin = new Vector2(1f, 0f);
            verRt.anchorMax = new Vector2(1f, 0f);
            verRt.pivot = new Vector2(1f, 0f);
            verRt.anchoredPosition = new Vector2(-24f, 16f);
            version.color = new Color(1f, 1f, 1f, 0.6f);

            return go;
        }

        public static void BindMainMenu(MainMenuBinder binder, GameObject layoutGo, GameObject settingsModalGo, GameObject historyModalGo, GameObject loadingOverlayGo)
        {
            var so = new SerializedObject(binder);
            so.FindProperty("scenarioCardButton").objectReferenceValue = layoutGo.transform.Find("ScenarioCard").GetComponent<Button>();
            var btnRoot = layoutGo.transform.Find("Buttons");
            so.FindProperty("settingsButton").objectReferenceValue = btnRoot.Find("SettingsButton").GetComponent<Button>();
            so.FindProperty("historyButton").objectReferenceValue = btnRoot.Find("HistoryButton").GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = btnRoot.Find("QuitButton").GetComponent<Button>();
            so.FindProperty("settingsModal").objectReferenceValue = settingsModalGo != null ? settingsModalGo.GetComponent<SettingsModalBinder>() : null;
            so.FindProperty("historyModal").objectReferenceValue = historyModalGo != null ? historyModalGo.GetComponent<RecentHistoryModalBinder>() : null;
            so.FindProperty("loadingOverlay").objectReferenceValue = loadingOverlayGo != null ? loadingOverlayGo.GetComponent<LoadingOverlayBinder>() : null;
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateSettingsModal(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("Modal_Settings", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(640, 520);
            frame.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 24, 24);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "Title", "설정", bold, 30, new Vector2(0, 46), TextAlignmentOptions.Center);

            // Subtitles row
            var subRow = BuildSettingRow(frame.transform, "SubtitlesRow", "자막 표시", font);
            var subToggle = CreateInlineToggle(subRow.transform, "SubtitlesToggle");

            // Font scale row
            var fontRow = BuildSettingRow(frame.transform, "FontRow", "폰트 크기", font);
            var fontGroupGo = new GameObject("FontGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ToggleGroup));
            fontGroupGo.transform.SetParent(fontRow.transform, false);
            var fgLayout = fontGroupGo.GetComponent<HorizontalLayoutGroup>();
            fgLayout.spacing = 8f;
            fgLayout.childControlWidth = true;
            fgLayout.childControlHeight = true;
            fgLayout.childForceExpandWidth = true;
            var fgLe = fontGroupGo.AddComponent<LayoutElement>();
            fgLe.preferredWidth = 320f;
            var group = fontGroupGo.GetComponent<ToggleGroup>();
            var small = CreateLabelToggle(fontGroupGo.transform, "FontSmall", "소", font, group);
            var medium = CreateLabelToggle(fontGroupGo.transform, "FontMedium", "중", font, group);
            var large = CreateLabelToggle(fontGroupGo.transform, "FontLarge", "대", font, group);

            // Volume row
            var volRow = BuildSettingRow(frame.transform, "VolumeRow", "마스터 볼륨", font);
            var sliderGo = CreateSlider(volRow.transform, "VolumeSlider");
            var sliderLe = sliderGo.AddComponent<LayoutElement>();
            sliderLe.preferredWidth = 320f;
            sliderLe.minHeight = 24f;
            sliderGo.GetComponent<Slider>().value = 1f;

            CreateButton(frame.transform, "CloseButton", "닫기", bold);

            dim.SetActive(false);
            return dim;
        }

        public static void BindSettingsModal(SettingsModalBinder binder, GameObject modalGo)
        {
            var frame = modalGo.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = modalGo;
            so.FindProperty("subtitlesToggle").objectReferenceValue = frame.Find("SubtitlesRow/SubtitlesToggle").GetComponent<Toggle>();
            so.FindProperty("fontSmallToggle").objectReferenceValue = frame.Find("FontRow/FontGroup/FontSmall").GetComponent<Toggle>();
            so.FindProperty("fontMediumToggle").objectReferenceValue = frame.Find("FontRow/FontGroup/FontMedium").GetComponent<Toggle>();
            so.FindProperty("fontLargeToggle").objectReferenceValue = frame.Find("FontRow/FontGroup/FontLarge").GetComponent<Toggle>();
            so.FindProperty("volumeSlider").objectReferenceValue = frame.Find("VolumeRow/VolumeSlider").GetComponent<Slider>();
            so.FindProperty("closeButton").objectReferenceValue = frame.Find("CloseButton").GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateRecentHistoryModal(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            var dim = new GameObject("Modal_History", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(parent, false);
            StretchToParent(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            frame.transform.SetParent(dim.transform, false);
            var frameRt = frame.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f); frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(900, 680);
            frame.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.97f);
            var layout = frame.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 24, 24);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            AddText(frame.transform, "Title", "최근 기록", bold, 30, new Vector2(0, 46), TextAlignmentOptions.Center);

            var empty = AddText(frame.transform, "EmptyPlaceholder", "기록이 없습니다.", font, 22, new Vector2(0, 40), TextAlignmentOptions.Center);
            empty.color = new Color(1f, 1f, 1f, 0.6f);
            empty.gameObject.SetActive(true);

            var scroll = new GameObject("RowsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(frame.transform, false);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
            var scrollLe = scroll.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1f;
            scrollLe.minHeight = 440f;
            var scrollRect = scroll.GetComponent<ScrollRect>();

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scroll.transform, false);
            StretchToParent(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.padding = new RectOffset(6, 6, 6, 6);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.horizontal = false; scrollRect.vertical = true;

            CreateButton(frame.transform, "CloseButton", "닫기", bold);

            dim.SetActive(false);
            return dim;
        }

        public static void BindRecentHistoryModal(RecentHistoryModalBinder binder, GameObject modalGo, GameObject rowPrefab, NursingSim.Core.Runner.SaveService saveService)
        {
            var frame = modalGo.transform.Find("Frame");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = modalGo;
            so.FindProperty("saveService").objectReferenceValue = saveService;
            so.FindProperty("rowsParent").objectReferenceValue = frame.Find("RowsScroll/Viewport/Content");
            so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("emptyPlaceholder").objectReferenceValue = frame.Find("EmptyPlaceholder").gameObject;
            so.FindProperty("closeButton").objectReferenceValue = frame.Find("CloseButton").GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        public static GameObject CreateLoadingOverlay(Transform parent, TMP_FontAsset font, TMP_FontAsset bold)
        {
            // 호스트는 항상 active로 유지 (binder의 코루틴을 위해). Visual 자식만 toggle 대상.
            var host = new GameObject("Loading_Overlay", typeof(RectTransform));
            host.transform.SetParent(parent, false);
            StretchToParent(host.GetComponent<RectTransform>());

            var visual = new GameObject("Visual", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            visual.transform.SetParent(host.transform, false);
            StretchToParent(visual.GetComponent<RectTransform>());
            visual.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
            var cg = visual.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            cg.interactable = true;

            var center = new GameObject("Center", typeof(RectTransform), typeof(VerticalLayoutGroup));
            center.transform.SetParent(visual.transform, false);
            var centerRt = center.GetComponent<RectTransform>();
            centerRt.anchorMin = new Vector2(0.5f, 0.5f);
            centerRt.anchorMax = new Vector2(0.5f, 0.5f);
            centerRt.pivot = new Vector2(0.5f, 0.5f);
            centerRt.sizeDelta = new Vector2(400, 240);
            var centerLayout = center.GetComponent<VerticalLayoutGroup>();
            centerLayout.spacing = 16f;
            centerLayout.padding = new RectOffset(0, 0, 0, 0);
            centerLayout.childAlignment = TextAnchor.MiddleCenter;
            centerLayout.childControlWidth = true;
            centerLayout.childControlHeight = true;
            centerLayout.childForceExpandWidth = true;
            centerLayout.childForceExpandHeight = false;

            // Spinner (4-dot rotating ring, parent transform Z is rotated by binder).
            var spinner = new GameObject("Spinner", typeof(RectTransform));
            spinner.transform.SetParent(center.transform, false);
            var spinRt = spinner.GetComponent<RectTransform>();
            spinRt.sizeDelta = new Vector2(96, 96);
            var spinLe = spinner.AddComponent<LayoutElement>();
            spinLe.preferredWidth = 96f;
            spinLe.preferredHeight = 96f;
            spinLe.minWidth = 96f;
            spinLe.minHeight = 96f;
            AddSpinnerDot(spinner.transform, "Dot_N", new Vector2(0f, 36f), 1.00f);
            AddSpinnerDot(spinner.transform, "Dot_E", new Vector2(36f, 0f), 0.75f);
            AddSpinnerDot(spinner.transform, "Dot_S", new Vector2(0f, -36f), 0.50f);
            AddSpinnerDot(spinner.transform, "Dot_W", new Vector2(-36f, 0f), 0.25f);

            AddText(center.transform, "Message", "불러오는 중…", bold, 26, new Vector2(0, 38), TextAlignmentOptions.Center);

            var bar = CreateSlider(center.transform, "ProgressBar");
            var barLe = bar.AddComponent<LayoutElement>();
            barLe.preferredHeight = 14f;
            barLe.minHeight = 14f;
            var barSlider = bar.GetComponent<Slider>();
            barSlider.interactable = false;
            barSlider.value = 0f;

            AddText(center.transform, "ProgressLabel", "0%", font, 20, new Vector2(0, 28), TextAlignmentOptions.Center);

            visual.SetActive(false);
            return host;
        }

        public static void BindLoadingOverlay(LoadingOverlayBinder binder, GameObject overlayGo)
        {
            var visual = overlayGo.transform.Find("Visual");
            var center = visual.Find("Center");
            var so = new SerializedObject(binder);
            so.FindProperty("root").objectReferenceValue = visual.gameObject;
            so.FindProperty("spinner").objectReferenceValue = center.Find("Spinner").GetComponent<RectTransform>();
            so.FindProperty("messageLabel").objectReferenceValue = FindText(center.gameObject, "Message");
            so.FindProperty("progressBar").objectReferenceValue = center.Find("ProgressBar").GetComponent<Slider>();
            so.FindProperty("progressLabel").objectReferenceValue = FindText(center.gameObject, "ProgressLabel");
            so.ApplyModifiedProperties();
        }

        private static void AddSpinnerDot(Transform spinner, string name, Vector2 offset, float alpha)
        {
            var dot = new GameObject(name, typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(spinner, false);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(18, 18);
            var img = dot.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, alpha);
            img.raycastTarget = false;
        }

        private static GameObject BuildSettingRow(Transform parent, string name, string label, TMP_FontAsset font)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 48f;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            var lbl = AddText(row.transform, "Label", label, font, 22, new Vector2(240, 40));
            var lblLe = lbl.gameObject.AddComponent<LayoutElement>();
            lblLe.preferredWidth = 240f;
            lblLe.minWidth = 240f;
            return row;
        }

        private static Toggle CreateInlineToggle(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 40f;
            le.minWidth = 40f;
            le.preferredHeight = 40f;
            le.minHeight = 40f;
            var toggle = go.AddComponent<Toggle>();
            var bgGo = new GameObject("Background", typeof(RectTransform));
            bgGo.transform.SetParent(go.transform, false);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            StretchToParent(bgGo.GetComponent<RectTransform>());
            var checkGo = new GameObject("Checkmark", typeof(RectTransform));
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.color = new Color(0.3f, 0.85f, 0.4f, 1f);
            var checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkRect.sizeDelta = Vector2.zero;
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = true;
            return toggle;
        }

        private static Toggle CreateLabelToggle(Transform parent, string name, string label, TMP_FontAsset font, ToggleGroup group)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Toggle));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(96, 44);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.18f, 0.32f, 0.55f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 96f;
            le.minHeight = 44f;
            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = img;
            toggle.group = group;
            var checkGo = new GameObject("Checkmark", typeof(RectTransform));
            checkGo.transform.SetParent(go.transform, false);
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.color = new Color(0.30f, 0.85f, 0.40f, 1f);
            var checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0f, 0f);
            checkRect.anchorMax = new Vector2(1f, 0.12f);
            checkRect.sizeDelta = Vector2.zero;
            toggle.graphic = checkImg;
            toggle.isOn = false;
            var labelText = AddText(go.transform, "Label", label, font, 22, new Vector2(0, 0), TextAlignmentOptions.Center);
            StretchToParent(labelText.rectTransform);
            return toggle;
        }

        // ---- primitives ----

        private static TextMeshProUGUI AddText(Transform parent, string name, string content, TMP_FontAsset font, float size, Vector2 sizeDelta, TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = align;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 54);
            go.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.85f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 54f;
            AddText(go.transform, "Label", label, font, 24, new Vector2(0, 0), TextAlignmentOptions.Center)
                .rectTransform.anchorMin = Vector2.zero;
            var labelText = go.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            StretchToParent(labelText.rectTransform);
            return go.GetComponent<Button>();
        }

        private static GameObject CreateSlider(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 18);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            StretchToParent(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0); faRt.anchorMax = new Vector2(1, 1);
            faRt.offsetMin = new Vector2(4, 4); faRt.offsetMax = new Vector2(-4, -4);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            StretchToParent(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.4f, 1f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = bg.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
            return go;
        }

        private static TMP_Text FindText(GameObject root, string name)
        {
            var t = root.transform.Find(name);
            if (t == null) {
                foreach (var c in root.GetComponentsInChildren<TMP_Text>(true))
                    if (c.gameObject.name == name) return c;
                return null;
            }
            return t.GetComponent<TMP_Text>();
        }

        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
