using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PurrLobby;
using PurrLobby.Providers;
using UnityEngine;
// Our class is also called LobbyService.
using UnityLobbies = Unity.Services.Lobbies;

namespace Code.Lobby {

    public class LobbyService : ILobbyService, IDisposable {

        // The lobby property UnityLobbyProvider creates for the relay join code.
        private const string MATCH_JOIN_CODE_KEY = "JoinCode";
        // The player data key UnityLobbyProvider reads into LobbyUser.IsInLobby.
        private const string IN_LOBBY_KEY = "InLobby";
        // The player data key UnityLobbyProvider stores the display name under.
        private const string NAME_KEY = "Name";
        private const string MATCH_STARTING_KEY = "MatchStarting";
        private const string TRUE_VALUE = "True";
        // Doubling delays (1, 2, 4, 8 seconds) outlast the rate limit when everyone returns at once,
        // and still finish well before the host removes players who haven't returned.
        private const int IN_LOBBY_ATTEMPTS = 5;
        private static readonly TimeSpan IN_LOBBY_FIRST_RETRY_DELAY = TimeSpan.FromSeconds(1);

        public event Action<LobbyState> LobbyUpdated = delegate { };

        // The Unity provider specifically: joining by code isn't part of PurrLobby's ILobbyProvider.
        private readonly UnityLobbyProvider _provider;
        private PurrLobby.Lobby _currentLobby;
        private bool _isDisposed;

        public LobbyState CurrentLobby { get; private set; } = LobbyState.None;

        public LobbyService(UnityLobbyProvider provider) {
            _provider = provider;
            _provider.OnLobbyUpdated += OnProviderLobbyUpdated;
        }

        public async UniTask<LobbyState> CreateLobby(int maxPlayers, string playerName, CancellationToken cancellation) {
            // Signs in to Unity Services on first use; does nothing once signed in.
            await _provider.InitializeAsync();
            cancellation.ThrowIfCancellationRequested();

            // The provider sends this as the player's name when it creates or joins a lobby.
            _provider.playerName = playerName;

            // The provider's request can't be interrupted, so cancellation is checked once it finishes.
            var lobby = await _provider.CreateLobbyAsync(maxPlayers, new Dictionary<string, string>());
            if (cancellation.IsCancellationRequested) {
                // Created after the caller stopped waiting; leave so it doesn't stay open on the server.
                if (lobby.IsValid) {
                    await _provider.LeaveLobbyAsync(lobby.LobbyId);
                }
                cancellation.ThrowIfCancellationRequested();
            }

            SetCurrentLobby(lobby);
            return CurrentLobby;
        }

        public async UniTask<LobbyJoinResult> JoinLobby(string lobbyCode, string playerName, CancellationToken cancellation) {
            await _provider.InitializeAsync();
            cancellation.ThrowIfCancellationRequested();

            _provider.playerName = playerName;

            var lobby = await _provider.JoinLobbyByCodeAsync(lobbyCode);
            if (cancellation.IsCancellationRequested) {
                // Joined after the caller stopped waiting; leave so the player doesn't stay in it.
                if (lobby.IsValid) {
                    await _provider.LeaveLobbyAsync(lobby.LobbyId);
                }
                cancellation.ThrowIfCancellationRequested();
            }

            SetCurrentLobby(lobby);
            return new LobbyJoinResult(CurrentLobby, lobby.IsValid ? LobbyJoinFailure.None : GetJoinFailure());
        }

        public async UniTask LeaveLobby() {
            if (!_currentLobby.IsValid) {
                return;
            }

            await _provider.LeaveLobbyAsync();
            SetCurrentLobby(default);
        }

        public async UniTask StartMatch(string joinCode) {
            // A locked lobby rejects new joins on the server. PurrLobby's provider can't lock, so this uses the SDK directly.
            // Locking first means nobody can join after the code is shared.
            try {
                await UnityLobbies.LobbyService.Instance.UpdateLobbyAsync(_currentLobby.LobbyId,
                    new UnityLobbies.UpdateLobbyOptions { IsLocked = true });
            }
            catch (UnityLobbies.LobbyServiceException exception) {
                // The match still starts; new players can then still join it.
                Debug.LogException(exception);
            }

            // Through the provider, so its copy of the lobby data stays up to date.
            await _provider.SetLobbyDataAsync(MATCH_JOIN_CODE_KEY, joinCode);
        }

        public async UniTask EndMatch() {
            // The flag first: a lobby with the flag but no code would look like a new match starting.
            await _provider.SetLobbyDataAsync(MATCH_STARTING_KEY, string.Empty);
            await _provider.SetLobbyDataAsync(MATCH_JOIN_CODE_KEY, string.Empty);

            // Unlocked last, so nobody joins while the last match's data is still there and gets told it's in progress.
            try {
                await UnityLobbies.LobbyService.Instance.UpdateLobbyAsync(_currentLobby.LobbyId,
                    new UnityLobbies.UpdateLobbyOptions { IsLocked = false });
            }
            catch (UnityLobbies.LobbyServiceException exception) {
                // New players then can't join until the next match ends.
                Debug.LogException(exception);
            }
        }

        public async UniTask AnnounceMatchStart() {
            await _provider.SetLobbyDataAsync(MATCH_STARTING_KEY, TRUE_VALUE);
        }

        public async UniTask SetInLobby(bool isInLobby, CancellationToken cancellation) {
            if (!_currentLobby.IsValid) {
                return;
            }

            // Through the SDK, because the provider hides failed requests, and a lost "back" would get the player removed.
            var options = new UnityLobbies.UpdatePlayerOptions {
                Data = new Dictionary<string, UnityLobbies.Models.PlayerDataObject> {
                    { IN_LOBBY_KEY, new UnityLobbies.Models.PlayerDataObject(
                        UnityLobbies.Models.PlayerDataObject.VisibilityOptions.Public, isInLobby.ToString()) }
                }
            };
            var retryDelay = IN_LOBBY_FIRST_RETRY_DELAY;
            for (var attempt = 1; ; attempt++) {
                try {
                    await UnityLobbies.LobbyService.Instance.UpdatePlayerAsync(_currentLobby.LobbyId, _provider.LocalPlayerId, options);
                    return;
                }
                catch (UnityLobbies.LobbyServiceException exception) {
                    // Usually the request rate limit, which passes after a moment.
                    if (attempt == IN_LOBBY_ATTEMPTS) {
                        Debug.LogException(exception);
                        return;
                    }
                }
                await UniTask.Delay(retryDelay, DelayType.Realtime, cancellationToken: cancellation);
                retryDelay += retryDelay;
            }
        }

        public async UniTask<bool> ReturnToLobby(CancellationToken cancellation) {
            if (!_currentLobby.IsValid) {
                return false;
            }

            if (!await IsStillMember()) {
                SetCurrentLobby(default);
                return false;
            }
            await SetInLobby(true, cancellation);
            return true;
        }

        public async UniTask<IReadOnlyList<LobbyPlayer>> FetchPlayers() {
            if (!_currentLobby.IsValid) {
                return Array.Empty<LobbyPlayer>();
            }

            try {
                var lobby = await UnityLobbies.LobbyService.Instance.GetLobbyAsync(_currentLobby.LobbyId);
                var players = new List<LobbyPlayer>();
                foreach (var player in lobby.Players) {
                    // Like UnityLobbyProvider: players who never set InLobby have just joined, so they are in the lobby.
                    var isInLobby = GetPlayerData(player, IN_LOBBY_KEY, TRUE_VALUE) == TRUE_VALUE;
                    players.Add(new LobbyPlayer(player.Id, GetPlayerData(player, NAME_KEY, string.Empty), isInLobby,
                        player.Id == _provider.LocalPlayerId));
                }
                return players;
            }
            catch (UnityLobbies.LobbyServiceException exception) {
                Debug.LogException(exception);
                return Array.Empty<LobbyPlayer>();
            }
        }

        public async UniTask RemovePlayers(IReadOnlyList<string> playerIds) {
            // PurrLobby's provider can only remove the local player, so this uses the SDK directly.
            foreach (var playerId in playerIds) {
                try {
                    await UnityLobbies.LobbyService.Instance.RemovePlayerAsync(_currentLobby.LobbyId, playerId);
                }
                catch (UnityLobbies.LobbyServiceException exception) {
                    Debug.LogException(exception);
                }
            }
        }

        public void Dispose() {
            _isDisposed = true;
            _provider.OnLobbyUpdated -= OnProviderLobbyUpdated;
        }

        private void OnProviderLobbyUpdated(PurrLobby.Lobby lobby) {
            // Lobby events can arrive off the main thread; Unity objects must only be touched on it.
            UniTask.Post(() => {
                // A late update for a lobby already left mustn't replace the one the player is in now.
                if (!_currentLobby.IsValid || !lobby.IsValid || lobby.LobbyId != _currentLobby.LobbyId) {
                    return;
                }
                SetCurrentLobby(lobby);
            });
        }

        private void SetCurrentLobby(PurrLobby.Lobby lobby) {
            // A request can finish after the scope is gone (e.g. Play mode ended); nobody should hear about it then.
            if (_isDisposed) {
                return;
            }

            _currentLobby = lobby;
            CurrentLobby = ToLobbyState(lobby);
            LobbyUpdated.Invoke(CurrentLobby);
        }

        private LobbyState ToLobbyState(PurrLobby.Lobby lobby) {
            if (!lobby.IsValid) {
                return LobbyState.None;
            }

            var players = new List<LobbyPlayer>();
            foreach (var member in lobby.Members) {
                players.Add(new LobbyPlayer(member.Id, member.DisplayName, member.IsInLobby, member.Id == _provider.LocalPlayerId));
            }
            return new LobbyState(true, lobby.LobbyCode, lobby.MaxPlayers, players, lobby.IsOwner, GetMatchJoinCode(lobby),
                IsMatchStarting(lobby));
        }

        // Asks Unity Lobby, because PurrLobby doesn't notice being removed and keeps the old lobby.
        private async UniTask<bool> IsStillMember() {
            try {
                var lobby = await UnityLobbies.LobbyService.Instance.GetLobbyAsync(_currentLobby.LobbyId);
                return lobby.Players.Exists(player => player.Id == _provider.LocalPlayerId);
            }
            catch (UnityLobbies.LobbyServiceException exception) {
                // Removed players can't read the lobby any more; other errors don't mean the player was removed.
                return exception.Reason != UnityLobbies.LobbyExceptionReason.Forbidden
                       && exception.Reason != UnityLobbies.LobbyExceptionReason.LobbyNotFound;
            }
        }

        private LobbyJoinFailure GetJoinFailure() {
            if (!(_provider.LastJoinException is UnityLobbies.LobbyServiceException exception)) {
                return LobbyJoinFailure.Other;
            }

            switch (exception.Reason) {
                // StartMatch locks the lobby, so a locked lobby means its match is running.
                case UnityLobbies.LobbyExceptionReason.LobbyLocked: return LobbyJoinFailure.MatchInProgress;
                case UnityLobbies.LobbyExceptionReason.LobbyFull: return LobbyJoinFailure.LobbyFull;
                default: return LobbyJoinFailure.Other;
            }
        }

        private bool IsMatchStarting(PurrLobby.Lobby lobby) {
            return lobby.Properties != null
                   && lobby.Properties.TryGetValue(MATCH_STARTING_KEY, out var value)
                   && value == TRUE_VALUE;
        }

        private string GetMatchJoinCode(PurrLobby.Lobby lobby) {
            if (lobby.Properties != null && lobby.Properties.TryGetValue(MATCH_JOIN_CODE_KEY, out var joinCode)) {
                return joinCode ?? string.Empty;
            }
            return string.Empty;
        }

        private string GetPlayerData(UnityLobbies.Models.Player player, string key, string fallback) {
            if (player.Data != null && player.Data.TryGetValue(key, out var data) && data != null) {
                return data.Value;
            }
            return fallback;
        }
    }
}
