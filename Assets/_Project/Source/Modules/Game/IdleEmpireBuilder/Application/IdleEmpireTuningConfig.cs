using UnityEngine;

namespace IdleEmpireBuilder.Application
{
    /// <summary>Designer-tunable idle economy for the book companion sample.</summary>
    [CreateAssetMenu(fileName = "IdleEmpireBuilderTuning", menuName = "IdleEmpireBuilder/Tuning Config")]
    public sealed class IdleEmpireTuningConfig : ScriptableObject
    {
        [Header("Economy")]
        [Min(0)] public long startingGold = 40;
        [Min(1)] public long tapGoldAmount = 3;
        [Min(0.01f)] public float passiveGoldPerBuildingLevelPerSecond = 1.25f;

        [Header("Upgrades")]
        [Min(1)] public long upgradeBaseCost = 25;
        [Min(1.01f)] public float upgradeCostGrowth = 1.35f;
        [Min(1)] public int maxBuildingLevel = 80;

        [Header("World")]
        [Min(4f)] public float cameraDistance = 14f;
        [Min(5f)] public float cameraHeight = 10f;
        [Min(0.01f)] public float cameraSmooth = 8f;

        [Header("Feedback (procedural SFX — no WAV assets)")]
        public bool enableTapSound = true;
        [Range(0f, 1f)] public float tapSfxVolume = 0.22f;
        public bool enableUpgradeSound = true;
        [Range(0f, 1f)] public float upgradeSfxVolume = 0.32f;
        public bool enableUpgradeShake = true;
        [Min(0f)] public float upgradeShakeStrength = 0.35f;

        [Header("Addressables")]
        public string tuningConfigAddress = IdleEmpireAddressKeys.Config;

        public static IdleEmpireTuningConfig CreateRuntimeDefault()
        {
            var c = CreateInstance<IdleEmpireTuningConfig>();
            c.hideFlags = HideFlags.HideAndDontSave;
            return c;
        }
    }
}
