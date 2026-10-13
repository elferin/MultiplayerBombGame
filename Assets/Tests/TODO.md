# Test TODO

## Integration tests (not written yet)

1. **DI wiring (offline, do this first)**
   Build a container with `MainMenuSystemInstaller` and `LobbySystemInstaller` (as `GameLifetimeScope` does)
   and resolve `MainMenuFlowInstantiator`, `LobbyFlowInstantiator` and the flow factories.
   Catches a missing `Expose<T>()`, an unregistered constructor dependency, or a type exposed by two systems.
   TearDown must destroy the `UnityLobbyProvider` GameObject that `LobbySystemInstaller` creates.

2. **Real Unity Lobby (online, `[Explicit]`, `[Category("Integration")]`)**
   Real `LobbyService` + `UnityLobbyProvider`: create a lobby, check it's valid with a code, leave, check it's gone.
   - Needs internet and the project linked to Unity Gaming Services with Lobby enabled.
   - Always leave the lobby and destroy the provider in `finally`.
   - May need PlayMode if the provider needs the player loop; Play mode also boots `GameLifetimeScope`
     through `VContainerSettings`, so use a separate empty scene or disable it.
   - Joining needs a second identity (another auth profile or a ParrelSync clone); probably keep that manual.
