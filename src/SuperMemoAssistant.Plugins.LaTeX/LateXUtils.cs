#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2020/03/29 00:21
// Modified On:  2020/04/06 19:00
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Plugins.LaTeX
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Web;
  using Anotar.Serilog;
  using Extensions;

  public static class LaTeXUtils
  {
    #region Methods

    public static string TextToHtml(string text)
    {
      text = HttpUtility.HtmlEncode(text);
      text = text.Replace("\r\n", "<br />\n");
      text = text.Replace("\n", "<br />\n");
      text = text.Replace("\r", "<br />\n");
      text = text.Replace("  ", " &nbsp;");

      return text;
    }

    public static string PlainText(string html)
    {
      html = LaTeXConst.RE.Br.Replace(html, "\\n");
      html = LaTeXConst.RE.DivOpen.Replace(html, "\\n");
      html = LaTeXConst.RE.DivClose.Replace(html, "");
      html = LaTeXConst.RE.Html.Replace(html, "");

      return WebUtility.HtmlDecode(html.Trim());
    }

    public static string GetPlaceholderValue(string arg)
    {
      switch (arg)
      {
        case "{inTex}":
          return LaTeXConst.Paths.TempTexFilePath;

        case "{inDvi}":
          return LaTeXConst.Paths.TempDviFilePath;

        default:
          return arg.StartsWith("{outImg}", StringComparison.InvariantCultureIgnoreCase)
            ? arg.Replace("{outImg}", LaTeXConst.Paths.TempFilePath)
            : arg;
      }
    }

    public static (bool success, string pathOrError) GenerateImgFile(LaTeXCfg config)
    {
      string imgFilePath =
        GetPlaceholderValue(
          config.ImageGenerationCmd.FirstOrDefault(s => s.StartsWith("{outImg}", StringComparison.InvariantCultureIgnoreCase)));

      if (File.Exists(imgFilePath))
        File.Delete(imgFilePath);

      var (success, output) = Execute(config.ImageGenerationCmd, config.ExecutionTimeout);

      // Log for debugging purpose
      LogTeXLogs();

      return (success, success ? imgFilePath : output);
    }

    public static (bool success, string pathOrError) GenerateDviFile(LaTeXCfg config,
                                                                     LaTeXTag tag,
                                                                     string   latexContent)
    {
      // Make sure to delete existing .tex, .dvi or .log file to start with a clean slate
      if (File.Exists(LaTeXConst.Paths.TempTexFilePath))
        File.Delete(LaTeXConst.Paths.TempTexFilePath);

      if (File.Exists(LaTeXConst.Paths.TempDviFilePath))
        File.Delete(LaTeXConst.Paths.TempDviFilePath);

      if (File.Exists(LaTeXConst.Paths.TexErrorLog))
        File.Delete(LaTeXConst.Paths.TexErrorLog);

      // Build .tex file content and write it
      latexContent = tag.LaTeXBegin + latexContent + tag.LaTeXEnd;

      File.WriteAllText(LaTeXConst.Paths.TempTexFilePath, latexContent);

      // Log for debugging purpose
      LogTo.Verbose(
        "Wrote TeX file {TempTexFilePath}:\r\n\r\n-------------------------------------------------\r\n{LatexContent}\r\n-------------------------------------------------\r\n",
        LaTeXConst.Paths.TempTexFilePath, latexContent);

      // Build the .dvi from the .tex file using dvi2png or another processor depending on user's config
      var (success, output) = Execute(config.DviGenerationCmd, config.ExecutionTimeout);

      // Log for debugging purpose
      LogTeXLogs();

      return (success, success ? LaTeXConst.Paths.TempDviFilePath : output);
    }

    public static void LogTeXLogs()
    {
      if (File.Exists(LaTeXConst.Paths.TexErrorLog))
      {
        var texLog = File.ReadAllText(LaTeXConst.Paths.TexErrorLog);
        LogTo.Verbose(
          "Content of texput.log:\r\n\r\n-------------------------------------------------\r\n{TexLog}\r\n-------------------------------------------------\r\n",
          texLog);
      }
    }

    public static (bool success, string output) Execute(List<string> fullCmd, int timeout)
    {
      var bin     = fullCmd[0];
      var args    = fullCmd.Skip(1).Select(cmd => GetPlaceholderValue(cmd).Quotify()).ToList();
      var argsStr = string.Join(" ", args);

      LogTo.Debug($"Executing: {bin} {argsStr}.");

      var p = ProcessEx.CreateBackgroundProcess(bin, argsStr, Path.GetTempPath());

      var (exitCode, output, timedOut) = p.ExecuteBlockingWithOutputs(300000);

      LogTo.Debug(
        $"Execution result:\r\nExit code: {exitCode}\r\nTimeout?: {timedOut}\r\nOutput:\r\n\r\n-------------------------------------------------\r\n{output}\r\n-------------------------------------------------\r\n");

      return (timedOut == false && exitCode == 0, output);
    }

    #endregion
  }
}
