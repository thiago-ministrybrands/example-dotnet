#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."

dotnet format example-dotnet.slnx --verify-no-changes
dotnet build example-dotnet.slnx /warnaserror
