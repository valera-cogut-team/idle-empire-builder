using Input.Domain;
using Input.Facade;
using IdleEmpireBuilder.Facade;
using IdleEmpireBuilder.Presentation;
using LifeCycle.Facade;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleEmpireBuilder.Application
{
    /// <summary>Central per-frame pump: passive income + tap / building raycasts (single update handler).</summary>
    public sealed class IdleEmpireGameTickService : IUpdateHandler
    {
        private readonly IInputFacade _input;
        private readonly IIdleEmpireFacade _facade;
        private Camera _camera;

        public IdleEmpireGameTickService(IInputFacade input, IIdleEmpireFacade facade)
        {
            _input = input;
            _facade = facade;
        }

        public void AttachCamera(Camera camera)
        {
            _camera = camera;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_facade == null)
                return;

            _facade.TickPassive(deltaTime);

            if (!PollInteractDown(out var screenPoint))
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
            {
                _facade.AddTapGold();
                return;
            }

            var ray = _camera.ScreenPointToRay(screenPoint);
            if (Physics.Raycast(ray, out var hit, 200f, ~0, QueryTriggerInteraction.Collide))
            {
                var slot = hit.collider.GetComponentInParent<IdleEmpireBuildingSlotView>();
                if (slot != null)
                {
                    _facade.TryUpgradeBuilding(slot.SlotIndex);
                    return;
                }
            }

            _facade.AddTapGold();
        }

        private bool PollInteractDown(out Vector2 screenPoint)
        {
            screenPoint = default;

            if (_input.GetButtonDown("Fire1"))
            {
                _input.GetPointerPosition(out var x, out var y);
                screenPoint = new Vector2(x, y);
                return true;
            }

            if (_input.TouchCount <= 0)
                return false;

            _input.GetTouch(0, out var tx, out var ty, out var phase);
            if (phase != InputTouchPhase.Began)
                return false;

            screenPoint = new Vector2(tx, ty);
            return true;
        }
    }
}
