using BigTreeCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace BigTree
{
    [DebuggerDisplay("Id:{Id}, Parent:{ParentId}, IsSys:{IsSystem}, Type:{NodeType}")]
    internal class SnContent : Node
    {
        // NodeId ParentNodeId    IsSystem NodeTypeId  Type
        public int Id { get; set; }
        public int ParentId { get; set; }
        public bool IsSystem { get; set; }
        public string Name { get; set; }
        public SnContentType ContentType { get; set; }

        private static readonly char[] _split = "\t,;".ToCharArray();

        public static SnContent Parse(string src)
        {
            // NodeId   ParentNodeId    IsSystem    NodeTypeId  Type    Name
            var result = new SnContent();

            var cols = src.Split(_split, StringSplitOptions.RemoveEmptyEntries);
            if (cols[1] == "NULL") cols[1] = "0";

            int id;
            if (!int.TryParse(cols[0], out id)) return null; result.Id = id;
            if (!int.TryParse(cols[1], out id)) return null; result.ParentId = id;
            if (!int.TryParse(cols[2], out id)) return null; result.IsSystem = id != 0;
            if (!int.TryParse(cols[3], out id)) return null; result.NodeType = id;

            result.Name = cols[5];

            return result;
        }
    }
}
