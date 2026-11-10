# dn-lib

.NET libraries for Neotales. This directory is the root of `github.com/neotales/dn-lib`. In the plab monorepo it is `dn/lib`, and that prefix is removed when the directory is published. A path here is the same path there.

`lib.slnx` is the solution. Restore and build it with the .NET SDK. Do not add a second solution at this root for the same projects.

`dev` is the integration branch. Do not create `main` or `master`.

This directory is published from plab with Copybara. The satellite is a copy, not the place a change starts. Make the change under `dn/lib` in plab, then export the commits that touched that path. Do not force-push `neotales/dn-lib` to replace that history.

Folders that configure this repository belong here. `.agents`, `.vscode`, and `.github` are exported with the prefix. Do not put them at the plab root to configure this library.

No credentials, tokens, or key bodies in this tree. A local env file is gitignored. A committed example may be named `.env.example` and must not contain a real secret.
