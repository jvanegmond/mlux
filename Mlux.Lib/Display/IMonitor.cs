namespace Mlux.Lib.Display
{
    public interface IMonitor
    {
        byte GetBrightness();
        void SetBrightness(uint brightness);
        void SetColorProfile(ColorAdjustment adjustment, int gamma);
        void Reset();
    }
}