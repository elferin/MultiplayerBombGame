# Multiplayer Bomb Game

An online bomb-tag game for 2–8 players, made in Unity. One player holds a bomb with a random fuse and passes it on by touching someone else. When the fuse runs out, the holder is out, and the last player alive wins. Power-ups on the arena floor help players escape or catch others.

## How to play

1. Enter a player name in the main menu.
2. **Host** a lobby, or **join** one with the lobby code the host shares.
3. When 2–8 players are in, the host starts the match. Everyone connects through Unity Relay, so the players don't need to be on the same network.
4. Move around and touch another player to pass the bomb to them before it explodes.
5. Pick up a power-up and press **Space** to use it:
   - **Dash**: a burst of speed in the direction you last moved.
   - **Speed Boost**: a higher top speed for a while.
   - **Shield**: nobody can pass you the bomb for a while.
6. The last player alive wins the match. Then everyone goes back to the lobby, ready for the next one.

## Tech stack

| Area | Used |
| --- | --- |
| Engine | Unity **6000.6.3f1**, URP |
| Networking | [PurrNet](https://github.com/PurrNet/PurrNet) v1.23.0 over UTP + Unity Relay |
| Lobby | Unity Lobby through [PurrLobby](https://github.com/PurrNet/PurrLobby), included in `Assets/ExternalCode/PurrLobby` |
| Dependency injection | [VContainer](https://github.com/hadashiA/VContainer) 1.19.0 |
| Async | [UniTask](https://github.com/Cysharp/UniTask) 2.5.11 |
| Content | Addressables, Input System, TextMesh Pro |
| Local multiplayer testing | [ParrelSync](https://github.com/VeriorPies/ParrelSync) 1.5.3 |

## Getting started

1. Install [Git LFS](https://git-lfs.com) before cloning. Models, audio, fonts and images are stored in LFS.
   ```bash
   git lfs install
   git clone https://github.com/elferin/MultiplayerBombGame.git
   ```
2. Open the project in Unity **6000.6.3f1**. The git packages (PurrNet, VContainer, UniTask, ParrelSync) download on first open.
3. Link the project to a Unity Cloud project (**Edit → Project Settings → Services**) with **Lobby** and **Relay** turned on. Without it you can't create or join lobbies.
4. Open `Assets/Test.unity` and press Play.
5. To test with a second player on the same machine, make a clone with **ParrelSync → Clones Manager**, open it in a second Editor and join the first Editor's lobby by its code.

### Optional: merging scenes and prefabs

For better merges of scenes and prefabs, you can set up Unity's Smart Merge (UnityYAMLMerge) as the git merge tool. See [Unity's docs on Smart Merge](https://docs.unity3d.com/Manual/SmartMerge.html).

## Architecture

All the game code is in `Assets/Code` (the `Code` assembly), split into **systems**:

| System | What it does |
| --- | --- |
| `FlowSystem` | Registers flows and runs them through `IFlowRunner` |
| `GameFlow` | `GamePresenter` and the transitions between flows |
| `MainMenu` | Main menu: entering a name, hosting and joining |
| `Lobby` | Lobby membership, host settings (2–8 players), starting matches |
| `Match` | Relay allocation, starting and stopping the network, spawning players |
| `MatchMenu` | The in-match menu (e.g. leaving the match) |
| `BombTag` | The game rules, run on the host and broadcast to clients |
| `Powerup` | Spawns power-ups, handles pickups, dash, speed boost and shield |
| `Profile` | The player's name, used by other systems |
| `Util`, `Shared`, `DependancyInjection` | Shared helpers, interfaces and DI plumbing |

### How it fits together

- **Dependency injection.** `GameLifetimeScope` (the VContainer root scope, set in `VContainerSettings.asset`) installs each system's `SystemInstaller`. An installer sets up its system's own private scope and exposes only the types other systems may use.
- **Flows.** `GamePresenter` is the entry point. It runs the game as a sequence of flows: **MainMenu → Lobby → Match → Lobby → … → MainMenu**. Transition classes in `GameFlow` (`MainMenuToLobby`, `LobbyToMatch`, `MatchToLobby`, `LobbyToMainMenu`) build each flow's input from the previous flow's result, so systems never read each other's state directly.
- **Views.** Views are passive. Flows hold the logic and call `Refresh()` on view controllers when the data changes. `IViewDataGatherer<T>` maps state to view data.
- **State.** Each piece of state has a getter interface, a setter interface and a holder class, and code depends on the narrowest interface it needs. When another system must react to a change, the getter has a change event.
- **Networking.** The host owns the game. It runs the bomb-tag rules and the power-up field, and sends the results to clients. The host shares the Relay join code through the lobby, and clients connect when they see it.

## Tests

EditMode tests are in `Assets/Tests/EditMode` (the `Code.Tests.EditMode` assembly), with one folder per system. Test fakes live next to the tests of the system they fake.

Run them from **Window → General → Test Runner → EditMode**.

Integration tests (DI wiring and the real Unity Lobby) aren't written yet. They're planned in [`Assets/Tests/TODO.md`](Assets/Tests/TODO.md).

## Third-party code notes

PurrLobby is included as source and has a few local edits: an asmdef define for UTP + Relay, and changes in `UnityLobbyProvider` and `LobbyUser`. **Reimporting or updating PurrLobby removes them.** They're listed, with steps to apply them again, in [`Assets/ExternalCode/PurrLobby/LOCAL_EDITS.md`](Assets/ExternalCode/PurrLobby/LOCAL_EDITS.md).

The project also works around a PurrNet bug: `UTPTransport.Disconnect()` doesn't reset its client state, so `MatchNetwork` resets it after stopping. Check whether the workaround is still needed when you upgrade PurrNet.
