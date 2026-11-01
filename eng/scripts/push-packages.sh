#!/usr/bin/env bash
# Wrapper. The push rules live in eng/scripts/push-packages.cs.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/../.."
exec dotnet ./eng/scripts/push-packages.cs "$@"
