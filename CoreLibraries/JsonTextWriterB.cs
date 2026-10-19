using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace OmniApi.CoreLibraries
{
 
public class JsonTextWriterB : JsonTextWriter
{
    public string NewLine { get; set; }

    public JsonTextWriterB (TextWriter textWriter) : base(textWriter)
    {
        NewLine = Environment.NewLine;
    }

    protected override void WriteIndent ()
    {
        if (Formatting == Formatting.Indented) {
            WriteWhitespace(NewLine);
            int currentIndentCount = Top * Indentation;
            for (int i = 0; i < currentIndentCount; i++)
                WriteIndentSpace();
        }
    }


   

}
}