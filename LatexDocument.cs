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
// Created On:   2018/06/02 22:15
// Modified On:  2018/06/08 17:12
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.Medias.Images;
using SuperMemoAssistant.Sys.IO.FS;

namespace SuperMemoAssistant.Plugins.LateX
{
  public class LatexDocument
  {
    #region Properties & Fields - Non-Public

    private ISMAPlugin Plugin { get; }

    private LateXCfg Config    { get; }
    private int      ElementId { get; }
    private string   Selection { get; set; }
    private string   Html      { get; set; }

    #endregion




    #region Constructors

    public LatexDocument(
      [NotNull] ISMAPlugin plugin,
      [NotNull] LateXCfg   config,
      int                  elementId,
      [NotNull] string     html,
      string               selection = null)
    {
      Plugin    = plugin;
      Config    = config;
      ElementId = elementId;
      Html      = html;
      Selection = selection ?? html;
    }

    #endregion




    #region Methods

    public void PruneOrphanImages()
    {
      var usedFSIds = GetAllImages().Select(i => i.fileId);
      var allFSIds = Svc.CollectionFS.ForElement(ElementId,
                                                 Plugin);

      foreach (var idToRm in allFSIds.Select(f => f.Id).Except(usedFSIds))
        Svc.CollectionFS.DeleteById(idToRm);
    }

    public string ConvertImagesToLatex()
    {
      var newSelection = Selection.Clone() as string;
      var allImagesData = GetAllImages();

      foreach (var (html, fileId, filePath) in allImagesData)
      {
        var texBytes = PngChunkService.ReadCustomChunk(filePath, Const.PNG.LatexChunkId);
        var tex = Encoding.UTF8.GetString(texBytes);

        newSelection = newSelection.ReplaceFirst(html,
                               tex);
      }

      Html = Html.Replace(Selection,
                          newSelection);
      Selection = newSelection;

      return Html;
    }

    public string ConvertLatexToImages()
    {
      string newSelection = Const.RE.LatexError.Replace(Selection,
                                                        string.Empty);
      var filters = Config.Filters;

      var allTaggedMatches = filters.Select(
        f => (f.Value, f.Key.Matches(Selection))
      );

      foreach (var taggedMatches in allTaggedMatches)
      {
        Dictionary<string, int> itemsOccurences = new Dictionary<string, int>();
        var processedMatches = GenerateImages(taggedMatches.Item1,
                                              taggedMatches.Item2);

        foreach (var processedMatch in processedMatches)
        {
          var (success, imgFilePathOrError, fullHtml) = processedMatch;

          int nb = itemsOccurences.SafeGet(fullHtml,
                                           0) + 1;
          itemsOccurences[fullHtml] = nb;

          if (success)
            try
            {
              var colFile = CopyImageToCollectionFS(imgFilePathOrError);

              if (colFile == null)
                throw new InvalidOperationException("Unable to write generated image file to Collection FileSystem");

              newSelection = newSelection.ReplaceNth(fullHtml,
                                                     GenerateImgHtml(colFile),
                                                     nb);
            }
            catch (Exception ex)
            {
              success            = false;
              imgFilePathOrError = ex.Message;
            }

          if (success == false)
            newSelection = newSelection.ReplaceNth(fullHtml,
                                                   GenerateErrorHtml(fullHtml,
                                                                     imgFilePathOrError),
                                                   nb);

          nb++;
        }
      }

      Html = Html.Replace(Selection,
                          newSelection);
      Selection = newSelection;

      PruneOrphanImages();

      return Html;
    }

    private string GenerateImgHtml(CollectionFile colFile)
    {
      return String.Format(Const.Html.LatexImage,
                           colFile.Id,
                           colFile.Path);
    }

    private string GenerateErrorHtml(string html,
                                     string error)
    {
      error = LateXUtils.TextToHtml(error);

      return html + string.Format(Const.Html.LatexError,
                                  error);
    }

    private CollectionFile CopyImageToCollectionFS(string imgFilePath)
    {
      void StreamCopier(Stream outStream)
      {
        using (var inStream = File.OpenRead(imgFilePath))
          inStream.CopyTo(outStream);
      }

      string imgFileCrc32 = FileEx.GetCrc32(imgFilePath);

      return Svc.CollectionFS.Create(Plugin,
                                     ElementId,
                                     StreamCopier,
                                     ".png",
                                     imgFileCrc32);
    }

    private IEnumerable<(bool success, string imgFilePathOrError, string fullHtml)> GenerateImages(
      LateXTag        tag,
      MatchCollection matches)
    {
      List<(bool, string, string)> ret = new List<(bool, string, string)>();

      foreach (Match match in matches)
      {
        string fullHtml = match.Groups[0].Value;
        string texHtml  = match.Groups[1].Value;

        try
        {
          string latex = LateXUtils.HtmlToLateX(texHtml);

          var (success, pathOrError) = LateXUtils.GenerateDviFile(Config,
                                                                  tag,
                                                                  latex);

          if (success == false)
          {
            ret.Add((false, pathOrError, fullHtml));
            continue;
          }

          (success, pathOrError) = LateXUtils.GenerateImgFile(Config);

          if (success)
            PngChunkService.WriteCustomChunk(
              pathOrError,
              null,
              Const.PNG.LatexChunkId,
              Encoding.UTF8.GetBytes(tag.SurroundTexWith(latex))
            );

          ret.Add((success, pathOrError, fullHtml));
        }
        catch (Exception ex)
        {
          ret.Add((false, ex.Message, fullHtml));
        }
      }

      return ret;
    }

    private HashSet<(string html, int fileId, string filePath)> GetAllImages()
    {
      HashSet<(string, int, string)> ret     = new HashSet<(string, int, string)>();
      var                    matches = Const.RE.LatexImage.Matches(Selection);

      foreach (Match imgMatch in matches)
      {
        var html          = imgMatch.Groups[0].Value;
        var fileIdMatch   = Const.RE.LatexImageFileId.Match(html);
        var filePathMatch = Const.RE.LatexImageFilePath.Match(html);

        if (fileIdMatch.Success && filePathMatch.Success)
        {
          int    fileId   = int.Parse(fileIdMatch.Groups[1].Value);
          string filePath = filePathMatch.Groups[1].Value;

          ret.Add((html, fileId, filePath));
        }
      }

      return ret;
    }

    #endregion
  }
}
