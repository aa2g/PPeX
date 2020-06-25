using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PPeX.External.PP;

namespace PPeXTests
{
    [TestClass]
    public class PPTests
    {
        [TestMethod]
        public void PPCryptoTest()
        {
	        var parser = new ppParser(@"G:\HDD 1\AA2\Pure\Play\data\jg2p09_07_00.pp");

	        foreach (ppSubfile file in parser.Subfiles)
	        {
                using (var stream = file.CreateReadStream())
					stream.CopyTo(Stream.Null);
	        }
        }
    }
}