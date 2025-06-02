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

        #region "Enunms"
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
        #endregion

        #region Private fields

        private char _delimiter = ',';
        protected PXModel _model;

        #endregion

        #region Public properties

        public bool IncludeTitle { get; set; } = false;


        private Delimiters _valueDelimiter = Delimiters.Comma;
        public Delimiters ValueDelimiter
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
            this._model = model;
            WriteTitle(wr);
            WriteHeading(wr);
            WriteTable(wr);
        }

        /// <summary>
        /// Writes a single string vaalue to a stream.
        /// </summary>
        /// <param name="wr">stream to write to</param>
        /// <param name="value">String value to write</param>
        protected static void WriteStringValue(StreamWriter wr, string value)
        {
            wr.Write('"');
            wr.Write(value);
            wr.Write('"');
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
                WriteStringValue(wr, Util.GetModelTitle(_model));
                wr.WriteLine("");
            }
        }

        /// <summary>
        /// Writes the heading (the column names separated by comma) to a stream
        /// </summary>
        /// <param name="wr">A StreamWriter that encapsulates the stream</param>
        protected void WriteHeading(StreamWriter wr)
        {
            // Write stub variable names 
            for (int i = 0; i < _model.Meta.Stub.Count; i++)
            {
                if (i > 0) wr.Write(this._delimiter);

                WriteStringValue(wr, GetLabel(_model.Meta.Stub[i]));
            }

            // Write concatenated heading variable values
            if (_model.Meta.Heading.Count > 0)
            {
                StringCollection sc = ConcatHeadingValues(0);
                wr.Write(this._delimiter);

                for (int i = 0; i < sc.Count; i++)
                {
                    if (i > 0) wr.Write(this._delimiter);

                    WriteStringValue(wr, sc[i]);
                }
                wr.WriteLine();
            }
            else
            {
                // All parameters are in the Stub
                wr.Write(this._delimiter);
                WriteStringValue(wr, _model.Meta.Contents);
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
            if (headingIndex < _model.Meta.Heading.Count - 1)
            {
                StringCollection sc2 = ConcatHeadingValues(headingIndex + 1);
                for (int valueIndex = 0; valueIndex < _model.Meta.Heading[headingIndex].Values.Count; valueIndex++)
                {
                    for (int j = 0; j < sc2.Count; j++)
                    {
                        sc.Add(GetLabel(_model.Meta.Heading[headingIndex].Values[valueIndex]) + " " + sc2[j]);
                    }
                }
            }
            else
            {
                for (int valueIndex = 0; valueIndex < _model.Meta.Heading[headingIndex].Values.Count; valueIndex++)
                {
                    sc.Add(GetLabel(_model.Meta.Heading[headingIndex].Values[valueIndex]));
                }
            }
            return sc;
        }

        /// <summary>
        /// Concatenates the stub values 
        /// </summary>
        /// <param name="stubIndex">The index of the stub variable</param>
        /// <returns>String collection with all the concatenated stub values for the given index</returns>
        private StringCollection ConcatStubValues(int stubIndex)
        {
            StringCollection sc = new StringCollection();
            if (stubIndex < _model.Meta.Stub.Count - 1)
            {
                StringCollection sc2 = ConcatStubValues(stubIndex + 1);
                for (int valueIndex = 0; valueIndex < _model.Meta.Stub[stubIndex].Values.Count; valueIndex++)
                {
                    for (int j = 0; j < sc2.Count; j++)
                    {
                        sc.Add($"\"{GetLabel(_model.Meta.Stub[stubIndex].Values[valueIndex])}\"{this._delimiter}{sc2[j]}");
                    }
                }
            }
            else
            {
                for (int valueIndex = 0; valueIndex < _model.Meta.Stub[stubIndex].Values.Count; valueIndex++)
                {
                    sc.Add($"\"{GetLabel(_model.Meta.Stub[stubIndex].Values[valueIndex])}\"");
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
            DataFormatter df = CreateDataFormater();


            if (_model.Meta.Stub.Count > 0)
            {
                sc = ConcatStubValues(0);

                if (sc.Count != _model.Data.MatrixRowCount)
                {
                    throw new PXSerializationException("Stub values do not match the data", "");
                }

                for (int i = 0; i < sc.Count; i++)
                {
                    wr.Write(sc[i]);
                    for (int c = 0; c < _model.Data.MatrixColumnCount; c++)
                    {
                        value = df.ReadElement(i, c);
                        wr.Write(this._delimiter);
                        wr.Write(value);
                    }
                    wr.WriteLine();
                }
            }
            else if (_model.Meta.Heading.Count > 0)
            {
                for (int c = 0; c < _model.Data.MatrixColumnCount; c++)
                {
                    value = df.ReadElement(0, c);
                    wr.Write(this._delimiter);
                    wr.Write(value);
                }
            }
        }

        private DataFormatter CreateDataFormater()
        {
            DataFormatter df = new DataFormatter(_model);
            df.DecimalSeparator = ".";
            df.ShowDataNotes = false;
            df.ThousandSeparator = "";
            return df;
        }



    }
}
