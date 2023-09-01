
[CmdletBinding()]
param(
  [Parameter(Mandatory=$false)]
  [string]$Path = "./",
  [Parameter(Mandatory=$false)]
  [int]$WaitInSeconds = 10,
  [Parameter(Mandatory=$true)]
  [ValidateNotNullOrEmpty()]
  [string]$NewPath
)

if (-not (Test-Path $NewPath))
{
  Write-Host "Destination folder '$NewPath' does not exist." -ForegroundColor Yellow
  exit 1
}

if (-not (Test-Path $Path))
{
  Write-Host "Source folder '$Path' does not exist." -ForegroundColor Yellow
  exit 1
}

Write-Host "Start listening for the files to move. Destination folder '$NewPath'" -ForegroundColor Green

while($true)
{
  Write-Host "Waiting $WaitInSeconds seconds until the next iteration..." -ForegroundColor Green
  Start-Sleep -Seconds $WaitInSeconds
  $allFiles = Get-ChildItem -Path $Path -File

  foreach ($file in $allFiles)
  {
    try {
      Move-Item -Path $file.FullName -Destination $NewPath -Force -ErrorAction Stop
      Write-Host "File '$($file.FullName)' has been moved to '$NewPath'." -ForegroundColor Green
    } catch {
      Write-Host "Unable to move file '$($file.FullName)' to a new destination '$NewPath'. Error: '$_'" -ForegroundColor Yellow
    }
  }
}