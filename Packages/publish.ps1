
foreach ($arg in $args) {
    Set-Location $arg
    npm publish
    Set-Location ..
}