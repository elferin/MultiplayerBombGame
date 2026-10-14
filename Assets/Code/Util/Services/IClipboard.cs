namespace Code.Util {

    // The operating system's clipboard.
    public interface IClipboard {

        string GetText();
        void SetText(string text);
    }
}
