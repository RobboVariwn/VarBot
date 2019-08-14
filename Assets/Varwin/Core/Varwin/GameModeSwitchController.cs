using Varwin.Public;

namespace Varwin
{
    public class GameModeSwitchController
    {
        private readonly ISwitchModeSubscriber _switchMode;
        public GameModeSwitchController(ISwitchModeSubscriber switchModeSubscriber)
        {
            _switchMode = switchModeSubscriber;
        }

        public void SwitchGameMode(GameMode newMode, GameMode oldMode)
        {
            _switchMode?.OnSwitchMode(newMode, oldMode);
        }
    }
}
