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
// Modified On:  2019/02/28 20:52
// Modified By:  Alexis

#endregion




using System.Windows;
using System.Windows.Input;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;

namespace SuperMemoAssistant.Plugins.LaTeX
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  public class LaTeXPlugin : SentrySMAPluginBase<LaTeXPlugin>
  {
    #region Constructors

    public LaTeXPlugin() { }

    #endregion




    #region Properties & Fields - Public

    public LaTeXCfg Config { get; set; }

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "LaTeX";

    public override bool HasSettings => true;

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {
      LoadConfigOrDefault();
      //SettingsModels = new List<INotifyPropertyChangedEx>
      //{
      //  Config
      //};

      Svc.HotKeyManager
         .RegisterGlobal(
           "LaTeXToImage",
           "Convert LaTeX to Image",
           new HotKey(Key.L, KeyModifiers.CtrlAlt),
           ConvertLaTeXToImage
         )
         .RegisterGlobal(
           "ImageToLaTeX",
           "Convert Image to LaTeX",
           new HotKey(Key.L, KeyModifiers.CtrlAltShift),
           ConvertImageToLaTeX
         );
    }
    
    /// <inheritdoc />
    public override void ShowSettings()
    {
      Application.Current.Dispatcher.Invoke(
        () => new ConfigurationWindow(HotKeyManager.Instance, Config).ShowAndActivate()
      );
    }

    #endregion




    #region Methods

    private void ConvertLaTeXToImage()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertLaTeXToImages();
    }

    private void ConvertImageToLaTeX()
    {
      var (texDoc, htmlDoc) = GetDocuments();

      if (texDoc == null || htmlDoc == null)
        return;

      htmlDoc.Text = texDoc.ConvertImagesToLaTeX();
    }

    private (LaTeXDocument texDoc, IControlHtml ctrlHtml) GetDocuments()
    {
      IControlHtml ctrlHtml = Svc.SMA.UI.ElementWindow.ControlGroup.FocusedControl.AsHtml();

      if (ctrlHtml == null)
        return (null, null);

      string html = ctrlHtml.Text ?? string.Empty;

      var texDoc = new LaTeXDocument(Config,
                                     html);

      return (texDoc, ctrlHtml);
    }

    private void LoadConfigOrDefault()
    {
      Config = Svc.Configuration.Load<LaTeXCfg>().Result;

      if (Config == null || Config.IsValid() == false)
      {
        Config = LaTeXConst.Default;

        Svc.Configuration.Save(Config).ConfigureAwait(false);
      }
    }

    #endregion
  }
}
