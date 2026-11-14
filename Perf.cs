using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace AbilityWheel;

// Debug log only: what the wheel costs a frame. A weapon switch (the one thing the wheel does that
// the game only does from its paused menu) reports how long the game's equip call took and the
// longest frame that followed; any Update of the wheel's own past SlowUpdateMs is reported too.
public partial class Wheel
{
    private const int WatchedFrames = 40;
    private const double SlowUpdateMs = 2.0;

    private int _watchLeft;
    private int _watchWorstAt;
    private float _watchWorst;
    private float _usualFrame;   // seconds, a running average outside watched stretches
    private string _watchWhat;
    private long _lastUpdateAt;
    private int _gcSeen;
    private long _allocatedAtStart, _allocatedByWheel, _allocatedFrames, _allocatedByAll;

    private long _markStart, _markInput, _markWeapons, _markSpells;

    internal static double Ms(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;

    [HideFromIl2Cpp]
    private void WatchFrames(string what)
    {
        _watchWhat = what;
        _watchLeft = WatchedFrames;
        _watchWorst = 0f;
        _watchWorstAt = 0;
    }

    // First thing in Update: the time since the Update before is how long that frame took. By the
    // clock, not Time.unscaledDeltaTime: Unity reports a fixed step while the window has no focus.
    [HideFromIl2Cpp]
    private void TrackFrames()
    {
        float frame = _lastUpdateAt == 0 ? 0f : (float)(Ms(_lastUpdateAt, _markStart) / 1000.0);
        _lastUpdateAt = _markStart;
        _allocatedAtStart = System.GC.GetAllocatedBytesForCurrentThread();
        if (_watchLeft <= 0)
        {
            _usualFrame = _usualFrame <= 0f ? frame : Mathf.Lerp(_usualFrame, frame, 0.05f);
            return;
        }
        if (frame > _watchWorst)
        {
            _watchWorst = frame;
            _watchWorstAt = WatchedFrames - _watchLeft + 1;
        }
        if (--_watchLeft == 0)
            Plugin.Logger.LogDebug(
                $"[perf] {_watchWhat}: longest of the next {WatchedFrames} frames {_watchWorst * 1000f:0.0} ms " +
                $"(frame {_watchWorstAt}), usual {_usualFrame * 1000f:0.0} ms");
    }

    [HideFromIl2Cpp]
    private void ReportUpdate()
    {
        long end = Stopwatch.GetTimestamp();
        double total = Ms(_markStart, end);
        _allocatedByWheel += System.GC.GetAllocatedBytesForCurrentThread() - _allocatedAtStart;
        _allocatedFrames++;
        int gc = System.GC.CollectionCount(0) + System.GC.CollectionCount(1) + System.GC.CollectionCount(2);
        bool collected = gc != _gcSeen;
        _gcSeen = gc;
        if (collected)
        {
            // The .NET runtime is shared by every BepInEx mod, and so are its collections.
            var info = System.GC.GetGCMemoryInfo();
            double pause = 0;
            foreach (var part in info.PauseDurations)
                pause += part.TotalMilliseconds;
            long all = System.GC.GetTotalAllocatedBytes(false);
            Plugin.Logger.LogDebug(
                $"[perf] .NET garbage collection (gen {info.Generation}{(info.Concurrent ? ", concurrent" : "")}): paused {pause:0.0} ms, " +
                $"heap {info.HeapSizeBytes / 1048576.0:0.0} MB; since the one before, every mod together allocated " +
                $"{(all - _allocatedByAll) / 1024.0:0} KB, the wheel {_allocatedByWheel / 1024.0:0} KB of it " +
                $"({(_allocatedFrames > 0 ? _allocatedByWheel / _allocatedFrames : 0)} B a frame over {_allocatedFrames} frames)");
            _allocatedByAll = all;
            _allocatedByWheel = 0;
            _allocatedFrames = 0;
        }
        if (total < SlowUpdateMs)
            return;
        Plugin.Logger.LogDebug(
            $"[perf] slow update {total:0.0} ms: input {Ms(_markStart, _markInput):0.0}, weapons {Ms(_markInput, _markWeapons):0.0}, " +
            $"spells {Ms(_markWeapons, _markSpells):0.0}, hud {Ms(_markSpells, end):0.0}{(collected ? " (.NET garbage collection since the last update)" : "")}");
    }
}
