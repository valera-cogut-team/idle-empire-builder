using Logger.Facade;

namespace IdleEmpireBuilder.Data
{
    public sealed class IdleEmpireAnalyticsNull : IIdleEmpireAnalytics
    {
        private readonly ILoggerFacade _logger;

        public IdleEmpireAnalyticsNull(ILoggerFacade logger)
        {
            _logger = logger;
        }

        public void LogSessionStart()
        {
            _logger?.LogInfo("[IdleEmpireBuilder] Session start (analytics stub).");
        }

        public void LogTapGold(long amount, long totalGold)
        {
            _logger?.LogDebug($"[IdleEmpireBuilder] Tap gold +{amount} (total {totalGold}).");
        }

        public void LogUpgrade(int buildingIndex, int newLevel, long goldAfter)
        {
            _logger?.LogDebug($"[IdleEmpireBuilder] Upgrade building {buildingIndex} → L{newLevel} (gold {goldAfter}).");
        }
    }
}
