using System;
using System.IO;
using System.Xml;

namespace  PacketGenerator
{ 
    // json, xml, 기타 자체 정의, xml이 json에 비해서 hierarchy가 편하게 보여서 패킷은 xml을 선호
    class Program
    {
        static string? genPackets;
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };
            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();

                while(r.Read()) // stream 방식으로 한줄 한줄 읽음
                {
                    // XmlNodeType.Element가 시작하는것이고, XmlNodeType.EndElement는 끝나는것
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);

                    // Console.WriteLine(r.Name + " " + r["name"]);
                }

                File.WriteAllText("GenPacket.cs", genPackets);

            }
            // r.Dispose(); using문을 사용하여 알아서 호출하도록 한다.
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement) return;
            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string? packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            Tuple<string, string, string>? t = ParseMembers(r);
            if (t == null) return;
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
        }

        public static Tuple<string, string, string>? ParseMembers(XmlReader r)
        {
            string? packetName = r["name"];

            string? memberCode = "";
            string? readCode = "";
            string? writeCode = "";

            int depth = r.Depth + 1;
            while(r.Read())
            {
                if (r.Depth != depth) break;

                string? memberName = r["name"];
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false) memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false) readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false) writeCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch(memberType)
                {
                    case "bool":
                    // case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string>? t = ParseList(r);
                        if (t != null)
                        {
                            memberCode += t.Item1;
                            readCode += t.Item2;
                            writeCode += t.Item3;
                        }
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string>? ParseList(XmlReader r)
        {
            string? listName = r["name"];
            if(string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string , string, string>? t = ParseMembers(r);
            if (t == null) return null;

            string memberCode = string.Format(PacketFormat.memberListFormat, 
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                // case "byte": byte배열에서 byte배열로 변환하는걸 따로 정의할 필요 없다. 나중에 필요하면 작업
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}
