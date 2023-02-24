namespace ApiSharp.Comparers;

public class ByteOrderComparer : IComparer<byte[]>
{
    public int Compare(byte[] x, byte[] y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        int num = Math.Min(x.Length, y.Length);
        for (int i = 0; i < num; i++)
        {
            byte x2 = x[i];
            byte y2 = y[i];
            int num2 = Comparer<byte>.Default.Compare(x2, y2);
            if (num2 != 0) return num2;
        }

        if (x.Length == y.Length) return 0;
        if (x.Length >= y.Length) return 1;

        return -1;
    }
}
