using IdleEmpireBuilder.Domain;

namespace IdleEmpireBuilder.Facade
{
    public interface IIdleEmpireState
    {
        IdleEmpireGamePhase Phase { get; }
        long Gold { get; }
        float IncomePerSecond { get; }
        int GetBuildingLevel(int index);
        long GetUpgradeCost(int index);
    }
}
