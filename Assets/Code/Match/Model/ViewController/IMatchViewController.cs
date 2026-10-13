using System;

namespace Code.Match {
    public interface IMatchViewController : IViewController {

        event Action EndClicked;
    }
}
