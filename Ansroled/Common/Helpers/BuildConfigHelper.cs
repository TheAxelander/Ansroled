namespace Ansroled.Common.Helpers;

public static class BuildConfigHelper
{
#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif
}