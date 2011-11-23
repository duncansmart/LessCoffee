using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DotSmart
{
    class ProcessUtil
    {
        public static int Exec(string filename, string args, TextReader stdIn, TextWriter stdOut, TextWriter stdErr, Encoding encoding)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo psi = process.StartInfo;
                psi.RedirectStandardError = psi.RedirectStandardInput = psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.FileName = filename;
                psi.Arguments = args;

                psi.StandardOutputEncoding = encoding;
                psi.StandardErrorEncoding = encoding;
                

                if (stdOut != null)
                {
                    process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        stdOut.WriteLine(e.Data);
                    };
                }
                if (stdErr != null)
                {
                    process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
                    {
                        stdErr.WriteLine(e.Data);
                    };
                }
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (stdIn != null)
                {
                    // There's no Process.Standard*Input*Encoding, so write specified encoding's raw bytes to base input stream
                    using (var encodedStdIn = new StreamWriter(process.StandardInput.BaseStream, encoding))
                        encodedStdIn.Write(stdIn.ReadToEnd());
                }

                process.WaitForExit();
                return process.ExitCode;
            }
        }

    }
}
