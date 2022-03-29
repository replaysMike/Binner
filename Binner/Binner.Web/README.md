# Binner.Web

## HTTPS Certificate Generation

To generate an SSL certificate:

```ps

$Path = "C:\BinnerCert"
$FilenameWithoutExtension = "Binner"
$IssuerStoreLocation = "LocalMachine"
$FriendlyName = "Binner Server HTTPS Certificate"
$Password = "password"

# create certificate
New-SelfSignedCertificate -CertStoreLocation "cert:\$IssuerStoreLocation\My" -DnsName localhost,binner.local,binner -KeyUsage DigitalSignature,KeyEncipherment -KeyAlgorithm RSA -KeyLength 2048 -NotAfter (Get-Date).AddYears(20) -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") -FriendlyName $FriendlyName

# export to pfx
$mypwd = ConvertTo-SecureString -String $Password -Force -AsPlainText
$Thumbprint = Get-ChildItem cert:\$IssuerStoreLocation\My | Where-Object {$_.FriendlyName -eq $FriendlyName} | Select-Object -ExpandProperty Thumbprint
Get-ChildItem -Path cert:\$IssuerStoreLocation\My\"$Thumbprint" | Export-PfxCertificate -FilePath "$Path\$FilenameWithoutExtension.pfx" -Password $mypwd

# import to trusted root
Import-PfxCertificate -FilePath "$Path\$FilenameWithoutExtension.pfx" -CertStoreLocation cert:\$IssuerStoreLocation\Root -Password $mypwd

```