using System;

using ClosedXML.Excel;

namespace PCAxis.Serializers.Excel
{
    internal static class CellValueHelper
    {
        internal static void SetTypedCellValue(IXLCell cell, object value)
        {
            switch (value)
            {
                case string s:
                    cell.SetValue(s);
                    break;
                case bool b:
                    cell.SetValue(b);
                    break;
                case int i:
                    cell.SetValue(i);
                    break;
                case long l:
                    cell.SetValue(l);
                    break;
                case double d:
                    cell.SetValue(d);
                    break;
                case float f:
                    cell.SetValue(f);
                    break;
                case decimal m:
                    cell.SetValue((double)m);
                    break;
                case DateTime dt:
                    cell.SetValue(dt);
                    break;
                case TimeSpan ts:
                    cell.SetValue(ts);
                    break;
                default:
                    cell.SetValue(value?.ToString());
                    break;
            }
        }
    }
}
