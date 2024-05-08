# PCAxis.Serializers

This repository contains the serialization components required for serializing PCAxis data represented in PAXIOM from .px-files or CNNM-enabled databases. It enables the conversion from PAXIOM to various formats.

## Supported Formats

- Csv
- DecimalJson
- Excel
- Html5Table
- Json
- Xml
- Json-stat v1 & v2 (https://json-stat.org/format/)
- Parquet (https://parquet.apache.org)
- SDMX (https://sdmx.org/)
- Xslx (https://learn.microsoft.com/en-us/openspecs/office_standards/ms-xlsx/f780b2d6-8252-4074-9fe3-5d7bc4830968)


## Getting Started

### Prerequisites

- .NET Core 3.1 or later.

### Installation

```bash
dotnet add package PCAxis.Serializers 
```

## Example Usage

```csharp
using System;
using System.IO;
using PCAxis.Paxiom;
using PCAxis.Serializers;

namespace MyPxConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the PX filename as an argument.");
                return;
            }

            string pxFileName = args[0];
            string baseFileName = Path.GetFileNameWithoutExtension(pxFileName);
            string outputFileName = $"{baseFileName}.parquet";

            try
            {
                // Initialize and build the PX file model
                var builder = new PXFileBuilder(pxFileName);
                var selection = Selection.SelectAll(builder.Model.Meta);
                builder.BuildForPresentation(selection);

                // Serialize to Parquet
                using (var stream = new FileStream(outputFileName, FileMode.Create))
                {
                    var parquetSer = new ParquetSerializer();
                    parquetSer.Serialize(builder.Model, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
```

## Contributing

We welcome contributions to this project. Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

This project is licensed under the Apache-2.0 License - see the [LICENSE](LICENSE) file for details.

## Support and Feedback

For support, feature requests, and bug reporting, please open an issue on the [GitHub project page](https://github.com/PxTools/PCAxis.Serializers/issues).

## See Also

- [PxWeb Project Website](https://www.scb.se/en/services/statistical-programs-for-px-files/px-web/)
- [PxWeb on GitHub](https://github.com/statisticssweden/PxWeb)
- For the C# Serializers, see [PxWeb issue #163](https://github.com/statisticssweden/PxWeb/issues/163).
