namespace IdleEmpireBuilder.Facade
{
    /// <summary>Published when an upgrade succeeds (for spend VFX / DOTween).</summary>
    public readonly struct IdleEmpireUpgradeSpend
    {
        public readonly int SlotIndex;
        public readonly long GoldSpent;

        public IdleEmpireUpgradeSpend(int slotIndex, long goldSpent)
        {
            SlotIndex = slotIndex;
            GoldSpent = goldSpent;
        }
    }
}
