using Code.FlowSystem;
using Code.Lobby;
using Code.MainMenu;
using Code.Match;
using Code.Profile;
using Code.Util;
using VContainer;
using VContainer.Unity;

namespace Code.DependancyInjection {
    public class GameLifetimeScope : LifetimeScope {
        
        protected override void Configure(IContainerBuilder builder) {
            
            new FlowSystemInstaller().Install(builder);
            
            new UtilSystemInstaller().Install(builder);
            new ProfileSystemInstaller().Install(builder);
            new MainMenuSystemInstaller().Install(builder);
            new LobbySystemInstaller().Install(builder);
            new MatchSystemInstaller().Install(builder);
            builder.RegisterEntryPoint<GamePresenter>();
        }
    }
}