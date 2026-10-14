using System;

namespace Code.Lobby {
    public interface ILobbyViewController : IViewController {

        event Action LeaveClicked;
        event Action StartClicked;
        event Action CopyCodeClicked;
    }
}
