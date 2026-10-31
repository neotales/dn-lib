#!/usr/bin/env bash
# Wrapper. The pin rules live in eng/scripts/check-action-pins.cs.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../.."
exec dotnet ./eng/scripts/check-action-pins.cs "$@"
