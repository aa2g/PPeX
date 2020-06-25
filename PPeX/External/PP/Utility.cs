using System.Text;
using System.IO;

namespace PPeX.External.PP
{
    public static class Utility
    {
	    static Utility()
	    {
		    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static Encoding EncodingShiftJIS => Encoding.GetEncoding("Shift-JIS");
        public static int BufSize => 0x400000; //4096;

        public static string GetDestFile(DirectoryInfo dir, string prefix, string ext)
        {
            string dest = dir.FullName + @"\" + prefix;
            int destIdx = 0;
            while (File.Exists(dest + destIdx + ext))
            {
                destIdx++;
            }
            dest += destIdx + ext;
            return dest;
        }
    }
}