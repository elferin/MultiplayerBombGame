using Code.BombTag;
using Code.FlowSystem;
using Code.GameFlow;
using Code.Lobby;
using Code.MainMenu;
using Code.Match;
using Code.Powerup;
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
            new PowerupSystemInstaller().Install(builder);
            new BombTagSystemInstaller().Install(builder);
            new GameFlowSystemInstaller().Install(builder);
            builder.RegisterEntryPoint<GamePresenter>();
        }
    }
}