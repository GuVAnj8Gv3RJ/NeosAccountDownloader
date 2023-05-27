$ProgressPreference = 'SilentlyContinue'

Invoke-WebRequest -Uri "https://assets.neos.com/install/Pro/Data/2022.1.28.1310_YTDLP.7z" -OutFile Neos.7z
7z x -y Neos.7z -oNeos
del Neos.7z
mv Neos/Neos_Data/Managed/BaseX.dll ./
mv Neos/Neos_Data/Managed/CloudX.Shared.dll ./
mv Neos/Neos_Data/Managed/CodeX.dll ./
rm -r ./Neos
