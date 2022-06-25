$folders = Get-ChildItem . | ?{$_.PsIsContainer -eq $true}
foreach ($folder in $folders) {
    cd $folder
    npm publish --registry http://dragonscapes.diandian.info:4000/
    npm publish
    cd ..
}