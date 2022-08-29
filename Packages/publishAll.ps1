$folders = Get-ChildItem . | ?{$_.PsIsContainer -eq $true}
foreach ($folder in $folders) {
    cd $folder
    npm publish
    cd ..
}