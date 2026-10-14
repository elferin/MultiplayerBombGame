using UnityEngine;

namespace PurrLobby
{
    public struct LobbyUser
    {
        public string Id;
        public string DisplayName;
        public bool IsReady;
        // Manual edit (not part of PurrLobby): the player is on the lobby screen, not still in a match.
        public bool IsInLobby;
        public Texture2D Avatar;
    }
}