param([string]$WorkDirectory=(Join-Path $env:TEMP ('Litfix-package-'+[Guid]::NewGuid().ToString('N'))))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null
$app=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'PKA-Proxy.exe'))
# The interface does not contain the core; copying it as the background engine caused startup failure.
if($app.GetManifestResourceNames() -contains 'PKA.core.gz'){throw 'Unexpected package layout; review background regression.'}
$resource=$app.GetManifestResourceStream('PKAengine.exe')
if($null -eq $resource){throw 'Missing background engine.'}
$path=Join-Path $WorkDirectory 'Litfix-Discord.exe'
try{$target=[IO.File]::Create($path);try{$resource.CopyTo($target)}finally{$target.Dispose()}}finally{$resource.Dispose()}
if((Get-FileHash $path).Hash -ne (Get-FileHash (Join-Path $PSScriptRoot 'PKAengine.exe')).Hash){throw 'Packaged engine mismatch.'}
$engine=[Reflection.Assembly]::LoadFile($path)
foreach($name in @('PKA.core.gz','PKArouteProbe.exe')){if($engine.GetManifestResourceNames() -notcontains $name){throw ('Missing engine resource: '+$name)}}
Write-Output 'PASS: permanent background package contains core and probe; interface alone cannot run the background engine. No tasks installed.'
