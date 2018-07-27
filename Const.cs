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
// Created On:   2018/05/31 17:47
// Modified On:  2018/06/06 15:16
// Modified By:  Alexis

#endregion




using System.Collections.Generic;
using System.Text.RegularExpressions;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;

namespace SuperMemoAssistant.Plugins.LateX
{
  public static class Const
  {
    #region Constants & Statics

    public static LateXTag FullTag => new LateXTag
    {
      LateXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
",
      LateXEnd = @"
\end{document}",
      TagBegin = "[latex]",
      TagEnd   = "[/latex]"
    };
    public static LateXTag ExpTag => new LateXTag
    {
      LateXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
$
",
      LateXEnd = @"
$
\end{document}",
      TagBegin = "[$]",
      TagEnd   = "[/$]"
    };
    public static LateXTag MathsTag => new LateXTag
    {
      LateXBegin = @"\documentclass[12pt]{article}
\special{papersize=3in,5in}
\usepackage[utf8]{inputenc}
\usepackage{amssymb,amsmath}
\pagestyle{empty}
\setlength{\parindent}{0in}
\begin{document}
\begin{displaymath}
",
      LateXEnd = @"
\end{displaymath}
\end{document}",
      TagBegin = "[$$]",
      TagEnd   = "[/$$]"
    };

    public static LateXCfg Default => new LateXCfg
    {
      DviGenerationCmd   = new List<string> { "latex", "-interaction=nonstopmode", "{inTex}" },
      ImageGenerationCmd = new List<string> { "dvipng", "-D", "200", "-T", "tight", "{inDvi}", "-o", "{outImg}.png" },
      Tags = new Dictionary<string, LateXTag>
      {
        { "Full", FullTag },
        { "Expression", ExpTag },
        { "Maths", MathsTag }
      }
    };

    #endregion




    public static class RE
    {
      #region Constants & Statics

      public static readonly Regex Br = new Regex("<br( \\t)*/>",
                                                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex DivOpen =
        new Regex("<div[ \\t]*>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex DivClose =
        new Regex("<div[ \\t]*/>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex Html =
        new Regex("<[^>]+>|&nbsp;",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public static readonly Regex LatexImage =
        new Regex(@"<img[^>]+class=""?sma_latex_img""?[^>]*>",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex LatexImageFileId =
        new Regex(@"data-file-id=""?([\d]+)""?",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);
      public static readonly Regex LatexImageFilePath =
        new Regex(@"src=""file:///([^""]+)""",
                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public static readonly Regex LatexError =
        new Regex(
          "(?:[\\s]*<br[\\s]*/>|<br[\\s]*>)*<div class=\"?sma_latex_error\"?><font color=\"?#ff0000\"?>.*?</font></div>[\\s]*(?:<br[\\s]*/>|<br[\\s]*>){0,2}",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

      public const string LatexTagOpen  = @"(?:<[\s]*(p|div)[^>]*>)?";
      public const string LatexTagClose = @"(?:<[\s]*/[\s]*\1[^>]*>)?";

      #endregion
    }

    public static class Html
    {
      #region Constants & Statics

      public const string LatexImage = @"<img class=""sma_latex_img"" data-file-id=""{0}"" src=""file:///{1}"" />";
      public const string LatexError = "\n<br />\n<br /><div class=\"sma_latex_error\"><font color=\"#ff0000\">{0}</font></div>";

      public const string CSS = @"
.sma_latex_error {
  padding: 15px;
  margin-bottom: 10px;
  border: 1px solid transparent;
  border-radius: 4px;
  color: #a94442;
  background-color: #f2dede;
  border-color: #ebccd1;
}";

      #endregion
    }


    public static class Paths
    {
      #region Constants & Statics

      public static string TempFilePath =>
        System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                               SMAPluginBase.GetAssemblyGuid().ToString("D"));
      public static string TempTexFilePath =>
        TempFilePath + ".tex";
      public static string TempDviFilePath =>
        TempFilePath + ".dvi";

      #endregion
    }

    public static class PNG
    {
      #region Constants & Statics

      public const string LatexChunkId = "lTEx";

      #endregion
    }
  }
}
