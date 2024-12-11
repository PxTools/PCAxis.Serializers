using System.IO;
using System.Threading.Tasks;

using Parquet;
using Parquet.Rows;

using PCAxis.Paxiom;

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
            var table = pb.PopulateTable();
            WriteTableAsync(table, stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously writes the Parquet table to the specified stream.
        /// </summary>
        /// <param name="table">The Parquet table to be written.</param>
        /// <param name="stream">The stream to write the Parquet table.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private static async Task WriteTableAsync(Table table, Stream stream)
        {
            await table.WriteAsync(stream);
        }
    }
}
