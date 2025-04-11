**Data Migrations**

These migrations are here to update Flagrum's persisted data from older versions of Flagrum to the current formats.
This has been done as there were poor design choices made in the past that we needed to move past to ensure we weren't
getting bogged down in technical debt. The migrations are very simple, a migration is a class that implements
`IDataMigration`.

```csharp
public interface IDataMigration
{
    int Order { get; }
    bool ShouldRun { get; }
    Task RunAsync();
}
```

* `Order` determines when the migration runs compared to other migrations (ascending numeric order).
* `RunAsync` is executed when the migration process runs.
* `ShouldRun` determines whether `RunAsync` should be executed or not. This will usually check some state to determine
  whether the migration has already been run before or not.

The migrations are automatically collected by Flagrum at runtime via reflection, by locating all classes in
`Flagrum.Desktop` that implement `IDataMigration`. The naming convention of these migration files isn't mandatory
for the code to work, but it has been done this way so it's easy to see the order of the migrations from the
solution explorer instead of needing to open each class to find the one you're looking for.