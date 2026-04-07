using Audio.Facade;
using Core;
using IdleEmpireBuilder.Application;
using IdleEmpireBuilder.Facade;
using IdleEmpireBuilder.Presentation;
using Input.Facade;
using LifeCycle.Facade;
using Logger.Facade;
using Shaker;
using UnityEngine;

namespace IdleEmpireBuilder
{
    /// <summary>Presentation/runtime world module: tick registration and world root lifecycle.</summary>
    public sealed class IdleEmpireWorldModule : IModule
    {
        public string Name => "IdleEmpireWorld";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "Input", "LifeCycle", "Audio", "Shaker", "IdleEmpireEconomy" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IdleEmpireGameTickService _tick;
        private GameObject _root;

        public void Initialize(IModuleContext context)
        {
            _context = context;
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            var facade = _context.GetModuleFacade<IIdleEmpireFacade>();
            var tuning = _context.GetModuleFacade<IdleEmpireTuningConfig>();
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            var input = _context.Container.Resolve<IInputFacade>();
            var lifeCycle = _context.GetModuleFacade<ILifeCycleFacade>();
            var audio = _context.GetModuleFacade<IAudioFacade>();
            var shaker = _context.GetModuleFacade<IShakerFacade>();

            if (facade == null || tuning == null || input == null)
            {
                logger?.LogError("[IdleEmpireBuilder] IdleEmpireWorldModule missing critical dependencies.");
                return;
            }

            IsEnabled = true;
            _tick = new IdleEmpireGameTickService(input, facade);
            lifeCycle?.RegisterUpdateHandler(_tick);

            _root = new GameObject("IdleEmpireBuilderWorld");
            Object.DontDestroyOnLoad(_root);
            var rootDriver = _root.AddComponent<IdleEmpireGameplayRoot>();
            rootDriver.Initialize(_tick, facade, logger, tuning, audio, shaker);
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;

            var lifeCycle = _context?.GetModuleFacade<ILifeCycleFacade>();
            if (_tick != null && lifeCycle != null)
                lifeCycle.UnregisterUpdateHandler(_tick);

            _tick = null;

            if (_root != null)
            {
                Object.Destroy(_root);
                _root = null;
            }
        }

        public void Shutdown()
        {
            Disable();
        }
    }
}
