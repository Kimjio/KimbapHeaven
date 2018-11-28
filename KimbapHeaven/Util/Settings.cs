using Windows.Storage;

namespace KimbapHeaven
{
    public static class Settings
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        #region Perfixs
        private static readonly string PREFIX_BOOL = "_bool";
        private static readonly string PREFIX_BYTE = "_byte";
        private static readonly string PREFIX_SBYTE = "_sbyte";
        private static readonly string PREFIX_CHAR = "_char";
        private static readonly string PREFIX_DECIMAL = "_decimal";
        private static readonly string PREFIX_DOUBLE = "_double";
        private static readonly string PREFIX_FLOAT = "_float";
        private static readonly string PREFIX_INT = "_int";
        private static readonly string PREFIX_UINT = "_uint";
        private static readonly string PREFIX_LONG = "_long";
        private static readonly string PREFIX_ULONG = "_ulong";
        private static readonly string PREFIX_SHORT = "_short";
        private static readonly string PREFIX_USHORT = "_ushort";
        private static readonly string PREFIX_STRING = "_string";
        #endregion

        #region bool
        public static void PutBool(string name, bool value)
        {
            LocalSettings.Values[name + PREFIX_BOOL] = value;
        }

        public static bool GetBool(string name, bool defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_BOOL];

            return value != null ? (bool) value : defValue;
        }
        #endregion

        #region byte
        public static void PutByte(string name, byte value)
        {
            LocalSettings.Values[name + PREFIX_BYTE] = value;
        }

        public static byte GetByte(string name, byte defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_BYTE];

            return value != null ? (byte) value : defValue;
        }
        #endregion

        #region sbyte
        public static void PutSByte(string name, sbyte value)
        {
            LocalSettings.Values[name + PREFIX_SBYTE] = value;
        }

        public static sbyte GetSByte(string name, sbyte defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_SBYTE];

            return value != null ? (sbyte) value : defValue;
        }
        #endregion

        #region char
        public static void PutChar(string name, char value)
        {
            LocalSettings.Values[name + PREFIX_CHAR] = value;
        }

        public static char GetChar(string name, char defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_CHAR];

            return value != null ? (char) value : defValue;
        }
        #endregion
        
        #region decimal
        public static void PutDecimal(string name, decimal value)
        {
            LocalSettings.Values[name + PREFIX_DECIMAL] = value;
        }

        public static decimal GetDecimal(string name, decimal defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_DECIMAL];

            return value != null ? (decimal) value : defValue;
        }
        #endregion

        #region double
        public static void PutDouble(string name, double value)
        {
            LocalSettings.Values[name + PREFIX_DECIMAL] = value;
        }

        public static double GetDouble(string name, double defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_DOUBLE];

            return value != null ? (double) value : defValue;
        }
        #endregion

        #region float
        public static void PutFloat(string name, float value)
        {
            LocalSettings.Values[name + PREFIX_FLOAT] = value;
        }

        public static float GetFloat(string name, float defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_FLOAT];

            return value != null ? (float) value : defValue;
        }
        #endregion

        #region int
        public static void PutInt(string name, int value)
        {
            LocalSettings.Values[name + PREFIX_INT] = value;
        }

        public static int GetInt(string name, int defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_INT];

            return value != null ? (int) value : defValue;
        }
        #endregion

        #region uint
        public static void PutUInt(string name, int value)
        {
            LocalSettings.Values[name + PREFIX_UINT] = value;
        }

        public static uint GetUInt(string name, uint defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_UINT];

            return value != null ? (uint) value : defValue;
        }
        #endregion

        #region long
        public static void PutLong(string name, long value)
        {
            LocalSettings.Values[name + PREFIX_LONG] = value;
        }

        public static long GetLong(string name, long defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_LONG];

            return value != null ? (long) value : defValue;
        }
        #endregion

        #region ulong
        public static void PutULong(string name, ulong value)
        {
            LocalSettings.Values[name + PREFIX_ULONG] = value;
        }

        public static ulong GetUInt(string name, ulong defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_ULONG];

            return value != null ? (ulong) value : defValue;
        }
        #endregion

        #region object
        public static void Put(string name, object value)
        {
            LocalSettings.Values[name] = value;
        }

        public static object Get(string name, object defValue)
        {
            object value = LocalSettings.Values[name];

            return value ?? defValue;
        }
        #endregion

        #region short
        public static void PutShort(string name, short value)
        {
            LocalSettings.Values[name + PREFIX_SHORT] = value;
        }

        public static short GetShort(string name, short defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_SHORT];

            return value != null ? (short) value : defValue;
        }
        #endregion

        #region ushort
        public static void PutUShort(string name, ushort value)
        {
            LocalSettings.Values[name + PREFIX_USHORT] = value;
        }

        public static ushort GetUShort(string name, ushort defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_USHORT];

            return value != null ? (ushort) value : defValue;
        }
        #endregion

        #region string
        public static void PutString(string name, string value)
        {
            LocalSettings.Values[name + PREFIX_STRING] = value;
        }

        public static string GetString(string name, string defValue)
        {
            object value = LocalSettings.Values[name + PREFIX_STRING];

            return value != null ? value.ToString() : defValue;
        }
        #endregion
    }
}
