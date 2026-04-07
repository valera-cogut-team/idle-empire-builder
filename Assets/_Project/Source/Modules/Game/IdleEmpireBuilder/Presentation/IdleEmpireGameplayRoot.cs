using Audio.Facade;
using Cysharp.Threading.Tasks;
using IdleEmpireBuilder.Application;
using IdleEmpireBuilder.Facade;
using Logger.Facade;
using Shaker;
using UnityEngine;

namespace IdleEmpireBuilder.Presentation
{
    /// <summary>Bootstraps 3D world: lighting, camera, ground, buildings, HUD (Chapter 11 sample pattern).</summary>
    public sealed class IdleEmpireGameplayRoot : MonoBehaviour
    {
        private IdleEmpireGameTickService _tick;
        private IIdleEmpireFacade _facade;
        private ILoggerFacade _logger;
        private IdleEmpireTuningConfig _tuning;
        private IAudioFacade _audio;
        private IShakerFacade _shaker;

        public void Initialize(
            IdleEmpireGameTickService tick,
            IIdleEmpireFacade facade,
            ILoggerFacade logger,
            IdleEmpireTuningConfig tuning,
            IAudioFacade audio = null,
            IShakerFacade shaker = null)
        {
            _tick = tick;
            _facade = facade;
            _logger = logger;
            _tuning = tuning ?? IdleEmpireTuningConfig.CreateRuntimeDefault();
            _audio = audio;
            _shaker = shaker;

            BuildWorldAsync().Forget();
        }

        private async UniTaskVoid BuildWorldAsync()
        {
            if (_facade == null || _tick == null)
            {
                _logger?.LogError("[IdleEmpireBuilder] Gameplay root: missing dependencies.");
                return;
            }

            EnsureDefaultWorldLighting();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(2.2f, 1f, 2.2f);

            var cam = CreateRuntimeCamera();
            cam.transform.SetParent(transform, false);
            cam.transform.position = new Vector3(0f, _tuning.cameraHeight, -_tuning.cameraDistance);
            cam.transform.rotation = Quaternion.LookRotation(
                (new Vector3(0f, 1.2f, 0f) - cam.transform.position).normalized,
                Vector3.up);

            _tick.AttachCamera(cam);

            var spacing = 4.5f;
            for (var i = 0; i < IdleEmpireGameState.BuildingCount; i++)
            {
                var bx = (i - 1) * spacing;
                var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{i}";
                building.transform.SetParent(transform, false);
                building.transform.localPosition = new Vector3(bx, 1f, 0f);
                building.transform.localScale = new Vector3(2.2f, 2f, 2.2f);

                var slot = building.AddComponent<IdleEmpireBuildingSlotView>();
                slot.SetSlotIndex(i);

                var col = building.GetComponent<BoxCollider>();
                if (col != null)
                    col.isTrigger = false;

                var visual = building.AddComponent<IdleEmpireBuildingVisual>();
                visual.Configure(i, _facade, cam);
            }

            var hudGo = new GameObject("HudRoot");
            hudGo.transform.SetParent(transform, false);
            var hud = hudGo.AddComponent<IdleEmpireHud>();
            hud.Initialize(_facade);

            var feedbackGo = new GameObject("IdleFeedback");
            feedbackGo.transform.SetParent(transform, false);
            var feedback = feedbackGo.AddComponent<IdleEmpireFeedback>();
            feedback.Initialize(_facade, _audio, _shaker, cam.transform, _tuning);

            await UniTask.Yield();
        }

        private Camera CreateRuntimeCamera()
        {
            var camGo = new GameObject("MainCamera_Runtime");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
            return cam;
        }

        private void EnsureDefaultWorldLighting()
        {
            if (FindObjectsByType<Light>(FindObjectsSortMode.None).Length > 0)
                return;

            var lightGo = new GameObject("IdleEmpireSun");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.rotation = Quaternion.Euler(55f, -40f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.98f, 0.95f);
            light.intensity = 1.05f;
            light.shadows = LightShadows.Soft;
        }
    }
}
