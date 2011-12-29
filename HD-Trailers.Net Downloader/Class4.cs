using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nfo.Movie;
using Serializer_IO;

namespace Nfo.File
{
    class NfoFile
    {
//       public NfoTV getNfoTV(String NfoPath)
//        {
//            NfoTV nf;
//            Serializer s = new Serializer(NfoPath, new NfoTV());
//            nf = (NfoTV)s.FromFile();
//           return nf;
//        }

        public NfoMovie getNfoMovie(String NfoPath)
        {
            NfoMovie nf;
            Serializer s = new Serializer(NfoPath, new NfoMovie());
            nf = (NfoMovie)s.FromFile();
            return nf;
        }

//        public bool saveNfoTV(NfoTV nf, String NfoPath)
//        {
//            Serializer s = new Serializer(NfoPath, nf);
//            return s.ToFile();
//        }

        public bool saveNfoMovie(NfoMovie nf, String NfoPath)
        {
            Serializer s = new Serializer(NfoPath, nf);
            return s.ToFile();
        }
    }
}
