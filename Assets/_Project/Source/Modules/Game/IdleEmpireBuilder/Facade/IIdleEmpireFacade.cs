using System;
using IdleEmpireBuilder.Domain;
using UniRx;
using UnityEngine;

namespace IdleEmpireBuilder.Facade
{
    public interface IIdleEmpireFacade : IIdleEmpireState
    {
        IObservable<Unit> TapGoldPulse { get; }
        /// <summary>Fired when the player taps a valid building slot (before gold check outcome).</summary>
        IObservable<int> BuildingSlotTappedPulse { get; }
        /// <summary>Fired only when gold was spent and level increased.</summary>
        IObservable<IdleEmpireUpgradeSpend> UpgradeSpendPulse { get; }

        IReadOnlyReactiveProperty<IdleEmpireGamePhase> PhaseRx { get; }
        IReadOnlyReactiveProperty<long> GoldRx { get; }
        IReadOnlyReactiveProperty<float> IncomeRx { get; }
        IReadOnlyReactiveProperty<int> Building0Rx { get; }
        IReadOnlyReactiveProperty<int> Building1Rx { get; }
        IReadOnlyReactiveProperty<int> Building2Rx { get; }

        void TickPassive(float deltaTime);
        void AddTapGold();
        bool TryUpgradeBuilding(int buildingIndex);
    }
}
