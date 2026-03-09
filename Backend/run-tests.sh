#!/bin/bash
# Test runner script that sets the necessary environment variable to avoid inotify limit issues

export DOTNET_USE_POLLING_FILE_WATCHER=true
dotnet test "$@"
