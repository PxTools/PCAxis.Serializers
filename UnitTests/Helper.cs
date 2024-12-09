using PCAxis.Paxiom;

namespace UnitTests
{
    public class Helper
    {


        public PXModel GetSelectAllModel(string file)
        {
            PCAxis.Paxiom.IPXModelBuilder builder = new PXFileBuilder();

            builder.SetPath(file);
            builder.BuildForSelection();

            PXMeta meta = builder.Model.Meta;
            builder.BuildForPresentation(Selection.SelectAll(meta));

            return builder.Model;
        }
    }
}
