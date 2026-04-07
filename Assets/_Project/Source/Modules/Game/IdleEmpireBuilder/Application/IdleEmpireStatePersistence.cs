using System;
using Storage.Facade;
using UnityEngine;

namespace IdleEmpireBuilder.Application
{
    /// <summary>Loads initial economy snapshot from storage into runtime state.</summary>
    public static class IdleEmpireStatePersistence
    {
        public static void ApplySnapshot(
            IdleEmpireGameState state,
            IdleEmpireTuningConfig tuning,
            IStorageFacade storage)
        {
            if (state == null || tuning == null)
                return;

            for (var i = 0; i < IdleEmpireGameState.BuildingCount; i++)
                state.BuildingLevels[i] = 1;

            if (storage == null)
            {
                state.Gold = Math.Max(0L, tuning.startingGold);
                return;
            }

            var gold = tuning.startingGold;
            if (storage.HasKey(IdleEmpirePersistenceKeys.Gold))
            {
                var raw = storage.GetString(IdleEmpirePersistenceKeys.Gold, null);
                if (!string.IsNullOrEmpty(raw) && long.TryParse(raw, out var parsed))
                    gold = parsed;
            }

            state.Gold = Math.Max(0L, gold);
            state.BuildingLevels[0] = ClampLevel(storage.GetInt(IdleEmpirePersistenceKeys.BuildingLevel0, 1), tuning.maxBuildingLevel);
            state.BuildingLevels[1] = ClampLevel(storage.GetInt(IdleEmpirePersistenceKeys.BuildingLevel1, 1), tuning.maxBuildingLevel);
            state.BuildingLevels[2] = ClampLevel(storage.GetInt(IdleEmpirePersistenceKeys.BuildingLevel2, 1), tuning.maxBuildingLevel);
        }

        private static int ClampLevel(int level, int maxLevel)
        {
            level = Mathf.Max(1, level);
            return Mathf.Min(level, Mathf.Max(1, maxLevel));
        }
    }
}
