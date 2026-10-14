using System;

namespace Code.MainMenu {
    public interface IMainMenuViewController : IViewController {

        event Action QuitClicked;
        event Action HostClicked;
        event Action JoinClicked;

        event Action<string> PlayerNameChanged;
        event Action<string> PlayerCountChanged;
        event Action<string> LobbyCodeChanged;
        event Action CreateLobbyClicked;
        event Action JoinLobbyClicked;
        event Action BackClicked;
        event Action PasteCodeClicked;
    }
}
