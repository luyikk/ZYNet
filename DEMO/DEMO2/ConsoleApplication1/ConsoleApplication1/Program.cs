using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf.Meta;
using System.IO;

namespace ConsoleApplication1
{
    //[ProtoBuf.ProtoContract(ImplicitFields =ProtoBuf.ImplicitFields.AllPublic)]
    public class A
    {
        public string name { get; set; }
        public int X { get; set; }
    }




    class Program
    {
        static void Main(string[] args)
        {
            A test = new A()
            {
                name = "sdfsdf",
                X = 1
            };

            byte[] data = null;

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, test);

                data = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(data))
            {
                var x= ProtoBuf.Serializer.Deserialize<A>(stream);

                Console.WriteLine();
            }

            byte[] datax=  Serialization.PackSingleObject(typeof(bool), true);

            Console.WriteLine();
        }
    }
}
