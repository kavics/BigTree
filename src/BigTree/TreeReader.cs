using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTree
{
    public class TreeReader<T> : IEnumerable<T>
    {
        private TextReader _baseReader;
        private Func<string, T> _parserMethod;
        public TreeReader(TextReader baseReader, Func<string, T> parserMethod)
        {
            _baseReader = baseReader;
            _parserMethod = parserMethod;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            string line;
            T item;
            while (true)
            {
                if ((line = _baseReader.ReadLine()) == null)
                    break;
                if ((item = _parserMethod(line)) != null)
                    yield return item;
            }
        }
    }
}
