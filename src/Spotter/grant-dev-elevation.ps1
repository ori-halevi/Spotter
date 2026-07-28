<#
.SYNOPSIS
    Registers an on-demand scheduled task that launches the Spotter build elevated,
    so development iterations do not need a UAC click each time.

.DESCRIPTION
    Deliberately narrow. The task's action is a FIXED path to Spotter.exe - it cannot run
    anything else. The caller-supplied parameter goes to that app's own flags (--search, --bench,
    --demo), none of which execute anything.

    That distinction matters: a task that ran arbitrary commands elevated would be a permanent
    local privilege-escalation backdoor, which is exactly the weakness flagged in the 1.5.1 app's
    own UAC-bypass task.

    THE TRADE-OFF, STATED PLAINLY: this still creates a permanent elevated entry point for one
    executable. That executable lives under your user profile, which you - and therefore anything
    running as you - can overwrite. On a personal dev machine that is usually an acceptable trade;
    it is not free. Remove it whenever you like:

        Unregister-ScheduledTask -TaskName 'Spotter.DevElevation' -Confirm:$false

.NOTES
    Run from an elevated PowerShell prompt.
#>

$ErrorActionPreference = 'Stop'

$TaskName = 'Spotter.DevElevation'
# Relative to this script, so moving the project does not silently leave the task pointing at a
# path that no longer exists.
$Exe = Join-Path $PSScriptRoot 'bin\Release\net8.0-windows\Spotter.exe'

$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host 'This must run from an ELEVATED PowerShell prompt.' -ForegroundColor Red
    Write-Host 'Right-click PowerShell -> Run as administrator, then run it again.'
    exit 1
}

if (-not (Test-Path $Exe)) {
    Write-Host "Executable not found:`n  $Exe" -ForegroundColor Red
    Write-Host 'Build it first:  dotnet build -c Release  (in src\Wpf)'
    exit 1
}

# $(Arg0) is substituted with whatever the caller passes to .Run(). The action path itself is
# fixed, so this can only ever start Spotter - never another program.
$action = New-ScheduledTaskAction -Execute $Exe -Argument '$(Arg0)' `
                                  -WorkingDirectory (Split-Path $Exe -Parent)

$principalDef = New-ScheduledTaskPrincipal -UserId $identity.Name `
                                           -LogonType Interactive `
                                           -RunLevel Highest

$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries `
                                         -DontStopIfGoingOnBatteries `
                                         -ExecutionTimeLimit ([TimeSpan]::Zero) `
                                         -MultipleInstances IgnoreNew

Register-ScheduledTask -TaskName $TaskName `
                       -Action $action `
                       -Principal $principalDef `
                       -Settings $settings `
                       -Description 'On-demand elevated launch of the Spotter dev build.' `
                       -Force | Out-Null

Write-Host ''
Write-Host "Registered '$TaskName'." -ForegroundColor Green
Write-Host "  action : $Exe `$(Arg0)"
Write-Host '  runs as: Highest (elevated), interactive token, on demand only'
Write-Host ''
Write-Host 'Trigger it (no elevation needed by the caller):' -ForegroundColor Cyan
Write-Host '  $s = New-Object -ComObject Schedule.Service; $s.Connect()'
Write-Host "  `$s.GetFolder('\').GetTask('$TaskName').Run('--search notepad --bench 4')"
Write-Host ''
Write-Host 'Remove it at any time:' -ForegroundColor Cyan
Write-Host "  Unregister-ScheduledTask -TaskName '$TaskName' -Confirm:`$false"
Write-Host ''
Write-Host 'Note: this is a standing elevated entry point for that one exe, and the exe sits in' -ForegroundColor Yellow
Write-Host 'a folder you can write to. Anything running as you could replace it.' -ForegroundColor Yellow
