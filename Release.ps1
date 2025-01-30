# powershell

# Определяем переменные
$folder = "PackFolder"
$mods = "PackFolder\Mods"
$maps = "PackFolder\UserData\ManiaMaps"
$userData = "PackFolder\UserData"
$userLibs = "PackFolder\UserLibs"
$mod = ".\bin\Debug\net6.0\MiSideMania.dll"
$osu = ".\libs\OsuParsers.dll"
$audio = ".\libs\AudioImportLib.dll"
$archive = "Release.zip"
$toArchive = @($mods, $userLibs, $userData)

if (Test-Path -Path $archive) {
    Remove-Item -Path $archive -Force
}

New-Item -ItemType Directory -Path $folder
New-Item -ItemType Directory -Path $mods
New-Item -ItemType Directory -Path $userLibs
New-Item -ItemType Directory -Path $maps

Copy-Item -Path $mod -Destination $mods
Copy-Item -Path $osu -Destination $userLibs
Copy-Item -Path $audio -Destination $mods

Compress-Archive -Path $toArchive -DestinationPath $archive

Remove-Item -Path $folder -Recurse -Force
