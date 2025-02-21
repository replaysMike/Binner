#!/bin/sh

if [ -n "$1" ]; then
    exec "$@"
fi
exec /bin/bash -l
