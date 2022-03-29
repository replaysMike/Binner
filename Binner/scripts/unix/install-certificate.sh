#!/bin/sh

is_user_root () { [ "$(id -u)" -eq 0 ]; }
if is_user_root; then
	cp ./Certificates/Binner.crt /usr/local/share/ca-certificates/Binner.crt
	update-ca-certificates
	echo "Certificate was installed to trusted root at /usr/local/share/ca-certificates/Binner.crt"
	exit 0
else
	echo "This script must be run as root user using sudo.";
	exit 1
fi
