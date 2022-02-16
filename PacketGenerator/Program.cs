using System;
using System.Xml;

namespace  PacketGenerator
{ 
    // json, xml, 기타 자체 정의, xml이 json에 비해서 hierarchy가 편하게 보여서 패킷은 xml을 선호
    class Program
    {
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

            ParseMembers(r);
        }

        public static void ParseMembers(XmlReader r)
        {
            string? packetName = r["name"];

            int depth = r.Depth + 1;
            while(r.Read())
            {
                if (r.Depth != depth) break;

                string? memberName = r["name"];
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                string MemberType = r.Name.ToLower();
                switch(MemberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
