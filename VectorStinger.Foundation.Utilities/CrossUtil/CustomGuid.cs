using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.CrossUtil
{
    public class CustomGuid
    {
        private static Guid GUID
        {
            get
            {
                return Guid.NewGuid();
            }
        }

        public static string GetGuid()
        {
            byte[] bArr = GUID.ToByteArray();
            int autonum = BitConverter.ToInt32(bArr, 0);
            autonum = Math.Abs(autonum) + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            string _sGUID = autonum.ToString();
            return _sGUID;
        }
    }
}
