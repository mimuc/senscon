using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif
using UnityEngine;
using UnityEngine.XR.Management;

namespace Util
{
    /// <summary>
    /// Initializes the active XR loader on <c>Start</c>, and deinitializes in <c>OnDestroy</c> and <see cref="StopXR"/>.
    /// This is useful if you don't want to rely on the XR Plugin Management to init and deinit the XR subsystem.
    /// Initialization error messages are given in <see cref="XRInitializationError"/>, if an error occurred.
    /// </summary>
    public class ManualXRManager : MonoBehaviour
    {
        private string xrInitializationError;
        public string XRInitializationError => xrInitializationError;

        protected virtual void Start()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitXR());
        }

        /// <summary>
        /// Initializes the currently selected XR Loader.
        /// Side effect: stores an error message in <see cref="xrInitializationError"/> if initialization fails.
        /// </summary>
        protected IEnumerator InitXR()
        {
#if UNITY_EDITOR
            Debug.Log("Loading XR settings...");
            /// allow Unity Editor to settle into Play Mode
            yield return new WaitForSeconds(1f);
            /// load XR settings
            XRGeneralSettings.Instance = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            var xrManager = XRGeneralSettings.Instance.Manager;

            Debug.Log("Initializing XR...");
            yield return xrManager.InitializeLoader();

            /// did it work?
            if (xrManager.isInitializationComplete)
            {
                Debug.Log($"{XRGeneralSettings.Instance.Manager.activeLoader.name} initialized. Starting XR Subsystem.");
                xrManager.StartSubsystems();
            }
            else
            {
                xrInitializationError = "Initializing XR Failed. Check Editor or Player log for details.";
                Debug.LogError(xrInitializationError);
            }
        }

        public IEnumerator StopXR()
        {
            Debug.Log("Stopping XR...");

            XRGeneralSettings.Instance.Manager.DeinitializeLoader();

            /// wait for XR deinit
            while (XRGeneralSettings.Instance.Manager.isInitializationComplete)
                yield return new WaitForSeconds(0.2f);

            Debug.Log("XR stopped.");
        }

        protected virtual void OnDestroy()
        {
            /// deinit XR in case it is still initialized
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}
