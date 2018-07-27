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
// Created On:   2018/06/02 01:04
// Modified On:  2018/06/06 13:09
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SuperMemoAssistant.Extensions;

namespace SuperMemoAssistant.Plugins.LateX
{
  public static class LateXUtils
  {
    #region Methods

    //private static List<string> FindOrphanImages(MatchCollection existing)
    //{
    //  return null;
    //}

    public static string TextToHtml(string text)
    {
      text = HttpUtility.HtmlEncode(text);
      text = text.Replace("\r\n",
                          "<br />\n");
      text = text.Replace("\n",
                          "<br />\n");
      text = text.Replace("\r",
                          "<br />\n");
      text = text.Replace("  ",
                          " &nbsp;");

      return text;
    }

    public static string HtmlToLateX(string html)
    {
      html = Const.RE.Br.Replace(html,
                                 "\\n");
      html = Const.RE.DivOpen.Replace(html,
                                      "\\n");
      html = Const.RE.DivClose.Replace(html,
                                       "");
      html = Const.RE.Html.Replace(html,
                                   "");

      return html.Trim();
    }

    public static string FixPlaceHolders(string arg)
    {
      switch (arg)
      {
        case "{inTex}":
          return Const.Paths.TempTexFilePath;

        case "{inDvi}":
          return Const.Paths.TempDviFilePath;

        default:
          return arg.StartsWith("{outImg}")
            ? arg.Replace("{outImg}",
                          Const.Paths.TempFilePath)
            : arg;
      }
    }

    public static (bool success, string pathOrError) GenerateImgFile(LateXCfg config)
    {
      string imgFilePath = FixPlaceHolders(config.ImageGenerationCmd.Last());

      if (File.Exists(imgFilePath))
        File.Delete(imgFilePath);

      var (success, output) = Execute(config.ImageGenerationCmd);

      return (success, success ? imgFilePath : output);
    }

    public static (bool success, string pathOrError) GenerateDviFile(LateXCfg config,
                                                                     LateXTag tag,
                                                                     string   latexContent)
    {
      if (File.Exists(Const.Paths.TempTexFilePath))
        File.Delete(Const.Paths.TempTexFilePath);

      if (File.Exists(Const.Paths.TempDviFilePath))
        File.Delete(Const.Paths.TempDviFilePath);

      latexContent = tag.LateXBegin + latexContent + tag.LateXEnd;

      File.WriteAllText(Const.Paths.TempTexFilePath,
                        latexContent);
      var (success, output) = Execute(config.DviGenerationCmd);

      return (success, success ? Const.Paths.TempDviFilePath : output);
    }

    public static (bool success, string output) Execute(List<string> fullCmd)
    {
      var bin  = fullCmd[0];
      var args = fullCmd.Skip(1).Select(FixPlaceHolders);

      var p = ProcessEx.CreateBackgroundProcess(bin,
                                                String.Join(" ",
                                                            args),
                                                Path.GetTempPath());

      var (exitCode, output, timedOut) = p.ExecuteBlockingWithOutputs(3000);

      return (timedOut == false && exitCode == 0, output);
    }

    #endregion
  }
}
