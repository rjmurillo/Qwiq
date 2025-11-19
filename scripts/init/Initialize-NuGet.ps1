Param(
    [Parameter(Mandatory=$true)] [string] $repoRoot
)

$ErrorActionPreference = "Stop"

# Create the .tools directory
New-Item -ItemType Directory -Force -Path "$repoRoot\.tools" | Out-Null
$toolsDir = Join-Path -Resolve $repoRoot ".tools"

# Ensure nuget.exe is up-to-date
$nugetDownloadName = "nuget.exe"
. "$PSScriptRoot\Initialize-DownloadLatest.ps1" -OutDir $toolsDir -DownloadUrl "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -DownloadName $nugetDownloadName -Unzip $false

# Ensure Azure Artifacts Credential Provider is up-to-date
# This replaces the deprecated Microsoft.VisualStudio.Services.NuGet.CredentialProvider
try {
    $credProviderZipUrl = 'https://github.com/microsoft/artifacts-credprovider/releases/latest/download/Microsoft.NuGet.CredentialProvider.zip'
    $credProviderDir = Join-Path $toolsDir 'CredentialProviders'
    $credProviderZipPath = Join-Path $env:TEMP 'Microsoft.NuGet.CredentialProvider.zip'
    
    Write-Host "Downloading Azure Artifacts Credential Provider from $credProviderZipUrl"
    
    # Download the credential provider ZIP
    Invoke-WebRequest -Uri $credProviderZipUrl -OutFile $credProviderZipPath -UseBasicParsing
    
    # Extract to the credential providers directory
    if (Test-Path $credProviderDir) {
        Remove-Item $credProviderDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $credProviderDir | Out-Null
    Expand-Archive -Path $credProviderZipPath -DestinationPath $credProviderDir -Force
    
    # Set the environment variable for NuGet to discover the credential provider
    $env:NUGET_CREDENTIALPROVIDERS_PATH = $credProviderDir
    
    # For GitHub Actions, persist the environment variable to subsequent steps
    if ($env:GITHUB_ENV) {
        "NUGET_CREDENTIALPROVIDERS_PATH=$credProviderDir" | Out-File -FilePath $env:GITHUB_ENV -Append -Encoding utf8
    }
    
    # Clean up the downloaded ZIP
    Remove-Item $credProviderZipPath -Force -ErrorAction SilentlyContinue
    
    Write-Host "Azure Artifacts Credential Provider installed successfully to $credProviderDir"
}
catch {
    Write-Warning "Failed to install Azure Artifacts Credential Provider: $_"
    Write-Warning "Continuing without credential provider - private feeds may not be accessible"
}

# Add the tools dir to the path which directly contains NuGet.exe
if (!($env:Path -like "*$toolsDir;*"))
{
    $env:Path = "$toolsDir;" + $env:Path
}
