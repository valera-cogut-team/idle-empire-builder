using System;
using System.Threading;
using Addressables.Facade;
using Core;
using Cysharp.Threading.Tasks;
using IdleEmpireBuilder.Application;
using IdleEmpireBuilder.Data;
using IdleEmpireBuilder.Facade;
using Logger.Facade;
using Storage.Facade;

namespace IdleEmpireBuilder
{
    /// <summary>Economy-only module: tuning load, state restore, facade bindings.</summary>
    public sealed class IdleEmpireEconomyModule : IModule
    {
        public string Name => "IdleEmpireEconomy";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "Storage", "Addressables" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IdleEmpireFacade _facade;
        private IAddressablesFacade _addressables;
        private bool _contentReady;
        private IIdleEmpireAnalytics _analytics;

        public void Initialize(IModuleContext context)
        {
            _context = context;
        }

        public async UniTask PrepareContentAsync(CancellationToken cancellationToken = default)
        {
            if (_contentReady)
                return;

            if (_context == null)
                throw new InvalidOperationException("IdleEmpireEconomyModule.Initialize must run before PrepareContentAsync.");

            _addressables = _context.GetModuleFacade<IAddressablesFacade>()
                            ?? throw new InvalidOperationException("IAddressablesFacade is not registered.");

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            IdleEmpireTuningConfig tuning;

            try
            {
                tuning = await _addressables.LoadAssetAsync<IdleEmpireTuningConfig>(IdleEmpireAddressKeys.Config)
                    .AttachExternalCancellation(cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError($"[IdleEmpireBuilder] Failed to load '{IdleEmpireAddressKeys.Config}': {ex.Message}", ex);
                tuning = IdleEmpireTuningConfig.CreateRuntimeDefault();
            }

            if (tuning == null)
                tuning = IdleEmpireTuningConfig.CreateRuntimeDefault();

            var state = new IdleEmpireGameState();
            var storage = _context.GetModuleFacade<IStorageFacade>();
            IdleEmpireStatePersistence.ApplySnapshot(state, tuning, storage);

            _analytics = new IdleEmpireAnalyticsNull(logger);
            _facade = new IdleEmpireFacade(state, tuning, storage, _analytics);

            _context.Container.Bind<IdleEmpireTuningConfig>().FromInstance(tuning).AsSingle();
            _context.Container.Bind<IIdleEmpireFacade>().FromInstance(_facade).AsSingle();
            _context.Container.Bind<IIdleEmpireState>().FromInstance(_facade).AsSingle();

            _contentReady = true;
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            if (!_contentReady || _facade == null)
                return;

            IsEnabled = true;
            _analytics?.LogSessionStart();
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;
        }

        public void Shutdown()
        {
            Disable();

            if (_addressables != null && _contentReady)
                _addressables.ReleaseAssetAsync(IdleEmpireAddressKeys.Config).Forget();

            if (_facade is IDisposable d)
                d.Dispose();

            _facade = null;
            _addressables = null;
            _analytics = null;
            _contentReady = false;
        }
    }
}
