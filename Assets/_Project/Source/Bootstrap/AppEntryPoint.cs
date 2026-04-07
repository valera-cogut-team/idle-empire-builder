using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Bootstrap
{
    /// <summary>
    /// Single Unity entrypoint for the IdleEmpireBuilder book companion sample.
    /// Creates a standalone Zenject DiContainer and runs <see cref="AppBootstrap"/>.
    /// </summary>
    public sealed class AppEntryPoint : MonoBehaviour
    {
        [SerializeField] private bool enableDebugLogs = true;

        private AppBootstrap _bootstrap;
        private DiContainer _container;
        private CancellationTokenSource _initCts;

        private void Awake()
        {
            if (FindObjectsByType<AppEntryPoint>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _initCts = new CancellationTokenSource();
            RunBootstrapAsync().Forget();
        }

        private async UniTaskVoid RunBootstrapAsync()
        {
            try
            {
                await InitializeApplicationAsync(_initCts.Token);
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.LogWarning("App initialization was cancelled");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"App initialization failed: {ex}");
            }
        }

        private async UniTask InitializeApplicationAsync(CancellationToken cancellationToken)
        {
            _container = new DiContainer();
            _bootstrap = new AppBootstrap(_container, enableDebugLogs);
            await _bootstrap.InitializeAsync(cancellationToken);
        }

        private void OnDestroy()
        {
            _initCts?.Cancel();
            _initCts?.Dispose();
            _bootstrap?.Shutdown();
        }
    }
}

