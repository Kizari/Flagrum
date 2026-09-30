# Data Migrations

These migrations are here to update Flagrum's persisted data from older versions of Flagrum to the current formats.
This has been done as there were poor design choices made in the past that we needed to move past to ensure we weren't
getting bogged down in technical debt.


## Authoring a Migration

Migrations are created by annotating a partial class with `[SteppedDataMigration(x)]`.  
Migrations are executed in the order passed to the constructor of `SteppedDataMigrationAttribute`.

When Flagrum runs the migration, it will run each method within the class that is annotated with `[MigrationStep]`.

`MigrationStep` takes the following constructor parameters:

| Parameter | Effect                                                                                           |
|-----------|--------------------------------------------------------------------------------------------------|
| order     | Each step within a migration will be executed in order of the number passed in here.             |
| guid      | Unique identifier for the step, used to prevent the step re-running in future.                   |
| scope     | Whether the migration is applied once globally, or once for each profile that hasn't run it yet. |
| mode      | How to handle the migration, see below.                                                          |
| warning   | Optional string that will be displayed if the migration is set to `MigrationStepMode.Warn`.      |

A step can have one of the following modes:

* Mandatory—application will not work if this fails, so will terminate on failure to avoid catastrophe.
* Retry—failure will not affect standard operation, so app will continue and migration will be retried on next launch.
* Warn—failure will not affect standard operation, but may impact user. Will show a message, but never re-run migration.


## Registering a Migration

`Flagrum.Generators` will automatically generate a matching partial class for a class annotated with 
`[SteppedDataMigration]` that implements the necessary interface and functionality for the migration runner.

They are also automatically registered as a transient service in a source-generated extension method
named `AddDataMigrations`. This is called on the service collection to register all data migrations
so that the migration runner can pick them up.