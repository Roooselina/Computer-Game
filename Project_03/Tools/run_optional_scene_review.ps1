$ErrorActionPreference='Stop'
$bridgeDir=Join-Path (Get-Location) 'Temp/EWBridge'
$reviewLog=Join-Path (Get-Location) 'Docs/Media/art_play_overhaul/optional_scene_verification.log'
function Run-BridgeStep([string]$command,[string]$done,[int]$timeout=180) {
    $resultPath=Join-Path $bridgeDir 'result.txt';$before=[IO.File]::ReadAllText($resultPath).Length;$started=Get-Date
    Set-Content -LiteralPath (Join-Path $bridgeDir 'cmd.txt') -Value $command
    Add-Content -LiteralPath $reviewLog -Value ('START '+$started.ToString('HH:mm:ss')+' '+$command)
    while(((Get-Date)-$started).TotalSeconds -lt $timeout) {
        Start-Sleep -Milliseconds 500;$all=[IO.File]::ReadAllText($resultPath);$new=$all.Substring([Math]::Min($before,$all.Length))
        if($new -match 'COMPILE ERRORS|ERROR System\.') {Add-Content -LiteralPath $reviewLog -Value $new;throw 'Unity bridge error'}
        if($new -match $done) {Add-Content -LiteralPath $reviewLog -Value $new;if($new -match 'TESTS FINISHED pass=\d+ fail=([1-9]\d*)'){throw 'Tests failed'};Write-Output ('DONE '+$command);return}
    }
    throw ('TIMEOUT '+$command)
}
Run-BridgeStep 'shot Assets/Game/Scenes/M09_StaffCafeteria.unity 3 Docs/Media/art_play_overhaul/after/cafeteria.png EW.DevHooks.ShotMapAt' 'shot done' 35
Run-BridgeStep 'shot Assets/Game/Scenes/M13_MixingRoom.unity 3 Docs/Media/art_play_overhaul/after/mixing.png EW.DevHooks.ShotMapAt' 'shot done' 35
Run-BridgeStep 'shot Assets/Game/Scenes/M14_ChemGreenhouse.unity 3 Docs/Media/art_play_overhaul/after/greenhouse.png EW.DevHooks.ShotMapAt' 'shot done' 35
Run-BridgeStep 'call EW.EditorTools.EWMenu.OpenTitle' 'RESULT EW.EditorTools.EWMenu.OpenTitle' 30
