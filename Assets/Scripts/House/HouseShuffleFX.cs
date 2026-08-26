using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrustNoOne.Shuffle
{
    // goes on the House object next to HouseShuffleController.
    // flickers the lights whenever the house rearranges
    public class HouseShuffleFX : MonoBehaviour
    {
        [Tooltip("leave empty to grab every light under this object")]
        public Light[] lights;

        [Header("Feel")]
        public float duration = 1.2f;
        public float minGap = 0.03f;
        public float maxGap = 0.12f;

        [Range(0f, 1f)]
        [Tooltip("0 = full blackout on the off beats")]
        public float dimTo = 0f;

        Coroutine running;

        void Awake()
        {
            if (lights == null || lights.Length == 0)
                lights = GetComponentsInChildren<Light>(true);
        }

        void OnEnable() { GameEvents.HouseShuffled += Play; }
        void OnDisable() { GameEvents.HouseShuffled -= Play; }

        [ContextMenu("Play Flicker")]
        public void Play()
        {
            if (!isActiveAndEnabled) return;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Flicker());
        }

        IEnumerator Flicker()
        {
            // only touch lights that were already on, don't relight a room the player darkened
            var active = new List<Light>();
            var levels = new List<float>();

            foreach (var l in lights)
            {
                if (l == null || !l.enabled || !l.gameObject.activeInHierarchy) continue;
                active.Add(l);
                levels.Add(l.intensity);
            }

            float t = 0f;
            bool dark = false;

            while (t < duration)
            {
                dark = !dark;
                for (int i = 0; i < active.Count; i++)
                {
                    if (active[i] == null) continue;
                    active[i].intensity = dark ? levels[i] * dimTo : levels[i];
                }

                float gap = Random.Range(minGap, maxGap);
                t += gap;
                yield return new WaitForSeconds(gap);
            }

            // always land back on the original values
            for (int i = 0; i < active.Count; i++)
                if (active[i] != null) active[i].intensity = levels[i];

            running = null;
        }
    }
}