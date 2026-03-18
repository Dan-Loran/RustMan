[CmdletBinding()]
param(
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($scriptRoot)) {
    throw 'Unable to determine installer script directory.'
}

$repoRoot = (Resolve-Path (Join-Path $scriptRoot '..')).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot 'artifacts\installer'
}
$outputRoot = (Resolve-Path -LiteralPath $OutputDirectory -ErrorAction SilentlyContinue)
if ($null -eq $outputRoot) {
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    $outputRoot = (Resolve-Path -LiteralPath $OutputDirectory).Path
}
else {
    $outputRoot = $outputRoot.Path
}

$stageRoot = Join-Path $outputRoot 'stage'
$packageRoot = Join-Path $stageRoot 'package'
$appOutput = Join-Path $packageRoot 'app'
$installerOutput = Join-Path $packageRoot 'installer'
$dbOutput = Join-Path $packageRoot 'db'
$tarPath = Join-Path $outputRoot 'rustman-installer.tar'
$archivePath = Join-Path $outputRoot 'rustman-installer.tar.gz'

if (Test-Path $stageRoot) {
    Remove-Item -Recurse -Force $stageRoot
}

New-Item -ItemType Directory -Force -Path $appOutput, $installerOutput, $dbOutput | Out-Null

dotnet publish `
    (Join-Path $repoRoot 'RustMan.Web\RustMan.Web.csproj') `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    /p:UseAppHost=true `
    /p:PublishSingleFile=false `
    -o $appOutput

Copy-Item (Join-Path $repoRoot 'installer\install.sh') $installerOutput
Copy-Item (Join-Path $repoRoot 'installer\bootstrap.sh') $installerOutput
Copy-Item (Join-Path $repoRoot 'installer\rustman.service') $installerOutput
Copy-Item (Join-Path $repoRoot 'db\*.sql') $dbOutput

if (Test-Path $archivePath) {
    Remove-Item -Force $archivePath
}

if (Test-Path $tarPath) {
    Remove-Item -Force $tarPath
}

tar -cf $tarPath -C $packageRoot .

$tarStream = [System.IO.File]::OpenRead($tarPath)
try {
    $gzipStream = [System.IO.File]::Create($archivePath)
    try {
        $compressor = New-Object System.IO.Compression.GzipStream(
            $gzipStream,
            [System.IO.Compression.CompressionLevel]::Optimal
        )
        try {
            $tarStream.CopyTo($compressor)
        }
        finally {
            $compressor.Dispose()
        }
    }
    finally {
        $gzipStream.Dispose()
    }
}
finally {
    $tarStream.Dispose()
}

Remove-Item -Force $tarPath

Write-Host "Created $archivePath"
