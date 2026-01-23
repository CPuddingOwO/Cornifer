namespace Cornifer.Helpers;

public static class AnyArrayExtensions {
    public static bool TryGet<T>(this T[] array, int index, out T value) {
        if (index < array.Length) {
            value = array[index];
            return true;
        }

        value = default!;
        return false;
    }
}