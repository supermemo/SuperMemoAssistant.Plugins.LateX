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
// Created On:   2018/05/30 17:20
// Modified On:  2018/12/31 00:56
// Modified By:  Alexis

#endregion




using System;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Components.Controls;
using SuperMemoAssistant.Interop.SuperMemo.Core;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.LateX
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class LateXPlugin : SMAPluginBase<LateXPlugin>
  {
    #region Constructors

    public LateXPlugin() { }

    #endregion




    #region Properties & Fields - Public

    public LateXCfg Config { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "LaTeX";

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void OnInit()
    {
      LoadConfigOrDefault();

      Svc.SMA.UI.ElementWindow.OnElementChanged += new ActionProxy<SMDisplayedElementChangedArgs>(OnElementChanged);
      Svc.KeyboardHotKey.RegisterHotKey(
        new HotKey(true,
                   true,
                   false,
                   false,
                   Key.L,
                   "LateX: Convert LateX to Image"),
        ConvertLatexToImages);
      Svc.KeyboardHotKey.RegisterHotKey(
        new HotKey(true,
                   true,
                   true,
                   false,
                   Key.L,
                   "LateX: Convert Image to LateX"),
        ConvertImagesToLatex);
    }

    #endregion




    #region Methods

    // TODO: Check exception if element changed inbetween
    public void OnElementChanged(SMDisplayedElementChangedArgs e)
    {
      try
      {
        return;

        // TODO: Improve pruning
        var (texDoc, htmlDoc) = GetDocuments();

        if (texDoc == null || htmlDoc == null)
          return;

        texDoc.PruneOrphanImages();
      }
      catch (Exception) { }
    }

    private void ConvertLatexToImages()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertLatexToImages();
    }

    private void ConvertImagesToLatex()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertImagesToLatex();
    }

    private (LatexDocument texDoc, IControlHtml ctrlHtml) GetDocuments()
    {
      IControlHtml ctrlHtml = Svc.SMA.UI.ElementWindow.ControlGroup.FocusedControl.AsHtml();

      if (ctrlHtml == null)
        return (null, null);

      int    elementId = Svc.SMA.UI.ElementWindow.CurrentElementId;
      string html      = ctrlHtml.Text ?? string.Empty;

      var texDoc = new LatexDocument(Config,
                                     elementId,
                                     html);

      return (texDoc, ctrlHtml);
    }

    private void LoadConfigOrDefault()
    {
      Config = Svc<LateXPlugin>.Configuration.Load<LateXCfg>().Result;

      if (Config == null || Config.IsValid() == false)
      {
        Config = LaTeXConst.Default;

        Svc<LateXPlugin>.Configuration.Save(Config);
      }
    }

    #endregion
  }
}
