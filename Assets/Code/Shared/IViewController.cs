namespace Code {

    // RunAsync creates the view, shows the first gathered data and keeps the view open until Close is called, then destroys it.
    public interface IViewController : IAsyncRunnable {

        // Gathers the data again and updates the view.
        void Refresh();

        // Ends RunAsync.
        void Close();
    }
}
