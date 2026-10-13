namespace Code {

    // Reads the current model state and builds the data a view displays.
    public interface IViewDataGatherer<out TViewData> {
        TViewData Get();
    }
}
