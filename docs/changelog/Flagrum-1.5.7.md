## Flagrum 1.5.7

_Released 29 January 2024_

### Changes

- Further improved profile migration system
- Improved logic when updating older installs to use a stepped migration system and ensure the  
  correct migrations are set as completed automatically
- Removed legacy migration feature that accounted for active mods, any remaining issues can now  
  be fixed simply with the app reset button
- Migrated from `Clowd.Squirrel` to `Velopack` for Flagrum updates

### Bug Fixes

- Fixed bug where fresh installs would crash due to configuration file not existing
- Fixed bug with migrations where it could be null after deserialization despite the default value