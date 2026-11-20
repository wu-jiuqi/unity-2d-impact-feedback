param([string]$Unity='D:/Unity/Editor/Unity.exe')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskProject=Join-Path $taskRoot 'assets/unity-sample'
$taskEvidence=Join-Path $taskRoot 'evidence'
New-Item -ItemType Directory -Path $taskEvidence -Force | Out-Null
$taskLog=Join-Path $taskEvidence 'build.log'
$taskArgs=@('-batchmode','-nographics','-quit','-projectPath',('"'+$taskProject+'"'),'-buildTarget','Win64','-executeMethod','GalleryBuild.Windows','-logFile',('"'+$taskLog+'"'))
$taskProcess=Start-Process -FilePath $Unity -ArgumentList $taskArgs -WindowStyle Hidden -PassThru -Wait
if($taskProcess.ExitCode -ne 0){throw "Unity failed: $($taskProcess.ExitCode). See $taskLog"}
Write-Output "Build complete: $taskRoot/Builds/Windows/ImpactFeedbackGallery.exe"
