using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace PCAxis.Paxiom
{
    /// <summary>
    /// Writes a PXModel to file or a stream in CSV format.
    /// </summary>
    public class CsvSerializer : IPXModelStreamSerializer
    {
        #region Private fields
        private PXModel _model;
        private char _delimiter = ',';
        private bool _doubleColumn = false;
        private bool _includeTitle = false;
        #endregion

        #region Public properties


        public bool DoubleColumn
        {
            get { return _doubleColumn; }
            set { _doubleColumn = value; }
        }

        public bool IncludeTitle
        {
            get { return _includeTitle; }
            set { _includeTitle = value; }
        }

        protected PXModel Model
        {
            get { return _model; }
            set { _model = value; }
        }


        public char Delimiter
        {
            get { return _delimiter; }
        }

        private Delimiters _valueDelimiter = Delimiters.Comma;
        public Delimiters DelimiterType
        {
            get
            {
                return _valueDelimiter;
            }
            set
            {
                _valueDelimiter = value;
                switch (value)
                {
                    case Delimiters.Comma:
                        _delimiter = ',';
                        break;
                    case Delimiters.Semicolon:
                        _delimiter = ';';
                        break;
                    case Delimiters.Tab:
                        _delimiter = '\t';
                        break;
                    case Delimiters.Space:
                        _delimiter = ' ';
                        break;
                    default:
                        _delimiter = ',';
                        break;
                }
            }
        }

        public LablePreference ValueLablesDisplay { get; set; } = LablePreference.None;

        #endregion

        #region Constructors
        public CsvSerializer() { }
        #endregion

        #region IPXModelStreamSerializer Interface members
        /// <summary>
        /// Write a PXModel to a file.
        /// </summary>
        /// <param name="model">The PXModel to write.</param>
        /// <param name="path">The complete file path to write to. <I>path</I> can be a file name.</param>
        public void Serialize(PXModel model, string path)
        {
            if (model == null) throw new ArgumentNullException("model");

            Encoding encoding = EncodingUtil.GetEncoding(model.Meta.CodePage);

            using (StreamWriter writer = new StreamWriter(path, false, encoding))
            {
                DoSerialize(model, writer);
            }
        }

        /// <summary>
        /// Write a PXModel to a stream.
        /// </summary>
        /// <param name="model">The PXModel to write.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <remarks>The caller is responsible of disposing the stream.</remarks>
        public void Serialize(PXModel model, Stream stream)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("The stream does not support writing", "stream");

            Encoding encoding = EncodingUtil.GetEncoding(model.Meta.CodePage);
            StreamWriter writer = new StreamWriter(stream, encoding);
            DoSerialize(model, writer);
            writer.Flush();
        }
        #endregion

        /// <summary>
        /// Serializes the model to the stream in the csv format.
        /// </summary>
        /// <param name="model">The model to serialize</param>
        /// <param name="wr">The stream to serialize to</param>
        protected virtual void DoSerialize(PXModel model, StreamWriter wr)
        {
            this.Model = model;
            WriteTitle(wr);
            WriteHeading(wr);
            WriteTable(wr);
        }

        /// <summary>
        /// Writes a single string vaalue to a stream.
        /// </summary>
        /// <param name="wr">stream to write to</param>
        /// <param name="value">String value to write</param>
        protected void WriteStringValue(StreamWriter wr, string value)
        {
                wr.Write('"');
                wr.Write(value);
                wr.WriteLine('"');
        }

        protected string GetLabel(Variable variable)
        {
            switch (ValueLablesDisplay)
            {
                case LablePreference.Code:
                    return variable.Code;
                case LablePreference.Text:
                    return variable.Name;
                case LablePreference.BothCodeAndText:
                    return $"{variable.Code} - {variable.Name}";
                default:
                    return variable.Code;
            }
        }
        protected string GetLabel(Value value)
        {
            switch (ValueLablesDisplay)
            {
                case LablePreference.Code:
                    return value.Code;
                case LablePreference.Text:
                    return value.Text;
                case LablePreference.BothCodeAndText:
                    return $"{value.Code} - {value.Text}";
                default:
                    return value.Code;
            }
        }

        /// <summary>
        /// Writes the title to the stream if title is set to true
        /// </summary>
        /// <param name="wr">The stream to write to</param>
        protected void WriteTitle(StreamWriter wr)
        {
            if (this.IncludeTitle)
            {
                WriteStringValue(wr, Util.GetModelTitle(Model));
                wr.WriteLine();
            }
        }

        /// <summary>
        /// Writes the heading (the column names separated by comma) to a stream
        /// </summary>
        /// <param name="wr">A StreamWriter that encapsulates the stream</param>
        protected void WriteHeading(StreamWriter wr)
        {
            // Write stub variable names 
            for (int i = 0; i < Model.Meta.Stub.Count; i++)
            {
                if (i > 0) wr.Write(this.Delimiter);

                if (this.DoubleColumn && Model.Meta.Stub[i].DoubleColumn)
                {
                    WriteStringValue(wr, Model.Meta.Stub[i].Code);
                    wr.Write(this.Delimiter);
                }
                WriteStringValue(wr, GetLabel(Model.Meta.Stub[i]));
            }

            // Write concatenated heading variable values
            if (Model.Meta.Heading.Count > 0)
            {
                StringCollection sc = ConcatHeadingValues(0);
                wr.Write(this.Delimiter);

                for (int i = 0; i < sc.Count; i++)
                {
                    if (i > 0) wr.Write(this.Delimiter);

                    WriteStringValue(wr, sc[i]);

                }
                wr.WriteLine();
            }
            else
            {
                // All parameters are in the Stub
                wr.Write(this.Delimiter);
                WriteStringValue(wr, Model.Meta.Matrix);
                wr.WriteLine();
            }
        }


        /// <summary>
        /// Creates the heading texts by finding all the possible combinations of the heading variables.
        /// </summary>
        /// <param name="headingIndex">The index of the heading variable</param>
        /// <returns>A string collection representing all the concatenated heading texts for the given index</returns>
        private StringCollection ConcatHeadingValues(int headingIndex)
        {
            StringCollection sc = new StringCollection();
            if (headingIndex < Model.Meta.Heading.Count - 1)
            {
                StringCollection sc2 = ConcatHeadingValues(headingIndex + 1);
                for (int valueIndex = 0; valueIndex < Model.Meta.Heading[headingIndex].Values.Count; valueIndex++)
                {
                    for (int j = 0; j < sc2.Count; j++)
                    {
                        sc.Add(Model.Meta.Heading[headingIndex].Values[valueIndex].Text + " " + sc2[j]);
                    }
                }
            }
            else
            {
                for (int valueIndex = 0; valueIndex < Model.Meta.Heading[headingIndex].Values.Count; valueIndex++)
                {
                    sc.Add(Model.Meta.Heading[headingIndex].Values[valueIndex].Text);
                }
            }
            return sc;
        }

        /// <summary>
        /// Writes the data to a stream
        /// </summary>
        /// <param name="wr">The stream to write to</param>
        protected void WriteTable(StreamWriter wr)
        {
            StringCollection sc;

            string value = "";
            bool containsDataCellNotes = Model.Meta.DataNoteCells.Count > 0;
            DataFormatter df = CreateDataFormater();

            if (Model.Meta.Stub.Count > 0)
            {
                sc = ConcatStubValues(0);

                if (sc.Count != Model.Data.MatrixRowCount)
                {
                    throw new PXSerializationException("Stub values do not match the data", "");
                }

                for (int i = 0; i < sc.Count; i++)
                {
                    wr.Write(sc[i]);
                    for (int c = 0; c < Model.Data.MatrixColumnCount; c++)
                    {
                        value = df.ReadElement(i, c);

                        if (containsDataCellNotes)
                        {
                            if (df.DataNotePlacment == DataNotePlacementType.After)
                            {
                                if (!char.IsDigit(value[value.Length - 1]))
                                {
                                    value = value.Substring(0, value.Length - 1);
                                }
                            }
                            else if (df.DataNotePlacment == DataNotePlacementType.Before)
                            {
                                if (!char.IsDigit(value[0]))
                                {
                                    value = value.Substring(1);
                                }
                            }
                        }

                        wr.Write(this.Delimiter);
                        wr.Write(value);
                    }
                    wr.WriteLine();
                }
            }
            else if (Model.Meta.Heading.Count > 0)
            {
                for (int c = 0; c < Model.Data.MatrixColumnCount; c++)
                {
                    value = df.ReadElement(0, c);

                    if (containsDataCellNotes)
                    {
                        if (df.DataNotePlacment == DataNotePlacementType.After)
                        {
                            if (!char.IsDigit(value[value.Length - 1]))
                            {
                                value = value.Substring(0, value.Length - 1);
                            }
                        }
                        else if (df.DataNotePlacment == DataNotePlacementType.Before)
                        {
                            if (!char.IsDigit(value[0]))
                            {
                                value = value.Substring(1);
                            }
                        }
                    }

                    wr.Write(this.Delimiter);
                    wr.Write(value);
                }
            }
        }

        private DataFormatter CreateDataFormater()
        {
            DataFormatter df = new DataFormatter(Model);
            df.DecimalSeparator = ".";
            df.ShowDataNotes = false;
            df.ThousandSeparator = "";
            return df;
        }

        /// <summary>
        /// Concatenates the stub values 
        /// </summary>
        /// <param name="stubIndex">The index of the stub variable</param>
        /// <returns>String collection with all the concatenated stub values for the given index</returns>
        private StringCollection ConcatStubValues(int stubIndex)
        {
            StringCollection sc = new StringCollection();
            if (stubIndex < Model.Meta.Stub.Count - 1)
            {
                StringCollection sc2 = ConcatStubValues(stubIndex + 1);
                for (int valueIndex = 0; valueIndex < Model.Meta.Stub[stubIndex].Values.Count; valueIndex++)
                {
                    for (int j = 0; j < sc2.Count; j++)
                    {
                        sc.Add(TableStub(stubIndex, valueIndex) + this.Delimiter + sc2[j]);
                    }
                }
            }
            else
            {
                for (int valueIndex = 0; valueIndex < Model.Meta.Stub[stubIndex].Values.Count; valueIndex++)
                {
                    sc.Add(TableStub(stubIndex, valueIndex));
                }
            }
            return sc;
        }

        /// <summary>
        /// Get the stub value and code
        /// </summary>
        /// <param name="stubIndex">Index of the stub variable</param>
        /// <param name="valueIndex">Index of the value</param>
        /// <returns>
        /// Returns the value. If the variable has code and double column is true both code
        /// and value are returned separated by the delimiter.
        /// </returns>
        private string TableStub(int stubIndex, int valueIndex)
        {
            StringBuilder sb = new StringBuilder();

            if (this.DoubleColumn && Model.Meta.Stub[stubIndex].DoubleColumn)
            {
                if (Model.Meta.Stub[stubIndex].Values[valueIndex].HasCode())
                {
                    sb.Append('"');
                    sb.Append(Model.Meta.Stub[stubIndex].Values[valueIndex].Code);
                    sb.Append('"');
                    sb.Append(this.Delimiter);
                }
            }

            sb.Append('"');
            sb.Append(GetLabel(Model.Meta.Stub[stubIndex].Values[valueIndex]));
            sb.Append('"');

            return sb.ToString();
        }

        public enum Delimiters
        {
            Comma,
            Semicolon,
            Tab,
            Space
        }

        public enum LablePreference
        {
            None,
            Code,
            Text,
            BothCodeAndText
        }

        
    }
}
