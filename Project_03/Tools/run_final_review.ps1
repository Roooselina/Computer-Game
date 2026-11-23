$ErrorActionPreference='Stop'
$bridgeDir=Join-Path (Get-Location) 'Temp/EWBridge'
$reviewLog=Join-Path (Get-Location) 'Docs/Media/art_play_overhaul/final_verification.log'
function Run-BridgeStep([string]$command,[string]$done,[int]$timeout=180) {
    $resultPath=Join-Path $bridgeDir 'result.txt'
    $before=[IO.File]::ReadAllText($resultPath).Length
    $started=Get-Date
    Set-Content -LiteralPath (Join-Path $bridgeDir 'cmd.txt') -Value $command
    Add-Content -LiteralPath $reviewLog -Value ('START '+$started.ToString('HH:mm:ss')+' '+$command)
    while(((Get-Date)-$started).TotalSeconds -lt $timeout) {
        Start-Sleep -Milliseconds 500
        $all=[IO.File]::ReadAllText($resultPath)
        $new=if($all.Length -ge $before){$all.Substring($before)}else{$all}
        if($new -match 'COMPILE ERRORS|ERROR System\.') {Add-Content -LiteralPath $reviewLog -Value $new;throw 'Unity bridge error'}
        if($new -match $done) {Add-Content -LiteralPath $reviewLog -Value $new;Write-Output ('DONE '+$command);return}
    }
    throw ('TIMEOUT '+$command)
}
Run-BridgeStep 'call EW.EditorTools.IllustratedArtSetup.Rebuild' 'RESULT EW.EditorTools.IllustratedArtSetup.Rebuild'
Run-BridgeStep 'rec Assets/Game/Scenes/C06_LongCorridor.unity 28 0.5 Docs/Media/art_play_overhaul/venelle_final EW.OverhaulReviewHooks.DesktopShot' 'rec done frames' 65
Run-BridgeStep 'rec Assets/Game/Scenes/P03_FriendDesktop.unity 22 0.5 Docs/Media/art_play_overhaul/capture_final EW.OverhaulReviewHooks.CaptureOnly' 'rec done frames' 60
Run-BridgeStep 'rec Assets/Game/Scenes/C01_TankLab.unity 36 0.5 Docs/Media/art_play_overhaul/tank_final EW.OverhaulReviewHooks.DesktopShot' 'rec done frames' 70
Run-BridgeStep 'rec Assets/Game/Scenes/P01_BurningVillage.unity 21 0.5 Docs/Media/art_play_overhaul/opening_final EW.OverhaulReviewHooks.DesktopShot' 'rec done frames' 60
Run-BridgeStep 'shot Assets/Game/Scenes/C01_TankLab.unity 4.5 Docs/Media/art_play_overhaul/after/lab.png EW.DevHooks.ShotC01Explore' 'shot done' 40
Run-BridgeStep 'shot Assets/Game/Scenes/C06_LongCorridor.unity 2.5 Docs/Media/art_play_overhaul/after/boss_hp.png EW.DevHooks.ShotC06' 'shot done' 40
Run-BridgeStep 'test playmode' 'TESTS FINISHED' 250
Run-BridgeStep 'test editmode' 'TESTS FINISHED' 60
Run-BridgeStep 'build' 'RESULT EW.EditorTools.EWMenu.BuildWindows' 240
Write-Output 'FINAL REVIEW COMPLETE'
