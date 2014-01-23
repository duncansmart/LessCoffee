using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DotSmart
{
    class ProcessUtil
    {
        public static int Exec(string filename, string args, TextReader stdIn = null, TextWriter stdOut = null, TextWriter stdErr = null, Encoding encoding = null, string workingDirectory = null)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo psi = process.StartInfo;
                psi.RedirectStandardError = psi.RedirectStandardInput = psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.FileName = filename;
                psi.Arguments = args;
                if (workingDirectory != null)
                    psi.WorkingDirectory = workingDirectory;

                if (encoding != null)
                {
                    psi.StandardOutputEncoding = encoding;
                    psi.StandardErrorEncoding = encoding;
                }

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
                    if (encoding != null)
                    {
                        // There's no Process.Standard*Input*Encoding, so write specified encoding's raw bytes to base input stream
                        using (var encodedStdIn = new StreamWriter(process.StandardInput.BaseStream, encoding))
                            encodedStdIn.Write(stdIn.ReadToEnd());
                    }
                    else
                    {
                        using (process.StandardInput)
                            process.StandardInput.Write(stdIn.ReadToEnd());
                    }
                }

                process.WaitForExit();
                return process.ExitCode;
            }
        }

    }
}
