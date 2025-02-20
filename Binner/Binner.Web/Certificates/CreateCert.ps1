$cert = New-SelfSignedCertificate -DnsName @("localhost") -Subject "localhost" -FriendlyName "Binner HTTPS Certificate" -NotBefore (Get-Date) -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm RSA -KeyLength 4096 -HashAlgorithm SHA256 -CertStoreLocation "cert:\LocalMachine\My"

$certKeyPath = "./localhost.pfx"
$password = ConvertTo-SecureString 'password' -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $certKeyPath -Password $password
$(Import-PfxCertificate -FilePath $certKeyPath -CertStoreLocation 'Cert:\LocalMachine\Root' -Password $password)
