# SSL Certificates

Binner ships with a self-signed certificate intended to be used for HTTPS/SSL.

## Specifying the certificate to load

In `appsettings.json` set the following properties in the `WebHostServiceConfiguration` section at the top of the file:

```json
{
  "WebHostServiceConfiguration": {
    "SslCertificate": "./certificates/localhost.pfx",
    "SslCertificatePassword": "password",
  }
}
```

## Generating your own certificate

If you would like to specify your own certificate instead of using the default one provided, run the following script(s) appropriate to your platform.

### Windows

The following powershell script can be used to generate a new SSL certificate, or run `.\CreateCert.ps1` in an elevated (Adminstrator) shell:

```ps
$cert = New-SelfSignedCertificate -DnsName @("localhost") -Subject "localhost" -FriendlyName "Binner HTTPS Certificate" -NotBefore (Get-Date) -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm RSA -KeyLength 4096 -HashAlgorithm SHA256 -CertStoreLocation "cert:\LocalMachine\My"

$certKeyPath = "./localhost-windows.pfx"
$password = ConvertTo-SecureString 'password' -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $certKeyPath -Password $password
$(Import-PfxCertificate -FilePath $certKeyPath -CertStoreLocation 'Cert:\LocalMachine\Root' -Password $password)
```
_[via microsoft](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide)_

### Unix

Create a config file named `localhost-unix.conf` for your certificate:

```conf
[req]
default_bits       = 4096
default_keyfile    = localhost-unix.key
distinguished_name = req_distinguished_name
req_extensions     = req_ext
x509_extensions    = v3_ca

[req_distinguished_name]
countryName                 = US
countryName_default         = US
stateOrProvinceName         = NY
stateOrProvinceName_default = New York
localityName                = New York
localityName_default        = Rochester
organizationName            = Binner
organizationName_default    = localhost
organizationalUnitName      = organizationalunit
organizationalUnitName_default = Development
commonName                  = localhost
commonName_default          = localhost
commonName_max              = 64

[req_ext]
subjectAltName = @alt_names

[v3_ca]
subjectAltName = @alt_names

[alt_names]
DNS.1   = localhost
DNS.2   = 127.0.0.1
```

Run the following commands using openssl to create a self-signed certificate:
```sh
sudo openssl req -x509 -nodes -newkey rsa:4096 -keyout localhost-unix.key -out localhost-unix.crt -sha256 -config localhost-unix.conf -passin "pass:password" -days 3650
```

*Note:* _When running the export command above, be sure to enter your password when it prompts for an export password._
```sh
sudo openssl pkcs12 -export -inkey localhost-unix.key -in localhost-unix.crt -out localhost-unix.pfx
```

To test if the certificate password is correct, run the following and enter the password `password` when asked:
```sh
sudo openssl rsa -noout -in localhost-unix.pfx
```

If successful, no error will be displayed. If it's unsuccessful you will see an error message such as:
```
Could not read private key from localhost-unix.pfx
40770C82157F0000:error:16000071:STORE routines:try_pkcs12:error verifying pkcs12 mac:../crypto/store/store_result.c:584:empty password
```

Install the certutil utility:
```sh
sudo apt-get update
sudo apt-get install libnss3-tools
```

Run the following command to add the certificate to your trusted CA root store:
```sh
certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n "localhost" -i localhost-unix.crt
```

Confirm the certificate was added as a trusted certificate:
```sh
certutil -L -d sql:${HOME}/.pki/nssdb
```

To delete the certificate:
```sh
certutil -D -d sql:${HOME}/.pki/nssdb -n "localhost"
```

_[via stackoverflow.com](https://stackoverflow.com/questions/10175812/how-to-generate-a-self-signed-ssl-certificate-using-openssl)_
_[via humankode.com](https://www.humankode.com/asp-net-core/develop-locally-with-https-self-signed-certificates-and-asp-net-core/)_

### Mac OSX

```sh
sudo nano localhost.conf
```

Paste the contents into the `localhost.conf` file and save the file:

```conf
[req]
default_bits       = 4096
default_keyfile    = localhost.key
distinguished_name = req_distinguished_name
req_extensions     = req_ext
x509_extensions    = v3_ca

[req_distinguished_name]
countryName                 = US
countryName_default         = US
stateOrProvinceName         = NY
stateOrProvinceName_default = New York
localityName                = New York
localityName_default        = Rochester
organizationName            = Binner
organizationName_default    = localhost
organizationalUnitName      = organizationalunit
organizationalUnitName_default = Development
commonName                  = localhost
commonName_default          = localhost
commonName_max              = 64

[req_ext]
subjectAltName = @alt_names

[v3_ca]
subjectAltName = @alt_names

[alt_names]
DNS.1   = localhost
DNS.2   = 127.0.0.1
```

Run the following command to check if OpenSSL is installed:
```sh
which openssl
```

If OpenSSL is not installed, install OpenSSL with homewbrew:
```sh
brew install openssl
```

Run the following 2 commands using OpenSSL to create a self-signed certificate in Mac OSX with OpenSSL:
```sh
sudo openssl req -x509 -nodes -newkey rsa:4096 -keyout localhost.key -out localhost.crt -config localhost.conf -passin pass:password -days 3650
```

```sh
sudo openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt
```

To trust the self-signed certificate:

- Open the KeyChain Access app (do a spotlight search for KeyChain to find it).
- Select System in the Keychains pane, and drag your localhost.pfx certificate into the certificate list pane.
- To trust your self-signed certificate, double-click your certificate, and under the trust section select Always Trust.

_[via humankode.com](https://www.humankode.com/asp-net-core/develop-locally-with-https-self-signed-certificates-and-asp-net-core/)_
