namespace IdleEmpireBuilder.Data
{
    public interface IIdleEmpireAnalytics
    {
        void LogSessionStart();
        void LogTapGold(long amount, long totalGold);
        void LogUpgrade(int buildingIndex, int newLevel, long goldAfter);
    }
}
