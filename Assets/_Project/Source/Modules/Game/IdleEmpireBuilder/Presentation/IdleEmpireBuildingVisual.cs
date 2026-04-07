using DG.Tweening;
using IdleEmpireBuilder.Facade;
using TMPro;
using UniRx;
using UnityEngine;

namespace IdleEmpireBuilder.Presentation
{
    /// <summary>World labels (Lv + next cost), tap highlight, DOTween spend float on successful upgrade.</summary>
    public sealed class IdleEmpireBuildingVisual : MonoBehaviour
    {
        private const float LabelYOffset = 1.42f;
        private const float SpendFloatYOffset = 1.85f;

        private int _slotIndex;
        private IIdleEmpireFacade _facade;
        private Camera _camera;
        private TextMeshPro _labelTmp;
        private Transform _labelTransform;
        private Renderer _cubeRenderer;
        private Material _cubeMaterial;
        private Color _baseCubeColor;
        private Vector3 _baseScale;
        private CompositeDisposable _disp;
        private int _tweenId;

        public void Configure(int slotIndex, IIdleEmpireFacade facade, Camera worldCamera)
        {
            _slotIndex = slotIndex;
            _facade = facade;
            _camera = worldCamera;
            _tweenId = gameObject.GetInstanceID();
            _baseScale = transform.localScale;

            _cubeRenderer = GetComponent<Renderer>();
            if (_cubeRenderer != null)
            {
                _cubeMaterial = _cubeRenderer.material;
                _baseCubeColor = _cubeMaterial.color;
            }

            BuildWorldLabel();

            _disp?.Dispose();
            _disp = new CompositeDisposable();

            if (_facade == null)
                return;

            var levelRx = BuildingLevelRx(_facade, slotIndex);
            _facade.GoldRx.CombineLatest(levelRx, (_, __) => 0)
                .Subscribe(_ => RefreshLabel())
                .AddTo(_disp);

            _facade.BuildingSlotTappedPulse
                .Where(i => i == _slotIndex)
                .Subscribe(_ => PulseTapHighlight())
                .AddTo(_disp);

            _facade.UpgradeSpendPulse
                .Where(e => e.SlotIndex == _slotIndex)
                .Subscribe(OnUpgradeSpend)
                .AddTo(_disp);

            RefreshLabel();
        }

        private static IReadOnlyReactiveProperty<int> BuildingLevelRx(IIdleEmpireFacade facade, int slot) =>
            slot switch
            {
                0 => facade.Building0Rx,
                1 => facade.Building1Rx,
                2 => facade.Building2Rx,
                _ => facade.Building0Rx
            };

        private void LateUpdate()
        {
            if (_camera == null || _labelTransform == null)
                return;

            var toCam = _labelTransform.position - _camera.transform.position;
            if (toCam.sqrMagnitude > 0.0001f)
                _labelTransform.rotation = Quaternion.LookRotation(toCam);
        }

        private void OnDestroy()
        {
            _disp?.Dispose();
            _disp = null;
            DOTween.Kill(_tweenId, false);
            if (_cubeMaterial != null)
                Destroy(_cubeMaterial);
        }

        private void BuildWorldLabel()
        {
            var labelGo = new GameObject("WorldLabel");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, LabelYOffset, 0f);
            labelGo.transform.localScale = Vector3.one * 0.07f;

            _labelTransform = labelGo.transform;
            _labelTmp = labelGo.AddComponent<TextMeshPro>();
            var font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                _labelTmp.font = font;
                _labelTmp.fontSharedMaterial = font.material;
            }

            _labelTmp.alignment = TextAlignmentOptions.Center;
            _labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            _labelTmp.richText = true;
        }

        private void RefreshLabel()
        {
            if (_labelTmp == null || _facade == null)
                return;

            var lvl = _facade.GetBuildingLevel(_slotIndex);
            var cost = _facade.GetUpgradeCost(_slotIndex);
            var maxed = cost == long.MaxValue;
            var canAfford = !maxed && _facade.Gold >= cost;

            if (maxed)
            {
                _labelTmp.text = $"<b>Lv {lvl}</b>\n<size=75%><color=#AAB0BC>MAX</color></size>";
                return;
            }

            var costColor = canAfford ? "#8CE78C" : "#FF8A65";
            _labelTmp.text =
                $"<b>Lv {lvl}</b>\n<size=78%><color={costColor}>Upgrade {cost} g</color></size>";
        }

        private void PulseTapHighlight()
        {
            if (_cubeMaterial == null || _facade == null)
                return;

            var cost = _facade.GetUpgradeCost(_slotIndex);
            var maxed = cost == long.MaxValue;
            var canAfford = !maxed && _facade.Gold >= cost;
            var pulse = maxed
                ? new Color(0.55f, 0.58f, 0.65f)
                : canAfford
                    ? new Color(1f, 0.92f, 0.45f)
                    : new Color(1f, 0.38f, 0.35f);

            DOTween.Kill(_tweenId, false);
            _cubeMaterial.color = _baseCubeColor;

            DOTween.Sequence()
                .SetId(_tweenId)
                .Append(_cubeMaterial.DOColor(pulse, 0.09f).SetEase(Ease.OutQuad))
                .Append(_cubeMaterial.DOColor(_baseCubeColor, 0.28f).SetEase(Ease.InQuad));
        }

        private void OnUpgradeSpend(IdleEmpireUpgradeSpend ev)
        {
            DOTween.Kill(_tweenId, false);
            if (_cubeMaterial != null)
                _cubeMaterial.color = _baseCubeColor;

            transform.DOPunchScale(Vector3.one * 0.22f, 0.55f, 10, 0.38f)
                .SetEase(Ease.OutQuad)
                .SetId(_tweenId)
                .OnComplete(() => transform.localScale = _baseScale);

            SpawnSpendFloat(ev.GoldSpent);
        }

        private void SpawnSpendFloat(long spent)
        {
            var anchor = transform.position + Vector3.up * SpendFloatYOffset;
            var go = new GameObject("SpendFloat");
            go.transform.SetPositionAndRotation(anchor, Quaternion.identity);
            go.transform.localScale = Vector3.one * 0.11f;

            var tmp = go.AddComponent<TextMeshPro>();
            var font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = font.material;
            }

            tmp.text = $"-{spent} <size=70%>g</size>";
            tmp.fontSize = 5.2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.78f, 0.28f, 1f);

            if (_camera != null)
            {
                var d = go.transform.position - _camera.transform.position;
                if (d.sqrMagnitude > 0.0001f)
                    go.transform.rotation = Quaternion.LookRotation(d);
            }

            var floatId = go.GetInstanceID();
            var endCol = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 0f);
            DOTween.Sequence()
                .SetId(floatId)
                .Append(go.transform.DOMoveY(anchor.y + 2.1f, 0.9f).SetEase(Ease.OutQuad))
                .Join(DOTween.To(() => tmp.color, c => tmp.color = c, endCol, 0.58f)
                    .SetEase(Ease.InQuad)
                    .SetDelay(0.2f))
                .OnComplete(() =>
                {
                    if (go != null)
                        Destroy(go);
                });
        }
    }
}
