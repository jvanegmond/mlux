namespace Mlux.Lib.Display
{
    public interface IMonitor
    {
        bool SupportBrightness { get; }
        int GetBrightness();
        void SetBrightness(int brightness);
        void SetColorProfile(ColorAdjustment adjustment);
        void SetColorProfile(ColorAdjustment adjustment, int gamma);
        void Reset();
    }
}