namespace PCAxis.Serializers.Excel
{


    public static class StringTests
    {
        public static bool IsNumeric(this string str)
        {
            double result;
            return double.TryParse(str, out result);
        }
    }


}
