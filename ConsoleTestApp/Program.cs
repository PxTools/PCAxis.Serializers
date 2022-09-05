using PCAxis.Paxiom;
using PCAxis.Serializers;

var builder = new PXFileBuilder();
builder.SetPath(new Uri(@"..\..\..\..\UnitTests\TestFiles\BE0101A1.px", UriKind.Relative).ToString());
builder.BuildForSelection();
var selection = Selection.SelectAll(builder.Model.Meta);
builder.BuildForPresentation(selection);

var model = builder.Model;

        using (System.IO.FileStream stream = new System.IO.FileStream(new Uri(@"..\..\..\..\UnitTests\TestFiles\TestExcel.xlsx", UriKind.Relative).ToString(), FileMode.Create))
        {
            XlsxSerializer ser;
            ser = new XlsxSerializer();
            ser.DoubleColumn = DoubleColumnType.NoDoubleColumns;
            ser.InformationLevel = InformationLevelType.AllInformation;
            try
            {
                ser.Serialize(model, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }


