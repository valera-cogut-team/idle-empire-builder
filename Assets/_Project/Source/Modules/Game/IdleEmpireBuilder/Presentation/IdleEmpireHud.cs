using IdleEmpireBuilder.Facade;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace IdleEmpireBuilder.Presentation
{
    /// <summary>Reactive HUD: gold and passive income; per-building info is shown in-world above blocks.</summary>
    public sealed class IdleEmpireHud : MonoBehaviour
    {
        private IIdleEmpireFacade _facade;
        private CompositeDisposable _bindings;
        private bool _uiBuilt;

        private TMP_Text _mainLine;
        private TMP_Text _detailLine;
        private TMP_Text _hintLine;

        public void Initialize(IIdleEmpireFacade facade)
        {
            if (!_uiBuilt)
            {
                BuildUi();
                _uiBuilt = true;
            }

            _bindings?.Dispose();
            _bindings = new CompositeDisposable();
            _facade = facade;
            if (_facade == null)
                return;

            _facade.GoldRx.CombineLatest(
                _facade.IncomeRx,
                (gold, income) => (gold, income)
            ).Subscribe(t => Refresh(t.gold, t.income)).AddTo(_bindings);
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
            _bindings = null;
            _facade = null;
        }

        private void Refresh(long gold, float income)
        {
            if (_mainLine != null)
            {
                _mainLine.text =
                    $"<b>IDLE EMPIRE</b>   <color=#FFC94A>{gold}</color> gold   " +
                    $"<color=#6ED9BE>+{income:0.##}/s</color>";
            }

            if (_detailLine != null)
            {
                _detailLine.text =
                    "<size=92%>Mine · Mill · Market — each block shows <b>Lv</b> and upgrade price above it.</size>";
            }

            if (_hintLine != null)
            {
                _hintLine.text =
                    "<size=90%>Tap empty ground — bonus gold · Tap a colored block — try upgrade (glow = feedback)</size>";
            }
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("IdleEmpireHud_Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.layer = 5;

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2600;
            canvas.overrideSorting = true;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.55f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var safeGo = new GameObject("SafeArea", typeof(RectTransform));
            safeGo.transform.SetParent(canvasGo.transform, false);
            safeGo.layer = 5;
            var safeRt = safeGo.GetComponent<RectTransform>();
            StretchFullCanvas(safeRt);
            ApplyScreenSafeArea(safeRt);

            var panelGo = new GameObject("Panel", typeof(RectTransform));
            panelGo.transform.SetParent(safeGo.transform, false);
            panelGo.layer = 5;

            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(28f, -24f);
            panelRt.sizeDelta = new Vector2(1100f, 0f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.1f, 0.16f, 0.86f);
            bg.raycastTarget = false;

            var vlg = panelGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(22, 22, 18, 18);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperLeft;

            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _mainLine = CreateTmpLine(panelGo.transform, "MainLine", 30f, FontStyles.Bold, new Color(0.96f, 0.97f, 1f));
            _detailLine = CreateTmpLine(panelGo.transform, "DetailLine", 20f, FontStyles.Normal, new Color(0.82f, 0.88f, 0.96f));
            _hintLine = CreateTmpLine(panelGo.transform, "HintLine", 18f, FontStyles.Normal, new Color(0.7f, 0.76f, 0.86f));
        }

        private static void StretchFullCanvas(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void ApplyScreenSafeArea(RectTransform rt)
        {
            var sa = Screen.safeArea;
            if (sa.width <= 1f || sa.height <= 1f)
                return;

            var anchorMin = sa.position;
            var anchorMax = sa.position + sa.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateTmpLine(Transform parent, string name, float fontSize, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = 5;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = font.material;
            }

            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.richText = true;
            tmp.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, fontSize + 12f);

            return tmp;
        }
    }
}
