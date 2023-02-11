#!/bin/sh

is_user_root () { [ "$(id -u)" -eq 0 ]; }
if is_user_root; then
	systemctl stop Binner.service
	systemctl disable Binner.service
	rm /etc/systemd/system/Binner.service
	systemctl daemon-reload
	systemctl reset-failed

	echo "Binner service is uninstalled!"
	exit 0
else
	echo "This script must be run as root user using sudo.";
	exit 1
fi
