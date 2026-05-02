using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using codessentials.CGM.Classes;
using codessentials.CGM.Elements;
using codessentials.CGM.Export;

namespace codessentials.CGM.Commands
{
    /// <summary>
    /// Class=9, ElementId=1
    /// </summary>
    public class ApplicationStructureAttribute : Command
    {
        public string AttributeType { get; private set; }
        public StructuredDataRecord Data { get; private set; }
        public IReadOnlyList<LinkUriElement> GetLinkUris()
        {
            if (!string.Equals(AttributeType, "linkuri", StringComparison.OrdinalIgnoreCase))
                return [];

            var result = new List<LinkUriElement>();
            // First try: decoded SDR (ideal case)
            if (Data?.Members is not null)
            {
                foreach (var member in Data.Members.Where(m=>m.Type is StructuredDataRecord.StructuredDataType.S
                        or StructuredDataRecord.StructuredDataType.SF))
                {
                    var values = member.Data.OfType<string>().ToList();

                    if (values.Count >= 1)
                    {
                        result.Add(new LinkUriElement(
                            destination: values[0],
                            title: values.Count > 1 ? values[1] : null,
                            behavior: values.Count > 2 ? values[2] : null
                        ));

                    }
                }

                if (result.Count > 0)
                    return result;
            }

            // Fallback: clear-text serialization (lossless)
            using var stream = new MemoryStream();

            using (var writer = new DefaultClearTextWriter(stream))
            {
                WriteAsClearText(writer);
            }

            stream.Position = 0;

            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            var text = encoding.GetString(stream.ToArray());

            var quoted = Regex.Matches(text, @"'([^']*)'", RegexOptions.Singleline)
                              .Cast<Match>()
                              .Select(m => m.Groups[1].Value)
                              .ToList();

            // quoted[0] == "linkuri" means: no SDR payload, no destination
            if (quoted.Count >= 1 &&
                !string.Equals(quoted[0], "linkuri", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(new LinkUriElement(
                    destination: quoted[0],
                    title: quoted.Count > 1 ? quoted[1] : null,
                    behavior: quoted.Count > 2 ? quoted[2] : null
                ));
            }

            return result;
        }

        public ApplicationStructureAttribute(CgmFile container)
            : base(new CommandConstructorArguments(ClassCode.ApplicationStructureDescriptorElements, 1, container))
        {
           
        }

        public ApplicationStructureAttribute(CgmFile container, string attributeType, StructuredDataRecord sdr)
            :this(container)
        {
            AttributeType = attributeType;
            Data = sdr;
        }

        public override void ReadFromBinary(IBinaryReader reader)
        {
            AttributeType = reader.ReadFixedString();
            Data = reader.ReadSDR();            
        }

        public override void WriteAsBinary(IBinaryWriter writer)
        {
            writer.WriteFixedString(AttributeType);
            writer.WriteSDR(Data);
        }

        public override void WriteAsClearText(IClearTextWriter writer)
        {
            writer.Write($" APSATTR {WriteString(AttributeType)} ");

            if (Data is not null)
            {
                WriteSDR(writer, Data);
            }

            writer.WriteLine(";");
        }
    }
}
