# Core source provenance

The package and test sources in `std/Core/src` and `std/Core/test` were imported from
`std/core/Core/src` and `std/core/Core/test` at source commit
`5fb66e0b641d2cc2c11abef1ef28fb443b9e1ad7`. The source-only shared files in
`cmn/src` were imported from `std/core/Common/src` at that same commit. The only
project-file source adjustment is the seven shared-source links rewritten to the
`cmn/src` location; package and test behavior were otherwise left intact.
