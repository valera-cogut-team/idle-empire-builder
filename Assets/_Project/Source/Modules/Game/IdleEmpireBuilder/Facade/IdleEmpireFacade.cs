using System;
using IdleEmpireBuilder.Application;
using IdleEmpireBuilder.Data;
using IdleEmpireBuilder.Domain;
using Storage.Facade;
using UniRx;
using UnityEngine;

namespace IdleEmpireBuilder.Facade
{
    public sealed class IdleEmpireFacade : IIdleEmpireFacade, IDisposable
    {
        private readonly IdleEmpireGameState _state;
        private readonly IdleEmpireTuningConfig _tuning;
        private readonly IStorageFacade _storage;
        private readonly IIdleEmpireAnalytics _analytics;

        private readonly Subject<Unit> _tapGoldPulse = new Subject<Unit>();
        private readonly Subject<int> _buildingSlotTapped = new Subject<int>();
        private readonly Subject<IdleEmpireUpgradeSpend> _upgradeSpendPulse = new Subject<IdleEmpireUpgradeSpend>();

        private readonly ReactiveProperty<IdleEmpireGamePhase> _phaseRx;
        private readonly ReactiveProperty<long> _goldRx;
        private readonly ReactiveProperty<float> _incomeRx;
        private readonly ReactiveProperty<int>[] _buildingRx;

        private double _passiveCarry;
        private bool _disposed;

        public IdleEmpireFacade(
            IdleEmpireGameState state,
            IdleEmpireTuningConfig tuning,
            IStorageFacade storage,
            IIdleEmpireAnalytics analytics)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _tuning = tuning ?? IdleEmpireTuningConfig.CreateRuntimeDefault();
            _storage = storage;
            _analytics = analytics;

            _phaseRx = new ReactiveProperty<IdleEmpireGamePhase>(_state.Phase);
            _goldRx = new ReactiveProperty<long>(_state.Gold);
            _incomeRx = new ReactiveProperty<float>(ComputeIncomePerSecond());
            _buildingRx = new[]
            {
                new ReactiveProperty<int>(_state.BuildingLevels[0]),
                new ReactiveProperty<int>(_state.BuildingLevels[1]),
                new ReactiveProperty<int>(_state.BuildingLevels[2])
            };
        }

        public IObservable<Unit> TapGoldPulse => _tapGoldPulse;
        public IObservable<int> BuildingSlotTappedPulse => _buildingSlotTapped;
        public IObservable<IdleEmpireUpgradeSpend> UpgradeSpendPulse => _upgradeSpendPulse;

        public IReadOnlyReactiveProperty<IdleEmpireGamePhase> PhaseRx => _phaseRx;
        public IReadOnlyReactiveProperty<long> GoldRx => _goldRx;
        public IReadOnlyReactiveProperty<float> IncomeRx => _incomeRx;
        public IReadOnlyReactiveProperty<int> Building0Rx => _buildingRx[0];
        public IReadOnlyReactiveProperty<int> Building1Rx => _buildingRx[1];
        public IReadOnlyReactiveProperty<int> Building2Rx => _buildingRx[2];

        public IdleEmpireGamePhase Phase => _state.Phase;
        public long Gold => _state.Gold;
        public float IncomePerSecond => ComputeIncomePerSecond();

        public int GetBuildingLevel(int index) =>
            index >= 0 && index < IdleEmpireGameState.BuildingCount ? _state.BuildingLevels[index] : 0;

        public long GetUpgradeCost(int index)
        {
            if (index < 0 || index >= IdleEmpireGameState.BuildingCount)
                return long.MaxValue;
            var level = _state.BuildingLevels[index];
            if (level >= _tuning.maxBuildingLevel)
                return long.MaxValue;
            return ComputeUpgradeCost(level);
        }

        public void TickPassive(float deltaTime)
        {
            if (_state.Phase != IdleEmpireGamePhase.Playing)
                return;

            var income = ComputeIncomePerSecond();
            if (income <= 0f || deltaTime <= 0f)
                return;

            _passiveCarry += income * deltaTime;
            var whole = (long)Math.Floor(_passiveCarry);
            if (whole <= 0)
                return;

            _passiveCarry -= whole;
            _state.Gold += whole;
            PushGold();
            PersistEconomy();
        }

        public void AddTapGold()
        {
            if (_state.Phase != IdleEmpireGamePhase.Playing)
                return;

            var add = Math.Max(0L, _tuning.tapGoldAmount);
            if (add == 0)
                return;

            _state.Gold += add;
            PushGold();
            PersistEconomy();
            _tapGoldPulse.OnNext(Unit.Default);
            _analytics?.LogTapGold(add, _state.Gold);
        }

        public bool TryUpgradeBuilding(int buildingIndex)
        {
            if (_state.Phase != IdleEmpireGamePhase.Playing)
                return false;
            if (buildingIndex < 0 || buildingIndex >= IdleEmpireGameState.BuildingCount)
                return false;

            _buildingSlotTapped.OnNext(buildingIndex);

            var level = _state.BuildingLevels[buildingIndex];
            if (level >= _tuning.maxBuildingLevel)
                return false;

            var cost = ComputeUpgradeCost(level);
            if (_state.Gold < cost)
                return false;

            _state.Gold -= cost;
            _state.BuildingLevels[buildingIndex] = level + 1;

            PushGold();
            _buildingRx[buildingIndex].Value = _state.BuildingLevels[buildingIndex];
            _incomeRx.Value = ComputeIncomePerSecond();
            PersistEconomy();

            _upgradeSpendPulse.OnNext(new IdleEmpireUpgradeSpend(buildingIndex, cost));
            _analytics?.LogUpgrade(buildingIndex, _state.BuildingLevels[buildingIndex], _state.Gold);
            return true;
        }

        private long ComputeUpgradeCost(int currentLevelBeforeUpgrade)
        {
            var g = Mathf.Max(1.01f, _tuning.upgradeCostGrowth);
            var raw = _tuning.upgradeBaseCost * Math.Pow(g, currentLevelBeforeUpgrade - 1);
            return Math.Max(1L, (long)Math.Round(raw));
        }

        private float ComputeIncomePerSecond()
        {
            var rate = 0f;
            for (var i = 0; i < IdleEmpireGameState.BuildingCount; i++)
                rate += _tuning.passiveGoldPerBuildingLevelPerSecond * Mathf.Max(1, _state.BuildingLevels[i]);
            return rate;
        }

        private void PushGold()
        {
            _goldRx.Value = _state.Gold;
        }

        private void PersistEconomy()
        {
            if (_storage == null)
                return;
            _storage.SetString(IdleEmpirePersistenceKeys.Gold, _state.Gold.ToString());
            _storage.SetInt(IdleEmpirePersistenceKeys.BuildingLevel0, _state.BuildingLevels[0]);
            _storage.SetInt(IdleEmpirePersistenceKeys.BuildingLevel1, _state.BuildingLevels[1]);
            _storage.SetInt(IdleEmpirePersistenceKeys.BuildingLevel2, _state.BuildingLevels[2]);
            _storage.Save();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _tapGoldPulse?.Dispose();
            _buildingSlotTapped?.Dispose();
            _upgradeSpendPulse?.Dispose();
            _phaseRx?.Dispose();
            _goldRx?.Dispose();
            _incomeRx?.Dispose();
            foreach (var b in _buildingRx)
                b?.Dispose();
        }
    }
}
