using System;

namespace Code.MatchMenu {
    public interface IMatchMenuViewController : IViewController {

        event Action ContinueClicked;
        event Action LeaveClicked;
    }
}
