using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree
{
    [DebuggerDisplay("Id:{Id}, Parent:{ParentId}, Name:{Name}")]
    class SnContentType : Node
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }

        private static readonly char[] _split = "\t,;".ToCharArray();

        public static SnContentType Parse(string src)
        {
            // PropertySetId	ParentId	Name
            var result = new SnContentType();

            var cols = src.Split(_split, StringSplitOptions.RemoveEmptyEntries);
            if (cols[1] == "NULL") cols[1] = "0";

            int id;
            if (!int.TryParse(cols[0], out id)) return null; result.Id = id;
            if (!int.TryParse(cols[1], out id)) return null; result.ParentId = id;
            result.Name = cols[2];

            return result;
        }

        internal bool IsInstanceOrDerivedFrom(string contentTypeName)
        {
            var contentType = this;
            while (contentType != null)
            {
                if (Name == contentTypeName)
                    return true;
                contentType = (SnContentType)contentType.Parent;
            }
            return false;
        }
    }
}
