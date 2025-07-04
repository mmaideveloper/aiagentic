Write-Output "ASPNETCORE_ENVIRONMENT: $env:ASPNETCORE_ENVIRONMENT"

if ($env:ASPNETCORE_ENVIRONMENT -eq "production") {
    Write-Output "Running npm run build"
    npm run build
} else {
    Write-Output "Running npm run build"
    npm run build
}
