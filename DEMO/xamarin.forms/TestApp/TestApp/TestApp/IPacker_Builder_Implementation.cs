using System;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;

namespace ZYNETClientForNetCore
{

    public class IPacker_Builder_Implementation2 : IPacker
    {
        private IFodyCall ifobj;

        public IPacker_Builder_Implementation2(IFodyCall call1)
        {
          
            this.ifobj = call1;
        }

        private object _call_(int num1, Type type1, object[] objArray1)
        {
            return this.ifobj.Call(num1, (Type)type1, objArray1);
        }

        public DateTime GetTime()
        {
            return (DateTime)this._call_(0x7d2, (Type)typeof(DateTime), new object[0]);
        }

        public ResultAwatier GetTimeAsync()
        {
            return (ResultAwatier)this._call_(0x7d2, (Type)typeof(ResultAwatier), new object[0]);
        }

        public Result GetTimer()
        {
            return (Result)this._call_(0x7d2, (Type)typeof(Result), new object[0]);
        }

        public Result IsLogOn(string text1, string text2)
        {
            return (Result)this._call_(0x3e8, (Type)typeof(Result), new object[] { text1, text2 });
        }

        public void SetPassWord(string text1)
        {
            this._call_(0x7d3, (Type)typeof(void), new object[] { text1 });
        }

        public Result StartDown(string text1)
        {
            return (Result)this._call_(0x7d1, (Type)typeof(Result), new object[] { text1 });
        }

        public ResultAwatier StartDownAsync(string text1)
        {
            return (ResultAwatier)this._call_(0x7d1, (Type)typeof(ResultAwatier), new object[] { text1 });
        }

        public Result TestRec(int num1)
        {
            return (Result)this._call_(0x9c4, (Type)typeof(Result), new object[] { (int)num1 });
        }

        public int TestRec2(int num1)
        {
            return (int)((int)this._call_(0x9c4, (Type)typeof(int), new object[] { (int)num1 }));
        }

        public ResultAwatier TestRecAsync(int num1)
        {
            return (ResultAwatier)this._call_(0x9c4, (Type)typeof(ResultAwatier), new object[] { (int)num1 });
        }
    }
}

