<#
.SYNOPSIS
    Registers SSL certificates required for the Binner web server
.DESCRIPTION
	Registers SSL certificates required for the Binner web server with the trusted root store for Windows.
    REQUIRES: ADMINISTRATOR ELEVATED console
.EXAMPLE
    C:\PS> .\install-windows-certificate.ps1
    Registers the SSL certificate with LocalMachine\Root
#>
#Requires -RunAsAdministrator

Import-PfxCertificate -FilePath "..\Binner.Web\Certificates\Binner.pfx" -CertStoreLocation cert:\LocalMachine\Root -Password (ConvertTo-SecureString -String password -Force -AsPlainText)