using System.Collections;
using UnityEngine;

public static class FMODPreload
{
    // Define the exact names of your FMOD banks here
    private static readonly string[] BanksToLoad = { "Master", "Master.strings", "Diegetic", "Music" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void UnifiedFMODInitialization()
    {
        Debug.Log("[FMOD Preloader] Querying browser hardware sample rate...");

        // 1. Fetch the raw, uninitialized core system reference
        FMODUnity.RuntimeManager.StudioSystem.getCoreSystem(out FMOD.System coreSystem);
        
        // 2. Query the browser's native hardware sample rate
        coreSystem.getDriverInfo(0, out _, out int systemRate, out _, out _);
        
        // 3. Force FMOD to match it EXACTLY, eliminating the software resampler CPU strain
        coreSystem.setSoftwareFormat(systemRate, FMOD.SPEAKERMODE.STEREO, 0);
        Debug.Log($"[FMOD Preloader] Match found! Forcing FMOD to run at native {systemRate}Hz.");

        // 4. Trigger the formal FMOD system initialization now that format is locked
        var forceInit = FMODUnity.RuntimeManager.StudioSystem;

        Debug.Log("[FMOD Preloader] FMOD Initialized. Starting bank downloads...");

        // 5. Loop through and manually fire off browser downloads for your banks
        foreach (string bankName in BanksToLoad)
        {
            FMODUnity.RuntimeManager.LoadBank(bankName, true);
            Debug.Log($"[FMOD Preloader] Started loading bank: {bankName}");
        }

        // 6. Create a temporary hidden GameObject to host the loading Coroutine
        GameObject runnerObj = new GameObject("FMOD_Preload_Runner");
        Object.DontDestroyOnLoad(runnerObj);
        var runner = runnerObj.AddComponent<PreloadRunnerBehaviour>();
        runner.StartCoroutine(WaitForAudioReady(runnerObj));
    }

    private static IEnumerator WaitForAudioReady(GameObject runnerToDestroy)
    {
        // Wait for browser network requests to finish writing to memory
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Wait for compressed audio streams to fully unpack on the main thread
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        Debug.Log("[FMOD Preloader] All banks safely cached! Unlocking game execution.");

        // Clean up our temporary runner object
        Object.Destroy(runnerToDestroy);
        yield return null;
    }

    // Tiny helper component used to execute the coroutine
    private class PreloadRunnerBehaviour : MonoBehaviour { }
}