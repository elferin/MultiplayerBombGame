namespace Code.MainMenu {

    // What the player is allowed to do on the Host and Join panels. Shared by MainMenuFlow and MainMenuViewDataGatherer.
    public interface IMainMenuLobbyRules {

        bool CanCreateLobby();
        bool CanJoinLobby();
    }
}
