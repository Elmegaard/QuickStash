# Vars
$pluginName = "quick_stash"
$projectFolder = "$PSScriptRoot/vrising_stash"
$msBuildLocation = "C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe"
$7zLocation = "C:\Program Files\7-Zip\7z.exe"

$dllName = "$pluginName.dll"
$outputDir = "$PSScriptRoot/build"
$zipFolder = "$PSScriptRoot/zip"

# Start
Push-Location $PSScriptRoot

# Compile
Push-Location $projectFolder
&$msBuildLocation /m /p:Configuration=Release /t:Rebuild /p:OutDir="$outputDir"
Pop-Location

# Update manifest version
$pluginVersion = (Get-Item "$outputDir/$dllName").VersionInfo.ProductVersion
$manifest = Get-Content "$PSScriptRoot/manifest.json" | ConvertFrom-Json
$manifest.version_number = $pluginVersion
$manifest | ConvertTo-Json | Set-Content "$PSScriptRoot/manifest.json"

# Make zip folder
New-Item $zipFolder -ItemType Directory -Force
Copy-Item -Path "$outputDir/$dllName" -Destination "$zipFolder/$dllName" -Force
Copy-Item -Path "$PSScriptRoot/icon.png" -Destination "$zipFolder/" -Force
Copy-Item -Path "$PSScriptRoot/manifest.json" -Destination "$zipFolder/" -Force
Copy-Item -Path "$PSScriptRoot/README.md" -Destination "$zipFolder/" -Force

# Create zip file
$zipLocation = "$PSScriptRoot/$($pluginName)_$pluginVersion.zip"
Remove-Item -Path "$zipLocation*" -Force -Recurse
& $7zLocation a $zipLocation "$zipFolder/*"

# Cleanup
Remove-Item -Path "$outputDir" -Force -Recurse
Remove-Item -Path "$zipFolder" -Force -Recurse

Pop-Location