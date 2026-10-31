# Actionlint, pinned.
#
# The workflow linter the GitHub Actions ecosystem actually uses. It knows the
# expression grammar and which contexts are available where, which is knowledge no
# amount of reading gives you: `runner.temp` in a workflow level `env` and
# `steps` in a job level `if` are both plausible looking and both are errors that
# only surface when a workflow runs.
#
# Pinned by version and by checksum, because a linter that decides whether CI
# passes is exactly the kind of binary that must not float. Check the checksum
# file on its own with:
#
#   eng/scripts/actionlint.sh verify
#
# The checksums file is committed next to this script, and it holds checksums and
# nothing else: a prose header in it would be read as an archive name. A download
# that is not verified is a download that can hand the repository to somebody
# else, so the checksum is compared before anything is unpacked, and the digest of
# the unpacked binary is recorded and checked again on every later run. Verifying
# only the first download would leave every run after it executing whatever
# happened to be sitting in the cache.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

VERSION="1.7.12"
# Use only platform names and formats published by actionlint.
archive_name() {
  local platform arch extension
  case "$1" in
    Linux) platform=linux; extension=tar.gz ;;
    Darwin) platform=darwin; extension=tar.gz ;;
    MINGW* | MSYS* | CYGWIN* | Windows_NT) platform=windows; extension=zip ;;
    *) echo "actionlint: unsupported platform: $1" >&2; return 1 ;;
  esac
  case "$2" in
    x86_64 | amd64) arch=amd64 ;;
    aarch64 | arm64) arch=arm64 ;;
    *) echo "actionlint: unsupported architecture: $2" >&2; return 1 ;;
  esac
  printf 'actionlint_%s_%s_%s.%s\n' "${VERSION}" "${platform}" "${arch}" "${extension}"
}

ARCHIVE="$(archive_name "$(uname -s)" "$(uname -m)")"
URL="https://github.com/rhysd/actionlint/releases/download/v${VERSION}/${ARCHIVE}"
INSTALL_DIR="${XDG_CACHE_HOME:-$HOME/.cache}/neotales/actionlint/${VERSION}"
MEMBER=actionlint
case "${ARCHIVE}" in
  *.zip) MEMBER=actionlint.exe ;;
esac
BINARY="${INSTALL_DIR}/${MEMBER}"
DOWNLOAD="${INSTALL_DIR}/${ARCHIVE}"
DIGEST="${BINARY}.sha256"
CHECKSUMS="${SCRIPT_DIR}/actionlint-checksums.txt"

# Return the checksum for this platform's archive, or nothing. Match the archive
# name as text, not as a regular expression.
expected_checksum() {
  awk -v archive="${ARCHIVE}" '$2 == archive {print $1}' "${CHECKSUMS}"
}

# Whether a file matches a recorded digest. Used for the extracted binary, so the
# thing that is about to run is the thing that was verified, on the first run and
# on every run after it.
matches_digest() {
  [ -f "$1" ] && [ -f "$2" ] && [ "$(sha256sum "$1" | awk '{print $1}')" = "$(cat "$2")" ]
}

ensure() {
  # A binary that is already here is not trusted because it is already here. It is
  # trusted because its digest still matches the one written when the archive it
  # came from was verified, so a replaced or edited cache entry is re-fetched
  # rather than executed.
  if [ -x "${BINARY}" ] && matches_digest "${BINARY}" "${DIGEST}"; then
    return 0
  fi

  local expected
  expected="$(expected_checksum)"
  if [ -z "${expected}" ]; then
    echo "actionlint: no checksum recorded for ${ARCHIVE}, refusing to run an unverified download" >&2
    exit 1
  fi

  if ! [ -f "${DOWNLOAD}" ] || [ "$(sha256sum "${DOWNLOAD}" | awk '{print $1}')" != "${expected}" ]; then
    echo "actionlint: fetching ${VERSION}" >&2
    mkdir -p "${INSTALL_DIR}"
    curl -sSfL "${URL}" -o "${DOWNLOAD}"

    local actual
    actual="$(sha256sum "${DOWNLOAD}" | awk '{print $1}')"
    if [ "${expected}" != "${actual}" ]; then
      echo "actionlint: checksum mismatch for ${ARCHIVE}" >&2
      echo "  expected ${expected}" >&2
      echo "  actual   ${actual}" >&2
      rm -f "${DOWNLOAD}"
      exit 1
    fi
  fi

  # The member is named, so nothing else in the archive is written, and the
  # previous binary goes first so a failed extraction cannot leave the old one in
  # place looking like a new one.
  rm -f "${BINARY}"
  case "${ARCHIVE}" in
    *.zip) unzip -p "${DOWNLOAD}" "${MEMBER}" > "${BINARY}" ;;
    *.tar.gz) tar xzf "${DOWNLOAD}" -C "${INSTALL_DIR}" "${MEMBER}" ;;
  esac
  chmod +x "${BINARY}"
  sha256sum "${BINARY}" | awk '{print $1}' > "${DIGEST}"
}

# Every line of the checksum file is a checksum and an archive name, and nothing
# else. A prose header would be read as an archive, which is the sort of quiet
# failure this script exists to prevent, so the explanation lives here instead.
verify_checksums() {
  local total
  total="$(grep -c '' "${CHECKSUMS}" || true)"

  local covered
  covered="$(grep -cE '^[0-9a-f]{64}  ' "${CHECKSUMS}" || true)"
  if [ "${covered}" -ne "${total}" ]; then
    echo "actionlint: ${total} line(s) in the checksum file, ${covered} of them a checksum" >&2
    echo "  a checksum file holds a digest and an archive name per line, and nothing else" >&2
    exit 1
  fi

  local expected
  expected="$(expected_checksum)"
  if [ -z "${expected}" ]; then
    echo "actionlint: no checksum recorded for ${ARCHIVE}, so this platform cannot be verified" >&2
    exit 1
  fi

  if [ -f "${DOWNLOAD}" ]; then
    local actual
    actual="$(sha256sum "${DOWNLOAD}" | awk '{print $1}')"
    if [ "${expected}" != "${actual}" ]; then
      echo "actionlint: the downloaded ${ARCHIVE} does not match the recorded checksum" >&2
      rm -f "${DOWNLOAD}"
      exit 1
    fi
  fi

  echo "actionlint: ${total} archive(s) covered by checksum, ${ARCHIVE} included"
}

# Only `verify` and `path` are verbs. Everything else is a path for the linter,
# including a single one: shifting the first argument unconditionally meant that
# naming one workflow linted actionlint's own default directory instead, and
# reported success for a file it had never read.
verb="lint"
case "${1:-}" in
  verify | path)
    verb="$1"
    shift
    ;;
esac

case "${verb}" in
  verify)
    verify_checksums
    ;;
  path)
    ensure
    echo "${BINARY}"
    ;;
  *)
    ensure
    exec "${BINARY}" "$@"
    ;;
esac
