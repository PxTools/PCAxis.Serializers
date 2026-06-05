using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Parquet;
using Parquet.Data;
using Parquet.Schema;

using PCAxis.Paxiom;
using PCAxis.Serializers.Parquet;

namespace PCAxis.Serializers
{
    /// <summary>
    /// A basic Parquet serializer, using the Row-API of Parquet.Net.
    /// </summary>
    public class ParquetSerializer : IPXModelStreamSerializer
    {
        /// <summary>
        /// Serializes the PXModel to a Parquet file at the specified path.
        /// </summary>
        /// <param name="model">The PXModel to be serialized.</param>
        /// <param name="path">The path to the output Parquet file.</param>
        public void Serialize(PXModel model, string path)
        {
            using (var stream = File.OpenWrite(path))
            {
                Serialize(model, stream);
            }
        }

        /// <summary>
        /// Serializes the PXModel to the provided stream as a Parquet file.
        /// </summary>
        /// <param name="model">The PXModel to be serialized.</param>
        /// <param name="stream">The stream to write the Parquet file.</param>
        public void Serialize(PXModel model, Stream stream)
        {
            var pb = new ParquetBuilder(model);
            var columns = pb.PopulateColumns();
            WriteColumnsAsync(columns, stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously writes Parquet data columns to the specified stream.
        /// </summary>
        /// <param name="columns">The Parquet data columns to be written.</param>
        /// <param name="stream">The stream to write the Parquet data.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private static async Task WriteColumnsAsync(DataColumn[] columns, Stream stream)
        {
            var schema = new ParquetSchema(columns.Select(column => column.Field).ToList());
            using (var writer = await ParquetWriter.CreateAsync(schema, stream))
            {
                using (var rowGroupWriter = writer.CreateRowGroup())
                {
                    foreach (var column in columns)
                    {
                        await rowGroupWriter.WriteColumnAsync(column);
                    }
                }
            }
        }
    }
}
