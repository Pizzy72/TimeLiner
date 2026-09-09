# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Christian Pistor
param(
    [Parameter(Mandatory = $true)][string]$InputPath,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$data = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
$rows = foreach ($run in $data | Where-Object { $null -ne $_.Trace }) {
    $frames = $run.Trace.Frames
    for ($i = 1; $i -lt $frames.Count; $i++) {
        $previous = $frames[$i - 1]
        $current = $frames[$i]
        $duration = $current.AtMs - $previous.AtMs
        $render = 0.0; $collision = 0.0; $timer = 0.0; $other = 0.0
        $renderOperations = 0
        foreach ($operation in $run.Trace.Operations) {
            # Only outermost operations: nested dispatcher frames would otherwise
            # double-count time. Clip operations at rendering-callback boundaries.
            if ($operation.Depth -ne 0) { continue }
            $overlap = [Math]::Min([double]$current.AtMs, [double]$operation.EndMs) - [Math]::Max([double]$previous.AtMs, [double]$operation.StartMs)
            if ($overlap -le 0) { continue }
            if ($operation.Callback -like 'System.Windows.Media.MediaContext.*RenderMessageHandler') {
                $render += $overlap
                $renderOperations++
            } elseif ($operation.Callback -like '*TimelineItemTextBehavior*') {
                $collision += $overlap
            } elseif ($operation.Callback -like 'System.Windows.Threading.DispatcherTimer*') {
                $timer += $overlap
            } else { $other += $overlap }
        }
        $unattributed = $duration - $render - $collision - $timer - $other
        if ($unattributed -lt -0.001) { throw 'Overlapping outermost dispatcher operations.' }
        [pscustomobject]@{
            Scale = $run.Scale; Run = $run.Run; Interval = $i
            StartMs = $previous.AtMs; EndMs = $current.AtMs; FrameMs = $duration
            RenderMs = $render; CollisionMs = $collision; TimerMs = $timer
            OtherDispatcherMs = $other; UnattributedMs = [Math]::Max(0.0, $unattributed)
            RenderOperations = $renderOperations
            ScrollSteps = $current.Step - $previous.Step
            LayoutCycles = $current.Layouts - $previous.Layouts
            ItemLoads = $current.Loads - $previous.Loads
            ScrollSetterMs = $current.SetterMs - $previous.SetterMs
            TextLayoutCalls = if ($null -eq $current.TextLayoutCalls) { $null } else { $current.TextLayoutCalls - $previous.TextLayoutCalls }
            TextLayoutMs = if ($null -eq $current.TextLayoutMs) { $null } else { $current.TextLayoutMs - $previous.TextLayoutMs }
            UiAllocatedBytes = $current.AllocatedBytes - $previous.AllocatedBytes
            Gen0 = $current.Gen0 - $previous.Gen0
            Gen1 = $current.Gen1 - $previous.Gen1
            Gen2 = $current.Gen2 - $previous.Gen2
            GcPauseMs = if ($null -eq $current.GcPauseMs) { $null } else { $current.GcPauseMs - $previous.GcPauseMs }
        }
    }
}
if (-not $rows) { throw 'No detailed frame intervals found. Enable TIMELINER_PROFILE_FRAMES=1.' }
$rows | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding utf8
$rows | Sort-Object FrameMs -Descending | Select-Object -First 12 `
    Scale, Run, Interval, FrameMs, RenderMs, CollisionMs, TimerMs, UnattributedMs, LayoutCycles, ItemLoads, Gen0, Gen1, Gen2 |
    Format-Table -AutoSize
