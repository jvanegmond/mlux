namespace Mlux.Lib.Display
{
    public interface IMonitor
    {
        int Brightness { get; set; }
        void SetColorProfile(ColorAdjustment adjustment, int gamma);
        void Reset();
    }
}