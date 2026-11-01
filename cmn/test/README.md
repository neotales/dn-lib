# Shared-source tests

`cmn/src` contains source files compiled into consuming packages through explicit
`Compile` links; it is not a runtime library. Put tests for a shared implementation
in the consuming package's `test` project, where its actual public behavior and
compilation context can be exercised. This directory is reserved for future
shared-source-only test infrastructure and intentionally contains no invented
test project.
