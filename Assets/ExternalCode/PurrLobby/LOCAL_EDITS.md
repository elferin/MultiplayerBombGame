# Local edits to PurrLobby

This copy of PurrLobby has been changed by hand. **Reimporting or updating PurrLobby removes these changes without any compile error.**
After a reimport, apply each edit again and check it with the steps at the end.

Every code edit is marked with a `// Manual edit` comment, so you can find them by searching for `Manual edit` in this folder.

## 1. `UTP_LOBBYRELAY` define

**Files:** `Runtime/PurrNetLobby_Runtime.asmdef` (the Editor asmdef, `Editor/PurrNetLobby_Editor.asmdef`, has the same entry).

**Change:** this entry in `versionDefines`:

```json
{
    "name": "com.unity.services.multiplayer",
    "expression": "1.1.2",
    "define": "UTP_LOBBYRELAY"
}
```

**Why:** PurrLobby only compiles `UnityLobbyProvider` when `UTP_LOBBYRELAY` is defined. This entry defines it whenever the project's `com.unity.services.multiplayer` package is installed.

**If missing:** `Code/Lobby` stops compiling, because `UnityLobbyProvider` doesn't exist.

## 2. `UnityLobbyProvider.LastJoinException`

**File:** `Runtime/Providers/UnityLobbyProvider.cs`

**Change:**
- New property: `public Exception LastJoinException { get; private set; }`
- `JoinLobbyByCodeAsync` sets it to `null` at the start, and to the caught exception in its `catch`.

**Why:** PurrLobby only logs why a join failed. `LobbyService.GetJoinFailure` reads the exception's `LobbyExceptionReason`. That's how the Join panel shows "Lobby is full" or "Match already in progress" instead of "Couldn't join".

**If missing:** `Code/Lobby/Services/LobbyService.cs` stops compiling.

## 3. `LobbyUser.IsInLobby`

**Files:** `Runtime/Lobby/LobbyUser.cs` and `Runtime/Providers/UnityLobbyProvider.cs`

**Change:**
- `LobbyUser` gets `public bool IsInLobby;`
- `UnityLobbyProvider.GetLobbyUsers` fills it from the `InLobby` player data key:
  ```csharp
  IsInLobby = !player.Data.TryGetValue("InLobby", out var inLobby) || inLobby.Value == "True",
  ```
  A missing key counts as `true`, because a player who never set it has just joined.

**Why:** after a match, the host can only start again when everyone is back on the lobby screen. Players who don't come back within 30 seconds are removed (`LobbyStartRules`, `MissingPlayerRemover`).

**If missing:** `LobbyService` stops compiling. If the line in `GetLobbyUsers` is missing but the field is there, the code compiles but every player counts as not back. Start then stays greyed out after a match, and the host removes everyone after 30 seconds.

## Keys the game relies on

These aren't edits, but the game reads and writes them directly, so they must still match after an update:

| Key | Kind | Set by | Read by |
|---|---|---|---|
| `JoinCode` | lobby data | PurrLobby creates it in `CreateLobbyAsync`; `LobbyService.StartMatch` / `EndMatch` set it | `LobbyService` |
| `MatchStarting` | lobby data | `LobbyService` | `LobbyService` |
| `Name` | player data | PurrLobby, `InitializeLocalPlayerData` | PurrLobby, `LobbyService.FetchPlayers` |
| `InLobby` | player data | `LobbyService.SetInLobby` (through the SDK) | edit 3, `LobbyService.FetchPlayers` |

## Checking after a reimport

1. Search this folder for `Manual edit`. There should be three hits: one in `LobbyUser.cs`, and two in `UnityLobbyProvider.cs` (above the `LastJoinException` property, and in `GetLobbyUsers`). The two `LastJoinException` lines in `JoinLobbyByCodeAsync` aren't marked, so check them by hand.
2. Check that `UTP_LOBBYRELAY` is in the asmdef's `versionDefines`.
3. The project compiles, and the EditMode tests pass.
4. With a host and a ParrelSync clone, join a full or started lobby and check the message. Then play a match, return to the lobby, and check that Start becomes available once both players are back.
