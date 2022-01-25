#!/bin/sh

is_user_root () { [ "$(id -u)" -eq 0 ]; }
if is_user_root; then
	chmod +x ./Binner.service
	sed --expression "s#@INSTALLPATH@#${PWD}#g" Binner.service.systemctl.template > /etc/systemd/system/Binner.service
	systemctl daemon-reload
	systemctl enable Binner.service
	systemctl start Binner.service
	echo "Binner.Web is now running - http://localhost:8090"
	exit 0
else
	echo "This script must be run as root user using sudo.";
	exit 1
fi
