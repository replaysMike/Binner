#!/bin/sh

RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

is_user_root () { [ "$(id -u)" -eq 0 ]; }
if is_user_root; then
	echo "This script should not be run as root user using sudo.";
	exit 1
else
  if command -v certutil 2>&1 >/dev/null
  then
    echo -n "Registering trusted certificate in certificate store..."
    if certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n "localhost" -i ./Certificates/localhost-unix.crt;
    then
      echo " ${WHITE}[${GREEN}OK${WHITE}]${NC}\n"
    else
      echo " ${WHITE}[${RED}FAILED${WHITE}]${NC}\n"
    fi

    echo "More details on working with or replacing certificates are available in '${WHITE}Certificates/README.md${NC}'\n"
    exit 0
  else
    echo "${CYAN}certutil${NC} is not installed. Install it using '${GREEN}sudo apt-get install libnss3-tools${NC}'\n"
    exit 1
  fi
fi
