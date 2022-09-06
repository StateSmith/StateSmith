using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace StateSmithTest
{

    //https://stackoverflow.com/a/47529356
    public class ConsoleCaptureConverter : TextWriter
    {
        ITestOutputHelper _output;
        public ConsoleCaptureConverter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }
    }

}
