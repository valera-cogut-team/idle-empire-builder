using IdleEmpireBuilder.Application;
using UnityEngine;

namespace IdleEmpireBuilder.Presentation
{
    /// <summary>Marks a 3D building collider for upgrade raycasts (index matches persistence keys).</summary>
    public sealed class IdleEmpireBuildingSlotView : MonoBehaviour
    {
        [SerializeField] private int slotIndex;

        public int SlotIndex => slotIndex;

        public void SetSlotIndex(int index)
        {
            slotIndex = Mathf.Clamp(index, 0, IdleEmpireGameState.BuildingCount - 1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            slotIndex = Mathf.Clamp(slotIndex, 0, IdleEmpireGameState.BuildingCount - 1);
        }
#endif
    }
}
