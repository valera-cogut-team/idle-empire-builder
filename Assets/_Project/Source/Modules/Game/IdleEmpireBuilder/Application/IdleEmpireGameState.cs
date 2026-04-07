using IdleEmpireBuilder.Domain;

namespace IdleEmpireBuilder.Application
{
    public sealed class IdleEmpireGameState
    {
        public const int BuildingCount = 3;

        public IdleEmpireGamePhase Phase = IdleEmpireGamePhase.Playing;
        public long Gold;
        public readonly int[] BuildingLevels = new int[BuildingCount];
    }
}
