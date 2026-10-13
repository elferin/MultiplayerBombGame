using System;
using System.Collections.Generic;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;

namespace Code.Tests {

    // Create, join and leave requests stay pending until the test answers them, so tests can check state mid-request.
    public class FakeLobbyService : ILobbyService {

        public event Action<LobbyState> LobbyUpdated = delegate { };

        private UniTaskCompletionSource<LobbyState> _pendingCreate;
        private UniTaskCompletionSource<LobbyJoinResult> _pendingJoin;
        private UniTaskCompletionSource _pendingLeave;

        public LobbyState CurrentLobby { get; set; } = LobbyState.None;

        public int CreateCount { get; private set; }
        public int LastCreateMaxPlayers { get; private set; }
        public string LastCreatePlayerName { get; private set; }
        public bool IsCreatePending => _pendingCreate != null;

        public int JoinCount { get; private set; }
        public string LastJoinCode { get; private set; }
        public string LastJoinPlayerName { get; private set; }
        public bool IsJoinPending => _pendingJoin != null;

        public int LeaveCount { get; private set; }
        public bool IsLeavePending => _pendingLeave != null;

        public string MatchJoinCode { get; private set; } = string.Empty;
        public bool IsLocked { get; private set; }

        public async UniTask<LobbyState> CreateLobby(int maxPlayers, string playerName, CancellationToken cancellation) {
            CreateCount++;
            LastCreateMaxPlayers = maxPlayers;
            LastCreatePlayerName = playerName;
            _pendingCreate = new UniTaskCompletionSource<LobbyState>();

            var lobby = await _pendingCreate.Task.AttachExternalCancellation(cancellation);
            CurrentLobby = lobby;
            return lobby;
        }

        public async UniTask<LobbyJoinResult> JoinLobby(string lobbyCode, string playerName, CancellationToken cancellation) {
            JoinCount++;
            LastJoinCode = lobbyCode;
            LastJoinPlayerName = playerName;
            _pendingJoin = new UniTaskCompletionSource<LobbyJoinResult>();

            var result = await _pendingJoin.Task.AttachExternalCancellation(cancellation);
            CurrentLobby = result.Lobby;
            return result;
        }

        public async UniTask LeaveLobby() {
            LeaveCount++;
            _pendingLeave = new UniTaskCompletionSource();

            await _pendingLeave.Task;
            CurrentLobby = LobbyState.None;
        }

        public void SucceedCreate() {
            Answer(ref _pendingCreate, CreateLobbyState("NEW123", LastCreateMaxPlayers, LastCreatePlayerName));
        }

        public void FailCreate() {
            Answer(ref _pendingCreate, LobbyState.None);
        }

        public void SucceedJoin() {
            SucceedJoin(CreateLobbyState(LastJoinCode, 4, LastJoinPlayerName));
        }

        public void SucceedJoin(LobbyState lobby) {
            Answer(ref _pendingJoin, new LobbyJoinResult(lobby, LobbyJoinFailure.None));
        }

        public void FailJoin(LobbyJoinFailure failure = LobbyJoinFailure.Other) {
            Answer(ref _pendingJoin, new LobbyJoinResult(LobbyState.None, failure));
        }

        public void CompleteLeave() {
            var pendingLeave = _pendingLeave;
            _pendingLeave = null;
            pendingLeave.TrySetResult();
        }

        public UniTask StartMatch(string joinCode) {
            IsLocked = true;
            MatchJoinCode = joinCode;
            return UniTask.CompletedTask;
        }

        public void RaiseLobbyUpdated(LobbyState lobby) {
            LobbyUpdated(lobby);
        }

        // A lobby the local player created, so they are its host.
        public static LobbyState CreateLobbyState(string code, int maxPlayers, params string[] playerNames) {
            return new LobbyState(true, code, maxPlayers, new List<string>(playerNames), true, string.Empty);
        }

        private static void Answer<T>(ref UniTaskCompletionSource<T> pending, T result) {
            var request = pending;
            pending = null;
            request.TrySetResult(result);
        }
    }
}
