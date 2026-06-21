namespace NDTB.Core
{
    public interface IFocusable
    {
        public void OnBeginFocus();
        public void OnStartFocus();
        public void OnEndFocus();
    }
}
