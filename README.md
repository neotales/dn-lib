# dn-lib

.NET libraries for Neotales.

This tree is the root of [neotales/dn-lib](https://github.com/neotales/dn-lib).
In the plab monorepo it is the `dn/lib` directory, and that prefix is removed
when the directory is published. A path here is the same path there.

`lib.slnx` is the solution. `cmn/src` is shared source compiled into a package
through explicit links. It is not a package. `std/Core` is `Neotales.Core`.

`eng/tag.cs` creates the dated tag. No suffix is a release. `-s alpha` or
`-s beta` is a prerelease. The revision comes from the tags already on
`neotales/dn-lib`. `--push` creates the tag there, which starts `dn-core-cd`.
