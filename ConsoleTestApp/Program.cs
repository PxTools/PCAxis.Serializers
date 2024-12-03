using PCAxis.Paxiom;
using PCAxis.Serializers;

class Program
{
    static void Main()
    {
        var builder = new PXFileBuilder();
        builder.SetPath(new Uri(@"../../../../UnitTests/TestFiles/MultipleContent.px", UriKind.Relative).ToString());
        builder.BuildForSelection();
        var selection = Selection.SelectAll(builder.Model.Meta);
        builder.BuildForPresentation(selection);
        var model = builder.Model;

        //// Serialize to Parquet
        //using (FileStream stream = new FileStream(new Uri(@"../../../../UnitTests/TestFiles/TestParquet.parquet", UriKind.Relative).ToString(), FileMode.Create))
        //{
        //    var parquetSer = new ParquetSerializer();
        //    try
        //    {
        //        parquetSer.Serialize(model, stream);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}

        //// Serialize to XLSX
        //using (FileStream stream = new FileStream(new Uri(@"../../../../UnitTests/TestFiles/TestExcel.xlsx", UriKind.Relative).ToString(), FileMode.Create))
        //{
        //    var ser = new XlsxSerializer();
        //    ser.DoubleColumn = DoubleColumnType.NoDoubleColumns;
        //    ser.InformationLevel = InformationLevelType.AllInformation;
        //    try
        //    {
        //        ser.Serialize(model, stream);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}

        //// Serialize to JSON
        //using (FileStream stream = new FileStream(new Uri(@"../../../../UnitTests/TestFiles/TestJson.json", UriKind.Relative).ToString(), FileMode.Create))
        //{
        //    var ser = new JsonSerializer();
        //    try
        //    {
        //        ser.Serialize(model, stream);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}

        // Serialize to CSV
        using (FileStream stream = new FileStream(new Uri(@"../../../../UnitTests/TestFiles/TestCsv.csv", UriKind.Relative).ToString(), FileMode.Create))
        {
            var ser = new CsvSerializer();
            try
            {
                ser.Serialize(model, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        Console.WriteLine("Serialization completed.");
    }
}
