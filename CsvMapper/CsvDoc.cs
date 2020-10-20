using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CsvMapper
{
    class CsvDoc
    {
        public List<CsvRow> Data = new List<CsvRow>();
        public CsvRow FirstRow { get;  set; }
        public string FirstRowInputString { get; private set; }
        public int NumColumns { get { return FirstRow != null ? FirstRow.Fields.Count : 0; } }

        public CsvDoc(string file)
        {
            using (StreamReader sr = new StreamReader(file) )
            {
                string line = string.Empty;
                FirstRow = CsvRow.CreateRow(sr, out line);
                FirstRowInputString = line;
                line = string.Empty;
                if (NumColumns <= 1)
                {
                    throw new InvalidDataException("Csv file must have more than one column and all rows must have same number of columns");
                }
                CsvRow rdr = null;
                while((rdr = CsvRow.CreateRow(sr, out line)) != null)
                {
                    if(rdr.Fields.Count != NumColumns)
                    {
                        if(rdr.Fields.Count == 1)
                        {
                            break;
                        }
                        Console.Error.WriteLine($"Incorrectly formatted CSV line? Expecting {NumColumns} fields, found {rdr.Fields.Count}.");
                        Console.Error.WriteLine("Input: " + line);
#if DEBUG
                        System.Diagnostics.Debugger.Break();
#endif
                        line = string.Empty;
                        continue;
                    }
                    Data.Add(rdr);
                    line = string.Empty;
                }
            }
        }
        
        private CsvDoc()
        {

        }
        public void WriteToFile(string file)
        {
            using (StreamWriter sr = new StreamWriter(File.OpenWrite(file)))
            {
                sr.WriteLine(FirstRow.ToString());
                foreach(CsvRow row in Data)
                {
                    sr.WriteLine(row.ToString());
                }
            }
        }

        public CsvDoc TransformFromMapFile(string xmlMapFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlMapFile);
            if(doc.DocumentElement.Name != "CsvMap")
            {
                throw new InvalidDataException("Not a CsvMap document");
            }

            CsvDoc csvDoc = new CsvDoc
            {
                FirstRow = CsvRow.CreateRow(GetFirstRow(doc))
            };

            foreach (CsvRow row in Data)
            {
                csvDoc.Data.Add(TransformRow(row, doc));
            }
            csvDoc = MergeRowsOn(csvDoc, doc);
            return csvDoc;
        }

        private CsvDoc MergeRowsOn(CsvDoc csvDoc, XmlDocument doc)
        {
            Dictionary<string, List<CsvRow>> rowsToMerge = new Dictionary<string, List<CsvRow>>();
            if (!doc.DocumentElement.HasAttribute("merge_on") || string.IsNullOrWhiteSpace(doc.DocumentElement.Attributes["merge_on"].Value))
            {
                return csvDoc;
            }
            int mergeFieldIndex = csvDoc.FirstRow.Fields.IndexOf(doc.DocumentElement.Attributes["merge_on"].Value);
            if (mergeFieldIndex == -1)
            {
                throw new InvalidDataException($"MergeRowsOn() failed to find index of Field {doc.DocumentElement.Attributes["merge_on"].Value} in {csvDoc.FirstRow}");
            }
            foreach (CsvRow row in csvDoc.Data)
            {
                if (rowsToMerge.Keys.Contains(row.Fields[mergeFieldIndex]))
                {
                    rowsToMerge[row.Fields[mergeFieldIndex]].Add(row);
                }
                else
                {
                    List<CsvRow> list = new List<CsvRow>();
                    list.Add(row);
                    rowsToMerge.Add(row.Fields[mergeFieldIndex], list);
                }
            }
            CsvDoc mergedDoc = new CsvDoc
            {
                FirstRow = csvDoc.FirstRow
            };

            foreach(var kvp in rowsToMerge)
            {
                mergedDoc.Data.Add(MergeRows(kvp.Value));
            }
            return mergedDoc;
        }

        private CsvRow MergeRows(List<CsvRow> value)
        {
            CsvRow first = value[0];
            if (value.Count > 1)
            {
                for (int i = 1; i < value.Count; ++i)
                {
                    for(int j = 0; j < first.Fields.Count; ++j)
                    {
                        if(first.Fields[j] != value[i].Fields[j])
                        {
                            first.Fields[j] = first.Fields[j] + "\r\r" + value[i].Fields[j];
                        }
                    }
                }
            }
            return first;
        }

        private CsvRow TransformRow(CsvRow row, XmlDocument doc)
        {
            List<string> targetFields = new List<string>();
            StringBuilder sb = new StringBuilder();
            int idx = -1;
            foreach (XmlNode node in doc.DocumentElement.ChildNodes) // Target nodes
            {
                if (node.ChildNodes.Count == 1)
                {
                    idx = FirstRow.Fields.IndexOf(node.FirstChild.InnerText);
                    if(idx == -1)
                    {
                        throw new InvalidDataException($"Failed to find field name '{node.FirstChild.InnerText}' in Headers: {FirstRowInputString}");
                    }
                    targetFields.Add(row.Fields[idx]);
                }
                else
                {
                    foreach (XmlNode n in node.ChildNodes) // Source nodes, might be more than one so we build the string
                    {
                        idx = FirstRow.Fields.IndexOf(n.InnerText);
                        if(idx == -1)
                        {
                            throw new InvalidDataException($"Failed to find field name '{node.FirstChild.InnerText}' in Headers: {FirstRowInputString}");
                        }
                        sb.Append($"{row.Fields[idx]}\r");
                    }
                    targetFields.Add(sb.ToString().Trim());
                    sb.Clear();
                }
            }
            return CsvRow.CreateRow(targetFields);
        }

        private List<string> GetFirstRow(XmlDocument doc)
        {
            List<string> fields = new List<string>();
            foreach(XmlNode n in doc.DocumentElement.ChildNodes)
            {
                fields.Add(n.Attributes["name"].Value);
            }
            return fields;
        }
    }

    class CsvRow
    {
        public List<string> Fields
        {
            get; private set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string field in Fields)
            {
                sb.Append(CsvField(field) + ",");
            }
            return sb.ToString().TrimEnd(',');
        }

        private string CsvField(string field)
        {
            field = field.Replace("\n", "\r").Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        private CsvRow(List<string> fields)
        {
            Fields = fields;
        }

        public static CsvRow CreateRow(List<string> fields)
        {
            return new CsvRow(fields);
        }
        public static CsvRow CreateRow(StreamReader sr, out string line)
        {
            return ReadCsvLine(sr, out line);
        }
        private static CsvRow ReadCsvLine(StreamReader sr, out string line)
        {
            StringBuilder sb = new StringBuilder();
            int c = -1;
            while((c = sr.Read()) != -1)
            {
                if(c == '\r')
                {
                    int next = sr.Read();
                    if(next == '\n')
                    {
                        break;
                    }
                    sb.Append((char)c);
                    sb.Append((char)next);
                    continue;
                }
                sb.Append((char)c);
            }
            line = sb.ToString();
            return ParseCsvLine(line);
        }

        private static CsvRow ParseCsvLine(string line)
        {
            List<string> list = new List<string>();
            StringBuilder sb = new StringBuilder();
            using (StringReader sr = new StringReader(line))
            {
                bool inquotes = false;
                int prev = -1, c = -1;
                while((c = sr.Read()) != -1)
                {
                    if(c == '"')
                    {
                        if (prev == -1 || (prev == ',' && !inquotes))
                        {
                            inquotes = true;
                            prev = c;
                            continue;
                        }
                        int next = sr.Read();
                        if(next == -1)
                        {
                            break;
                        }
                        if(next == ',')
                        {
                            inquotes = false;
                            list.Add(sb.ToString().Trim());
                            sb.Clear();
                            prev = next;
                            continue;
                        }
                    }
                    if(c == ',' && !inquotes)
                    {
                        prev = c;
                        list.Add(sb.ToString().Trim());
                        sb.Clear();
                        continue;
                    }
                    prev = c;
                    sb.Append((char)c);
                }
                list.Add(sb.ToString().Trim());
                return new CsvRow(list);
            }
        }
    }
}
