using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    public class HandControlsHelpOverlay : MonoBehaviour
    {
        private const string OverlayName = "Panel_HandControlsHelp";
        private const float RowHeight = 42f;
        private const int RowCount = 9;

        private GameObject overlayRoot;
        private TMP_FontAsset font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureOnSceneLoaded()
        {
            if (Object.FindFirstObjectByType<HandControlsHelpOverlay>() != null) return;
            new GameObject("HandControlsHelpOverlay").AddComponent<HandControlsHelpOverlay>();
        }

        private void Awake()
        {
            font = ResolveFontAsset();
            BuildOverlay();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        private void Toggle()
        {
            if (overlayRoot == null) BuildOverlay();
            bool show = !overlayRoot.activeSelf;
            overlayRoot.SetActive(show);
            if (show) overlayRoot.transform.SetAsLastSibling();
        }

        private void BuildOverlay()
        {
            var canvas = ResolveCanvas();
            var old = canvas.transform.Find(OverlayName);
            if (old != null) Destroy(old.gameObject);

            overlayRoot = new GameObject(OverlayName, typeof(RectTransform), typeof(Image));
            overlayRoot.transform.SetParent(canvas.transform, false);
            overlayRoot.transform.SetAsLastSibling();
            Stretch(overlayRoot.GetComponent<RectTransform>());
            overlayRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.58f);

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            card.transform.SetParent(overlayRoot.transform, false);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(900f, 620f);
            card.GetComponent<Image>().color = new Color(0.98f, 0.98f, 0.96f, 0.98f);

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(44, 44, 28, 28);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddText(card.transform, "Title", "게임", 30, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 38f));
            AddText(card.transform, "Subtitle", "조작법", 26, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 34f));
            AddText(card.transform, "Intro",
                "F1 키를 눌러 조작법을 열고 닫을 수 있습니다.\n손 시뮬레이션은 단순해 보이지만 실제 조작은 섬세하므로, 손 이동과 손가락 조작을 나눠서 사용합니다.",
                16, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 58f));

            var table = new GameObject("Table", typeof(RectTransform), typeof(VerticalLayoutGroup));
            table.transform.SetParent(card.transform, false);
            var tableLayout = table.GetComponent<VerticalLayoutGroup>();
            tableLayout.spacing = 1f;
            tableLayout.childControlWidth = true;
            tableLayout.childControlHeight = true;
            tableLayout.childForceExpandWidth = true;
            tableLayout.childForceExpandHeight = false;
            var tableElement = table.AddComponent<LayoutElement>();
            tableElement.preferredHeight = RowCount * RowHeight + (RowCount - 1);

            AddRow(table.transform, "A, S, D, F, Space", "현재 선택한 손을 움직입니다.");
            AddRow(table.transform, "Z, X, C, V, B", "엄지, 검지, 중지, 약지, 소지를 접고 펼 수 있습니다.");
            AddRow(table.transform, "Ctrl", "시점을 낮춰 앉은 자세로 봅니다.");
            AddRow(table.transform, "R", "일어서고 손 위치를 초기화합니다.");
            AddRow(table.transform, "Left Shift", "조작할 손을 바꿉니다.");
            AddRow(table.transform, "Right Mouse", "손목을 돌립니다.");
            AddRow(table.transform, "Left Mouse", "물건을 집거나 놓고, 펌프 가까이에서는 누릅니다.");
            AddRow(table.transform, "Mouse Wheel", "손 높이를 조절합니다.");
            AddRow(table.transform, "손위생 #1", "펌프 2회 후 수도꼭지 아래로 손을 옮기면 물이 나옵니다.");

            AddText(card.transform, "CloseHint", "F1: 닫기", 14, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 22f));
            overlayRoot.SetActive(false);
        }

        private void AddRow(Transform parent, string key, string description)
        {
            var row = new GameObject($"Row_{key}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.85f);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 1f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            var rowElement = row.AddComponent<LayoutElement>();
            rowElement.minHeight = RowHeight;
            rowElement.preferredHeight = RowHeight;

            AddCell(row.transform, $"Key_{key}", key, 0.33f, FontStyles.Bold);
            AddCell(row.transform, $"Description_{key}", description, 0.67f, FontStyles.Normal);
        }

        private void AddCell(Transform parent, string name, string text, float widthRatio, FontStyles style)
        {
            var cell = new GameObject(name, typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(parent, false);
            cell.GetComponent<Image>().color = new Color(0.96f, 0.96f, 0.94f, 1f);
            var element = cell.AddComponent<LayoutElement>();
            element.flexibleWidth = widthRatio;
            element.minHeight = RowHeight;
            element.preferredHeight = RowHeight;

            var label = AddText(cell.transform, "Label", text, 18, style, TextAlignmentOptions.Center, Vector2.zero);
            Stretch(label.rectTransform);
            label.margin = new Vector4(8f, 0f, 8f, 0f);
        }

        private TextMeshProUGUI AddText(Transform parent, string name, string text, float size, FontStyles style, TextAlignmentOptions alignment, Vector2 preferredSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.font = font;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;

            if (preferredSize != Vector2.zero)
            {
                go.GetComponent<RectTransform>().sizeDelta = preferredSize;
                var element = go.AddComponent<LayoutElement>();
                element.preferredHeight = preferredSize.y;
            }

            return label;
        }

        private static Canvas ResolveCanvas()
        {
            var hud = GameObject.Find("Canvas_HUD");
            var canvas = hud != null ? hud.GetComponent<Canvas>() : Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) return canvas;

            var go = new GameObject("Canvas_HandControls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return canvas;
        }

        private static TMP_FontAsset ResolveFontAsset()
        {
            foreach (var label in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                if (label.font != null) return label.font;
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
