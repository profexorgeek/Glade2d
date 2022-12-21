namespace Glade2d.Profiling;

internal struct ProfileData
{
    private int _nextIndex;
    private bool _rotatedAtLeastOnce;
    private long[] _elapsedTicks;

    public ProfileData(int capacity)
    {
        _nextIndex = 0;
        _rotatedAtLeastOnce = false;
        _elapsedTicks = new long[capacity];
    }
    
    
}