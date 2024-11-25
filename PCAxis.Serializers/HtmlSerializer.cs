using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using PCAxis.Paxiom;

namespace PCAxis.Serializers
{
    public class HtmlSerializer : PCAxis.Paxiom.IPXModelStreamSerializer
    {
        public enum LablePreference
        {
            None,
            Code,
            Text,
            BothCodeAndText
        }

        private int[] _subStubValues;
        private DataFormatter _fmt;

        public bool IncludeTitle { get; set; } = false;
        public LablePreference ValueLablesDisplay { get; set; } = LablePreference.None;

        public void Serialize(PXModel model, string path)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }


            //Let the StreamWriter verify the path argument

            using (var writer = new System.IO.StreamWriter(path, false, System.Text.Encoding.GetEncoding(model.Meta.CodePage)))
            {
                DoSerialize(model, writer);
            }


        }
        public void Serialize(PXModel model, Stream stream)
        {

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentNullException("stream", "The stream does not support writing");
            }

            var writer = new System.IO.StreamWriter(stream, System.Text.Encoding.GetEncoding(model.Meta.CodePage));

            DoSerialize(model, writer);
        }

        private void DoSerialize(PXModel model, StreamWriter wr)
        {
            wr.WriteLine(@"<table id=""" + model.Meta.Matrix + "_" + Guid.NewGuid().ToString() + @""" >"); //@""" aria-describedby="" "

            // Only write title if it is set to be included
            if (IncludeTitle)
            {
                wr.Write("<caption>");
                wr.Write(model.Meta.Title);
                wr.WriteLine("</caption>");
            }

            //Write table headings
            wr.WriteLine("<thead>");
            WriteHeadings(wr, model);
            wr.WriteLine("</thead>");

            // Calculate sub value sizes
            Paxiom.Variables stub = model.Meta.Stub;
            Array.Resize(ref _subStubValues, stub.Count + 1);
            CalculateSubValues(stub, 0, ref _subStubValues);


            // Write the table, start at row zero
            wr.WriteLine("<tbody>");
            int levels = stub.Count;
            int row = 0;
            WriteTable(wr, model, levels, 0, ref row);

            wr.WriteLine("</tbody>");
            wr.WriteLine("</table>");
            wr.Flush();
        }

        private int CalculateSubValues(Variables vars, int level, ref int[] subValues)
        {
            if ((vars.Count == 0))
            {
                subValues[level] = 1;
                return 0;
            }
            else if (((vars.Count - 1)
                        == level))
            {
                subValues[level] = 1;
                return vars[level].Values.Count;
            }
            else
            {
                int nextLevel = (level + 1);
                int ret = CalculateSubValues(vars, nextLevel, ref subValues);
                subValues[level] = ret;
                return (ret * vars[level].Values.Count);
            }

        }

        private void WriteHeadings(System.IO.StreamWriter wr, PCAxis.Paxiom.PXModel model)
        {
            Variables heading = model.Meta.Heading;
            if (heading != null && heading.Count > 0)
            {
                int[] subHeadings = new int[heading.Count];
                CalculateSubValues(heading, 0, ref subHeadings);

                //  This keep track of the number of times the current heading shall be written
                int timesToWrite = 1;

                //  This keep track of the number of times the current heading has been written
                int timesWritten = 0;

                for (int index = 0; (index
                            <= (heading.Count - 1)); index++)
                {
                    wr.WriteLine("<tr>");

                    WriteEmptyHeadingForStub(wr, model);

                    //  Write the heading
                    for (int j = 0; (j <= (timesToWrite - 1)); j++)
                    {
                        Paxiom.Values headingValues = heading[index].Values;
                        for (int ix = 0; (ix <= (headingValues.Count - 1)); ix++)
                        {
                            WriteHeadingOpeningTag(wr, heading, subHeadings[index], index);
                            wr.Write(GetLabel(headingValues[ix]));
                            wr.WriteLine("</th>");

                            timesWritten += 1;
                        }
                    }

                    timesToWrite = timesWritten;
                    timesWritten = 0;
                    wr.WriteLine("</tr>");
                }

            }

        }

        private static void WriteHeadingOpeningTag(StreamWriter wr, Variables heading, int spanSize, int index)
        {
            if (index == heading.Count - 1)
            {
                wr.Write(@"<th scope=""col"">");
            }
            else
            {
                wr.Write("<th colspan=");
                wr.Write(spanSize);
                wr.Write(">");
            }
        }

        private static void WriteEmptyHeadingForStub(StreamWriter wr, PXModel model)
        {
            if ((model.Meta.Stub.Count > 0))
            {
                wr.WriteLine("<th></th>");
            }
        }

        private string GetLabel(Value value)
        {
            switch (ValueLablesDisplay)
            {
                case LablePreference.Code:
                    return value.Code;
                case LablePreference.Text:
                    return value.Text;
                case LablePreference.BothCodeAndText:
                    return value.Code + " " + value.Text;
                default:
                    return value.Text;
            }
        }

        private void WriteDataLine(System.IO.StreamWriter wr, PCAxis.Paxiom.PXModel model, int row)
        {
            string value;
            string n = String.Empty;
            string dataNote = String.Empty;

            PCAxis.Paxiom.PXData data = model.Data;
            for (int c = 0; (c
                        <= (data.MatrixColumnCount - 1)); c++)
            {
                wr.Write("<td>");
                value = _fmt.ReadElement(row, c, ref n, ref dataNote);
                wr.Write(value);
                wr.WriteLine("</td>");
            }

        }

        private void WriteTable(System.IO.StreamWriter wr, Paxiom.PXModel model, int levels, int level, ref int row)
        {
            _fmt = new DataFormatter(model);

            if ((level == levels))
            {
                //  Time to write the data to the file
                WriteDataLine(wr, model, row);
                //  Close this row. The closing tag is not writen if level + 1 < levels, se
                //  the else clause below
                wr.WriteLine("</tr>");
                row = (row + 1);
            }
            else
            {
                Paxiom.Values values = model.Meta.Stub[level].Values;
                int nextLevel = (level + 1);
                for (int i = 0; (i <= (values.Count - 1)); i++)
                {
                    wr.WriteLine("<tr>");
                    wr.Write(@"<th scope=""row"">");

                    wr.Write(values[i].Text);
                    wr.WriteLine("</th>");
                    _fmt = new DataFormatter(model);


                    if (level + 1 < levels)
                    {
                        for (int y = 0; y <= model.Data.MatrixColumnCount - 1; y++)
                        {
                            wr.WriteLine("<td></td>");
                        }
                        wr.WriteLine("</tr>");
                    }


                    WriteTable(wr, model, levels, nextLevel, ref row);
                }

            }

        }
    }
}

