param(
    [string]$TargetPath = "bin\Release\net8.0-windows\Choas.exe",
    [string]$CertificatePath = $env:CHOAS_CODESIGN_CERT,
    [string]$CertificatePassword = $env:CHOAS_CODESIGN_PASSWORD,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $TargetPath)) {
    throw "Arquivo para assinatura nao encontrado: $TargetPath"
}

if ([string]::IsNullOrWhiteSpace($CertificatePath) -or -not (Test-Path -LiteralPath $CertificatePath)) {
    throw "Informe o certificado em -CertificatePath ou na variavel CHOAS_CODESIGN_CERT."
}

$signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if ($null -eq $signtool) {
    $kitsRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    if (Test-Path -LiteralPath $kitsRoot) {
        $signtool = Get-ChildItem -Path $kitsRoot -Recurse -Filter signtool.exe |
            Sort-Object FullName -Descending |
            Select-Object -First 1
    }
}

if ($null -eq $signtool) {
    throw "signtool.exe nao encontrado. Instale Windows SDK ou adicione signtool ao PATH."
}

$arguments = @(
    "sign",
    "/fd", "SHA256",
    "/f", $CertificatePath,
    "/tr", $TimestampUrl,
    "/td", "SHA256"
)

if (-not [string]::IsNullOrWhiteSpace($CertificatePassword)) {
    $arguments += @("/p", $CertificatePassword)
}

$arguments += $TargetPath

& $signtool.Source @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao assinar $TargetPath"
}

& $signtool.Source verify /pa /v $TargetPath
if ($LASTEXITCODE -ne 0) {
    throw "A verificacao da assinatura falhou para $TargetPath"
}

Write-Host "Assinatura Authenticode aplicada e verificada em $TargetPath"
