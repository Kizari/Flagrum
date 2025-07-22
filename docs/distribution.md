# Distribution Workflow

Flagrum is distributed from the [Kizari/Flagrum](https://github.com/Kizari/Flagrum) GitHub repository using
an automated workflow with a manual trigger.

## Versioning

Flagrum uses a typical 3-part version code, in the format `A.B.C`.

| Part | Description                                                                  |
|------|------------------------------------------------------------------------------|
| A    | Incremented when a full rewrite of the software is performed.                |
| B    | Incremented when a major refactor is performed, or a major feature is added. |
| C    | Incremented when any non-major updates are performed.                        |

## Preparing a New Release

When all commits for a new update have been committed, a new final commit should be prepared as follows:

1. `<BaseVersion>` updated appropriately in `Flagrum.csproj`.
2. New changelog fragment added to `docs/changelog` for this version.

This should then be pushed to source control.

## Distribution

1. Manually trigger the workflow from [GitHub actions](https://github.com/Kizari/Flagrum/actions).
   This creates a draft in [Kizari/Flagrum/releases](https://github.com/Kizari/Flagrum/releases).
2. Ensure the draft looks correct, then release it.
3. Ensure that the locally installed copy of Flagrum correctly updates to the new version when launched.
4. Remove the previous version of Flagrum from the releases page.