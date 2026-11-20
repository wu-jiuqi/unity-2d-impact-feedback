param([ValidateSet(30,60)][int]$Fps=60,[int]$Width=1600,[int]$Height=900)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskOut=Join-Path $taskRoot "evidence/qa$Fps"
New-Item -ItemType Directory -Path $taskOut -Force | Out-Null
$taskExe=Join-Path $taskRoot 'Builds/Windows/ImpactFeedbackGallery.exe'
$taskArgs=@('--gallery-qa','--qa-fps',"$Fps",'--qa-out',('"'+$taskOut+'"'),'-screen-width',"$Width",'-screen-height',"$Height",'-logFile',('"'+(Join-Path $taskOut 'player.log')+'"'))
$taskProcess=Start-Process -FilePath $taskExe -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
if(-not $taskProcess.WaitForExit(90000)){Stop-Process -Id $taskProcess.Id;throw "Gallery QA timed out; inspect $taskOut/player.log"}
if($taskProcess.ExitCode -ne 0){throw "Gallery QA failed: $($taskProcess.ExitCode); inspect $taskOut"}
$taskResult=Get-Content (Join-Path $taskOut 'result.json') -Raw | ConvertFrom-Json
if($taskResult.failed -ne 0){throw 'Gallery QA reported failures'}
if(Select-String -LiteralPath (Join-Path $taskOut 'player.log') -Pattern 'Exception:|\bError\b' -Quiet){throw 'Runtime log contains errors; inspect player.log'}
Write-Output "QA $Fps fps: $($taskResult.passed) passed, $($taskResult.failed) failed"
