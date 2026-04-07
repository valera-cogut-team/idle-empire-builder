using Audio.Facade;
using IdleEmpireBuilder.Application;
using IdleEmpireBuilder.Facade;
using Shaker;
using UniRx;
using UnityEngine;

namespace IdleEmpireBuilder.Presentation
{
    /// <summary>Lightweight procedural audio + shaker feedback (performance book — avoid asset churn).</summary>
    public sealed class IdleEmpireFeedback : MonoBehaviour
    {
        private static AudioClip _tapClip;
        private static AudioClip _upgradeClip;

        private IIdleEmpireFacade _facade;
        private IAudioFacade _audio;
        private IShakerFacade _shaker;
        private IdleEmpireTuningConfig _tuning;
        private CompositeDisposable _dsp;

        public void Initialize(
            IIdleEmpireFacade facade,
            IAudioFacade audio,
            IShakerFacade shaker,
            Transform cameraTransform,
            IdleEmpireTuningConfig tuning)
        {
            _facade = facade;
            _audio = audio;
            _shaker = shaker;
            _tuning = tuning ?? IdleEmpireTuningConfig.CreateRuntimeDefault();

            _shaker?.SetTarget(cameraTransform);

            _dsp?.Dispose();
            _dsp = new CompositeDisposable();

            if (_facade == null)
                return;

            _facade.TapGoldPulse.Subscribe(_ => PlayTap()).AddTo(_dsp);
            _facade.UpgradeSpendPulse.Subscribe(_ => PlayUpgrade()).AddTo(_dsp);
        }

        private void OnDestroy()
        {
            _dsp?.Dispose();
            _dsp = null;
            _shaker?.SetTarget(null);
        }

        private void PlayTap()
        {
            if (_audio == null || _tuning == null || !_tuning.enableTapSound)
                return;
            EnsureClips();
            _audio.PlaySound2D(_tapClip, _tuning.tapSfxVolume);
        }

        private void PlayUpgrade()
        {
            if (_tuning == null)
                return;
            if (_audio != null && _tuning.enableUpgradeSound)
            {
                EnsureClips();
                _audio.PlaySound2D(_upgradeClip, _tuning.upgradeSfxVolume);
            }

            if (_shaker != null && _tuning.enableUpgradeShake)
                _shaker.AddImpulse(_tuning.upgradeShakeStrength);
        }

        private static void EnsureClips()
        {
            if (_tapClip == null)
                _tapClip = BuildToneClip("IdleEmpireTap", 660f, 0.04f, 0.22f);
            if (_upgradeClip == null)
                _upgradeClip = BuildToneClip("IdleEmpireUpgrade", 880f, 0.09f, 0.28f);
        }

        private static AudioClip BuildToneClip(string name, float freqHz, float duration, float gain)
        {
            var sampleRate = 24000;
            var n = Mathf.Max(64, Mathf.CeilToInt(sampleRate * duration));
            var data = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Exp(-t * 22f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freqHz * t) * env * gain;
            }

            var clip = AudioClip.Create(name, n, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
