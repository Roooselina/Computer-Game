$ErrorActionPreference='Stop'
$bridgeDir=Join-Path (Get-Location) 'Temp/EWBridge'
$reviewLog=Join-Path (Get-Location) 'Docs/Media/art_play_overhaul/final_tests_build.log'
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
        if($new -match $done) {
            Add-Content -LiteralPath $reviewLog -Value $new
            if($new -match 'TESTS FINISHED pass=\d+ fail=([1-9]\d*)'){throw 'Tests failed'}
            Write-Output ('DONE '+$command);return
        }
    }
    throw ('TIMEOUT '+$command)
}
Run-BridgeStep 'refresh' 'refreshed' 40
Start-Sleep -Seconds 8
Run-BridgeStep 'test playmode' 'TESTS FINISHED' 240
Run-BridgeStep 'test editmode' 'TESTS FINISHED' 60
Run-BridgeStep 'build' 'RESULT EW.EditorTools.EWMenu.BuildWindows' 240
Write-Output 'FINAL TESTS AND BUILD COMPLETE'
