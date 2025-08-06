namespace CPU.Business;

public static class EnumExtensions
{
    public static int GetMaxValue<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<int>().Max();
    }

    public static int GetCount<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Length;
    }

    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}
