using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Xml;
using Ionic.Zip;
using System.Text.RegularExpressions;



namespace PdfToHtmlJS
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdfDir = args[0].ToString();
          //  pdfDir= @"D:\UBUNTU\";
            var pdfFiles = new DirectoryInfo(pdfDir).GetFiles("*.pdf");

            var hDPI = args[1].ToString();
            var imageFormat = args[2].ToString();
            var fontFormat = args[3].ToString();
            int totalPages = 0;
            if (args.Length > 4 &&  args[4] != null)
                totalPages = Convert.ToInt16( args[4]);
           
            var packageEpub = AppDomain.CurrentDomain.BaseDirectory + "\\package_template\\";

            foreach (var pdfFile in pdfFiles)
            {
                var htmlName = Path.GetFileNameWithoutExtension(pdfFile.FullName) +".html";
                var destDir = Path.GetDirectoryName(pdfFile.FullName) + "\\" + Path.GetFileNameWithoutExtension(htmlName);
                string targetDir = Path.GetDirectoryName(pdfFile.FullName) + "\\" + Path.GetFileNameWithoutExtension(htmlName) + "_final";


                var cmd = "cd /D " + pdfDir + " & " + AppDomain.CurrentDomain.BaseDirectory + "PdfJS\\pdf2htmlEX "
                    + pdfFile.Name + " "
                    + htmlName
                    + " --dest-dir=\"" + destDir + "\""                    
                    + " --hdpi=" + hDPI
                    + " --vdpi=" + hDPI
                    + " --bg-format=\"" + imageFormat + "\""
                    + " --embed-css=0"
                    + " --embed-javascript=0"
                    + " --embed-image=0"
                    + " --embed-font=0"
                    + " --tounicode=0"
                    + " --font-format=\"" + fontFormat + "\""
                    + " --split-pages=1"
                   + " --correct-text-visibility=1"
                   + " --zoom=1.25";
                // + " --heps=5"
                // + " --veps=5"
                // + " --process-nontext=0";
                // + " --font-size-multiplier=6"
                // + " --process-type3=1";
                // + " --auto-hint=1";
                // + " --optimize-text=1"
                // + " --decompose-ligature=1"
                // + " --printing=0"
                // + " --fallback=1";

                if (totalPages > 0)
                    cmd = cmd + " --last-page=" + totalPages;


                if (Directory.Exists(destDir))
                    Utilities.DeleteDirectory(destDir);

                // converting PDF to html
                ExecuteCommandSync(cmd);


                if (Directory.Exists(targetDir))
                    Utilities.DeleteDirectory(targetDir);

                Utilities.CopyDirectory(destDir, targetDir);

                // copying epub template structure
                CopyDirectory(packageEpub, targetDir);
                MoveAssests(targetDir);

                ChangeHtmltoXhtml(targetDir);
                //CreateTocNcxFile(destDir);
                CreateNavXhtmlFile(targetDir);
                CreateOpfFile(targetDir, Path.GetFileNameWithoutExtension(pdfFile.FullName));
                CreateEpubPackage(Path.GetFileNameWithoutExtension(pdfFile.FullName), targetDir);

            }

            //+".html";
            //var destDir = Path.GetDirectoryName(pdfDir) +"\\" + htmlName ; 

        }

      

        public static void Copy(string destDir, string tarDir) 
        {

            if (!Directory.Exists(tarDir))
            {
                Directory.CreateDirectory(tarDir);
            }
            foreach (var srcPath in Directory.GetFiles(destDir))
            {
                //Copy the file from sourcepath and place into mentioned target path, 
                //Overwrite the file if same file is exist in target path
                File.Copy(srcPath, srcPath.Replace(destDir, tarDir), true);
            }
        
        }



        public static void DeleteDirectory(string targetDir)
        {
            try
            {
                var directory = new DirectoryInfo(targetDir);
                //foreach (FileInfo file in directory.GetFiles()) file.Delete();
                //foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
                directory.Delete(true);
            }
            catch (IOException)
            {
                Thread.Sleep(5);
                DeleteDirectory(targetDir);
            }
        }
       
/// <summary>
/// rename .page to .html with some stuff.
/// </summary>
/// <param name="destDir"></param>
        public static void ChangeHtmltoXhtml(string destDir)
        {
            if (Directory.Exists(destDir))
            {
                var doc = new HtmlDocument();
                doc.OptionWriteEmptyNodes = true;
                var html =new  DirectoryInfo(destDir).GetFiles("*.html").First();
                
                if (html!=null)
                {
                    doc.Load(html.FullName,Encoding.UTF8);
                    var htmlAdd = doc.DocumentNode.SelectSingleNode("//html");
                    htmlAdd.SetAttributeValue("xmlns:epub", "http://www.idpf.org/2007/ops");
                    htmlAdd.SetAttributeValue("xml:lang", "en-US");

                   
                    var metaInfo = doc.CreateElement("meta");

                    string widthP = CssParserold.GetElementStyle(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css", "w0");
                    var pageWidth = widthP.Split(':')[1].Split(';')[0].Split('.')[0];
                    string f = destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name);
                    string heightP = CssParserold.GetElementStyle(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css", "h0");
                    var pageHeight = heightP.Split(':')[1].Split(';')[0].Split('.')[0];

                   string charSet =  metaInfo.GetAttributeValue("charset", null);
                   metaInfo.SetAttributeValue("name", "viewport");
                    metaInfo.SetAttributeValue("content", "width=" + pageWidth + ", height=" + pageHeight);
                    //doc.DocumentNode.SelectSingleNode("//meta[@charset='utf-8'").InsertAfter(metaInfo);
                    doc.DocumentNode.SelectSingleNode("//head").PrependChild(metaInfo);

                    var xmlNode = HtmlNode.CreateNode("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    doc.DocumentNode.PrependChild(xmlNode);
                    var script= doc.DocumentNode.Descendants().Where(n => n.Name == "script");// //removing all redundant script tags
                    if(script!=null)
                        script.ToList().ForEach(n => n.Remove());
                   
                   var fancyLink= doc.DocumentNode.SelectSingleNode("//link[@href='fancy.min.css']");
                   if (fancyLink != null)
                       fancyLink.Remove();
                   if (File.Exists(destDir + "\\OEBPS\\css\\fancy.min.css"))
                       File.Delete(destDir + "\\OEBPS\\css\\fancy.min.css");

                    var links=doc.DocumentNode.Descendants().Where(n => n.Name == "link");
                    if (links != null)
                    {
                        links.ToList().ForEach(l => { 
                            l.SetAttributeValue("href", "css/" + l.GetAttributeValue("href", null));
                        }); 
                    }

                  
                   
                    
                    var extra=doc.DocumentNode.SelectSingleNode("//div[@id='outline']");
                    if (extra != null)
                        extra.Remove();
                    
                    var sidebr = doc.DocumentNode.SelectSingleNode("//div[@id='sidebar']");
                    if (sidebr != null)
                        sidebr.Remove();

                    var png = doc.DocumentNode.SelectSingleNode("//div[@class='loading-indicator']");
                    if (png != null)
                        png.Remove();

                    var exMeta = doc.DocumentNode.SelectSingleNode("//meta[@http-equiv='X-UA-Compatible']");
                    if (exMeta != null)
                        exMeta.Remove();
                    var exMetaOther = doc.DocumentNode.SelectSingleNode("//meta[@name='generator']");
                    if (exMetaOther != null)
                        exMetaOther.Remove();

                    var pageFiles=new DirectoryInfo(destDir).GetFiles("*.PAGE");
                    int pageCounter = 1;
                    foreach (var pageFile in pageFiles)
                    {
                        var pageDoc = new HtmlDocument();
                        pageDoc.Load(pageFile.FullName, Encoding.UTF8);
                        var imgs = pageDoc.DocumentNode.SelectNodes("//img");
                        if(imgs!=null)
                            imgs.ToList().ForEach(i=>i.SetAttributeValue("src","images/"+ i.GetAttributeValue("src",null)));
                       
                       var container= doc.DocumentNode.SelectSingleNode("//div[@id='page-container']");
                       if (container != null)
                       {
                           container.RemoveAllChildren();
                           container.AppendChild(pageDoc.DocumentNode);
                           if (HtmlNode.ElementsFlags.ContainsKey("img"))
                           {
                               HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
                           }

                           pageCounter = Int32.Parse(Path.GetFileNameWithoutExtension(pageFile.Name).Replace(Path.GetFileNameWithoutExtension(html.Name), ""));

                            string newFileName = "page" + pageCounter.ToString().PadLeft(4, '0') + ".xhtml";
                            doc.DocumentNode.SelectSingleNode("//title").InnerHtml = newFileName;
                            
                           //RemoveEmptySpans(doc, destDir);
                           SaveTargetNameHtmlFile(destDir + "\\OEBPS\\" + newFileName, doc);
                       }
                    }
                    if (pageFiles.Count() > 0)
                    {
                        pageFiles.ToList().ForEach(p => File.Delete(p.FullName));
                    }

                    if(File.Exists(html.FullName))
                        File.Delete(html.FullName);
                    
                    var cssText = File.ReadAllText(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css");
                    File.WriteAllText(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css", cssText.Replace("src:url(f", "src:url(../fonts/f"));

                    var baseCssText = File.ReadAllText(destDir + "\\OEBPS\\css\\base.min.css");

                    baseCssText = CssParser.RemoveCssClassByName(baseCssText, "media screen");
                    baseCssText = CssParser.RemoveCssClassByName(baseCssText, "media print");
                    baseCssText = CssParser.RemoveCssClassByName(baseCssText, "sidebar");
                    //baseCssText = CssParser.CheckWellFormed(baseCssText);
                    
                    File.WriteAllText(destDir + "\\OEBPS\\css\\base.min.css", baseCssText);

                    var baseText =File.ReadAllText(destDir + "\\OEBPS\\css\\base.min.css");
                    
                    //var bg = baseText += "body{background-color:#808080;margin:0px;}";
                    
                    File.WriteAllText(destDir + "\\OEBPS\\css\\base.min.css", baseText.Replace("unicode-bidi:bidi-override;", "").Replace("unicode-bidi:bidi-override", ""));
                    //File.WriteAllText(destDir + "\\OEBPS\\css\\base.min.css",baseText.Replace("@media print" , ""));
                    //string extraSS = CssParser.GetElementStyle(destDir + "\\OEBPS\\css\\base.min.css", "body").ToString();
                }
            }
        }
      
           
        /// <summary>
        ///  remove extra empty spans
        /// </summary>
        /// <param name="doc">html document path</param>
        /// <param name="destDir">directory of html page</param>
        /// <returns></returns>
        private static HtmlDocument RemoveEmptySpans(HtmlDocument doc, string destDir)
        {

            var pageContainer = doc.DocumentNode.SelectSingleNode("//div[contains(@id, 'page-container')]");
            if (pageContainer == null) return null;
            var textDivHTML = pageContainer.InnerHtml;
            if (string.IsNullOrEmpty(textDivHTML)) return null;
            
            
            //var doc = new HtmlDocument();
            //  doc.OptionWriteEmptyNodes = true;
            var html = new DirectoryInfo(destDir).GetFiles("*.html").First();

            // string allText = File.ReadAllText(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css").ToString();
            //// string nh = @"[^\s_[0-9]*]";
            //   string nh = @"^_d*";

            // Regex regex1 = new Regex(nh, RegexOptions.IgnoreCase);
            // Match match = regex1.Match(allText);
         
            List<string> xpaths = new List<string>();
           // var elementsList = rdoc.DocumentNode.SelectNodes("//div[contains(@class, 'pc')]").Elements().ToList();
            var spans = doc.DocumentNode.SelectNodes("//span");
            if (spans == null)
                return doc;

            var elementsList = spans.ToList();
            if (elementsList != null)
            {

                foreach (var pnode in elementsList)
                {
                    var childElements = pnode.GetAttributeValue("class", null);

                    if (childElements != null && childElements.Contains('_'))
                    {
                        var arrSpanClasses = childElements.Split(' ');
                        string strSpanClass = "";
                        if (arrSpanClasses.Length > 1)
                        {
                            strSpanClass = arrSpanClasses[1];

                            string leftwidth = CssParserold.GetElementStyle(destDir + "\\OEBPS\\css\\" + Path.GetFileNameWithoutExtension(html.Name) + ".css", strSpanClass);
                            string emptyWidth = leftwidth.Split(';')[1].Split(':')[1].Split('p')[0];
                            var roundOff = Convert.ToInt32(double.Parse(emptyWidth));

                            if (roundOff >= -4 && roundOff <= 4)
                            {
                                pnode.Remove();
                            }


                        }
                    }
                }
            }
            return doc; 
               }

       /// <summary>
       /// To save targetHtml file.
       /// </summary>
       /// <param name="targetPageFile"></param>
       /// <param name="htmlDoc"></param>
        private static void SaveTargetNameHtmlFile(string targetPageFile, HtmlAgilityPack.HtmlDocument htmlDoc)
        {
            var newNode = HtmlNode.CreateNode("<?xml version=\"1.0\"?>");
            if (htmlDoc.DocumentNode != null && (htmlDoc.DocumentNode.FirstChild.Name != null && htmlDoc.DocumentNode.FirstChild.Name.ToLower().Trim() == "?xml"))
                htmlDoc.DocumentNode.ReplaceChild(newNode, htmlDoc.DocumentNode.FirstChild);
            htmlDoc.Save(targetPageFile, Encoding.UTF8);
        }

        private static void CreateEpubPackage(string ePubName, string path)
        {
            try
            {
                ePubName = ePubName.Contains(".epub") ? ePubName : ePubName + ".epub";
                using (var finalEpub = new ZipFile())
                {
                    finalEpub.EmitTimesInWindowsFormatWhenSaving = false;

                    finalEpub.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                    finalEpub.AddFile(path + "\\mimetype", "");
                    finalEpub.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    finalEpub.AddDirectory(path + "\\META-INF", "META-INF");
                    finalEpub.AddDirectory(path + "\\OEBPS", "OEBPS");
                    var ePubPath = path + "\\" + ePubName;
                    finalEpub.Save(ePubPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }
/// <summary>
/// moving of css,img folders to follow the epub structure.
/// </summary>
/// <param name="destDir"></param>
        private static void MoveAssests(string destDir)
        {
            if (Directory.Exists(destDir))
            {
                var cssDir = destDir + "\\OEBPS\\css";
                var fontsDir = destDir + "\\OEBPS\\fonts";
                var imgDir = destDir + "\\OEBPS\\images";
                var dirInfo = new DirectoryInfo(destDir);
                var images = dirInfo.GetFiles().Where(i => i.Name.Contains(".png") || i.Name.Contains(".jpg"));
                var fonts = dirInfo.GetFiles().Where(i => i.Name.Contains(".woff") || i.Name.Contains(".otf"));
                var css = dirInfo.GetFiles("*.css");

                //delete un-necessary JS files
                dirInfo.GetFiles("*.js").ToList().ForEach(f=>File.Delete(f.FullName));

                images.ToList().ForEach(f => {
                    if (File.Exists(imgDir + "\\" + f.Name))
                        File.Delete(imgDir + "\\" + f.Name);
                    File.Move(f.FullName, imgDir + "\\" + f.Name); 
                });

                fonts.ToList().ForEach(f => {
                    if (File.Exists(fontsDir + "\\" + f.Name))
                        File.Delete(fontsDir + "\\" + f.Name);
                    File.Move(f.FullName, fontsDir + "\\" + f.Name);
                });

                css.ToList().ForEach(f => {
                    if (File.Exists(cssDir + "\\" + f.Name))
                        File.Delete(cssDir + "\\" + f.Name);
                    File.Move(f.FullName, cssDir + "\\" + f.Name); 
                });
            }

        
        }
/// <summary>
/// creation of folders (css,images,fonts)
/// </summary>
/// <param name="destDir"></param>
        private static void CreateRequiredDirectories(string destDir)
        {
            if(Directory.Exists(destDir))
            {
                var cssDir = destDir + "\\css";
                var fontsDir = destDir + "\\fonts";
                var imgDir = destDir + "\\images";
              
                
                if (!Directory.Exists(cssDir))
                    Directory.CreateDirectory(cssDir);
                if (!Directory.Exists(fontsDir))
                    Directory.CreateDirectory(fontsDir);
                if (!Directory.Exists(imgDir))
                    Directory.CreateDirectory(imgDir);
            }
        }

        private static void CopyDirectory(string strSource, string strDestination)
        {
            if (!Directory.Exists(strDestination))
            {
                Directory.CreateDirectory(strDestination);
            }
            var dirInfo = new DirectoryInfo(strSource);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.CopyTo(Path.Combine(strDestination, tempfile.Name), true);
            }
            DirectoryInfo[] dirctories = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in dirctories)
            {
                CopyDirectory(Path.Combine(strSource, tempdir.Name), Path.Combine(strDestination, tempdir.Name));
            }
        }
      
        
        /// <summary>
        /// upload contents in opf template
        /// </summary>
        /// <param name="packagePath"></param>
        /// <param name="titleVal"></param>
        private static void CreateOpfFile(string packagePath, string titleVal)
        {
            if (Directory.Exists(packagePath))
            {
                var mediaTypeJsonText = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MediaType\\MediaType.json");
                var mediaTypeDict = JsonConvert.DeserializeObject<List<OPFClass>>(mediaTypeJsonText).ToDictionary(x => x.id, y => y.mediaType);
                var opfPath = packagePath + "\\" + GetOpfPath(packagePath);
                var ops_oebps_path = packagePath + "\\" + GetOpfPath(packagePath).Split('/')[0];
                if (File.Exists(opfPath))
                {
                    var epubFiles = GetAllFiles(ops_oebps_path);
                    XDocument xDoc = XDocument.Load(opfPath);
                    XNamespace opf = "http://www.idpf.org/2007/opf";
                    XNamespace dc = "http://purl.org/dc/elements/1.1/";
                    XElement manifestElement = xDoc.Root.Element(opf + "manifest");
                    XElement spineElement = xDoc.Root.Element(opf + "spine");
                    manifestElement.RemoveAll();
                    int item = 0;
                    foreach (var file in epubFiles)
                    {
                        string href = file.Substring(epubFiles[0].IndexOf(GetOpfPath(packagePath).Split('/')[0]) + GetOpfPath(packagePath).Split('/')[0].Length + 1).Replace(@"\", "/");
                        string extension = Path.GetExtension(file).Substring(1);
                        if (mediaTypeDict[extension] != null)
                        {
                            XElement xNewChild = new XElement(opf + "item");
                            if(extension.Contains("xhtml")||extension.Contains("jpg")||extension.Contains("otf"))
                                xNewChild.SetAttributeValue("id", Path.GetFileNameWithoutExtension(file));// "item_" + item);
                            else
                                xNewChild.SetAttributeValue("id","item_" + ++item);

                            if(file.Contains("toc.xhtml")||file.Contains("nav.xhtml"))
                                xNewChild.SetAttributeValue("properties", "nav");


                            xNewChild.SetAttributeValue("href", href);
                            xNewChild.SetAttributeValue("media-type", mediaTypeDict[extension]);
                            xNewChild.Attributes("xmlns").Remove();
                            manifestElement.Add(xNewChild);
                            
                        }
                        if (file.Contains(".xhtml") && !file.Contains("toc.xhtml"))
                        {
                            XElement xNewChild = new XElement(opf + "itemref");
                            xNewChild.SetAttributeValue("idref", Path.GetFileNameWithoutExtension(file));
                            spineElement.Add(xNewChild);
                        }
                    }
                    XElement metaData = xDoc.Root.Element(opf + "metadata");
                    var title = metaData.Element(dc + "title");
                    title.Value = titleVal;
                    xDoc.Save(opfPath);

                }


            }
        }

  /// <summary>
  /// create nav file
  /// </summary>
  /// <param name="buildPath"></param>
        private static void CreateNavXhtmlFile(string buildPath)
        {
            try
            {
                if (Directory.Exists(buildPath))
                {
                    string OebpsPath = buildPath + "\\OEBPS";
                    if (Directory.Exists(OebpsPath))
                    {
                        StringBuilder navStr = new StringBuilder();
                        navStr = navStr.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + System.Environment.NewLine);
                        navStr = navStr.Append("<html xmlns:epub=\"http://www.idpf.org/2007/ops\" xmlns=\"http://www.w3.org/1999/xhtml\">" + System.Environment.NewLine);
                        navStr = navStr.Append("	<head>" + System.Environment.NewLine);
                        navStr = navStr.Append("		<meta charset=\"utf-8\"></meta>" + System.Environment.NewLine);
                       

                        navStr = navStr.Append("	</head>" + System.Environment.NewLine);
                        navStr = navStr.Append("	<body>" + System.Environment.NewLine);
                        navStr = navStr.Append("		<nav id=\"toc\" epub:type=\"toc\">" + System.Environment.NewLine + "<ol>" + System.Environment.NewLine);
                        new DirectoryInfo(OebpsPath).GetFiles().Where(x => x.Name != "toc.xhtml" && x.Name.Contains(".xhtml")).ToList().ForEach(f=>{
                            navStr = navStr.Append("			<li><a href=\""+f.Name+"\">"+ Path.GetFileNameWithoutExtension(f.Name)+"</a></li>" + System.Environment.NewLine);
                        });
                        navStr = navStr.Append("		</ol>"+  System.Environment.NewLine+"</nav>" + System.Environment.NewLine);
                        navStr = navStr.Append("	</body>" + System.Environment.NewLine);
                        navStr = navStr.Append("</html>" + System.Environment.NewLine);

                        using (StreamWriter writer = new StreamWriter(OebpsPath + "\\toc.xhtml"))
                        {
                            writer.Write(navStr.ToString());
                            writer.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// create toc.ncx file.
        /// </summary>
        /// <param name="buildPath"></param>
        private static void CreateTocNcxFile(string buildPath)
        {
            try
            {
                if (Directory.Exists(buildPath))
                {
                    string OebpsPath = buildPath + "\\OEBPS";
                    if (Directory.Exists(OebpsPath))
                    {
                        string[] xhtmlFiles = Directory.GetFiles(OebpsPath, "*.xhtml");
                        StringBuilder navPoints = new StringBuilder();
                        int navPnt = 0;

                        foreach (string st in xhtmlFiles)
                        {
                            string pageName = Path.GetFileNameWithoutExtension(st);
                            string xhtmlPage = Path.GetFileName(st);
                            navPoints.Append("<navPoint id=" + "\"navPoint-" + navPnt + "\"" + " playOrder=" + "\"" + navPnt + "\"" + ">" + System.Environment.NewLine);
                            navPoints.Append("    <navLabel>" + System.Environment.NewLine);
                            navPoints.Append("       <text>" + pageName + "</text>" + System.Environment.NewLine);
                            navPoints.Append("    </navLabel>" + System.Environment.NewLine);
                            navPoints.Append("    <content src=\"" + xhtmlPage + "\" />" + System.Environment.NewLine);
                            navPoints.Append("</navPoint>" + System.Environment.NewLine);
                            navPnt++;
                        }
                        StringBuilder tocStr = new StringBuilder();
                        tocStr = tocStr.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" xml:lang=\"en\" version= \"2005-1\">" + System.Environment.NewLine);
                        tocStr = tocStr.Append("	<head>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("	    <meta name=\"dtb:uid\" content=\"9780544088269\" />" + System.Environment.NewLine);
                        tocStr = tocStr.Append("		<meta name=\"dtb:depth\" content= \"1\" />" + System.Environment.NewLine);
                        tocStr = tocStr.Append("		<meta name=\"dtb:totalPageCount\" content= \"0\" />" + System.Environment.NewLine);
                        tocStr = tocStr.Append("		<meta name=\"dtb:maxPageNumber\" content= \"0\" />" + System.Environment.NewLine);
                        tocStr = tocStr.Append("	</head>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("<docTitle>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("	<text>Content</text>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("</docTitle>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("	<navMap>" + System.Environment.NewLine);
                        tocStr = tocStr.Append(navPoints + System.Environment.NewLine);
                        tocStr = tocStr.Append("	 </navMap>" + System.Environment.NewLine);
                        tocStr = tocStr.Append("</ncx>" + System.Environment.NewLine);

                        using (StreamWriter writer = new StreamWriter(OebpsPath + "\\toc.ncx"))
                        {
                            writer.Write(tocStr.ToString());
                            writer.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private static List<string> GetAllFiles(string rootPath)
        {
            List<string> filesPath = new List<string>();
            if (Directory.Exists(rootPath))
            {
                foreach (string f in Directory.GetFiles(rootPath))
                {
                    if (!f.Contains(".opf"))
                        filesPath.Add(f);
                }
                foreach (string d in Directory.GetDirectories(rootPath))
                {
                    filesPath.AddRange(GetAllFiles(d));
                }
                return filesPath;
            }
            return filesPath;
        }
/// <summary>
/// get opf path
/// </summary>
/// <param name="extractedpath"></param>
/// <returns></returns>
        private static string GetOpfPath(string extractedpath)
        {
            string path = string.Empty;
            XmlDataDocument Containerdoc = new XmlDataDocument();
            string containerPath = extractedpath + "\\META-INF\\container.xml";
            if (File.Exists(containerPath))
            {
                Containerdoc.Load(containerPath);
                XmlNodeList rootfiles = Containerdoc.GetElementsByTagName("rootfile");
                foreach (XmlNode root in rootfiles)
                {
                    if (root.Attributes["media-type"] != null && root.Attributes["media-type"].Value == "application/oebps-package+xml")
                    {
                        path = root.Attributes["full-path"].Value;
                    }
                }
            }
            return path;
        }
/// <summary>
/// executing pdfjs exe
/// </summary>
/// <param name="command"></param>
/// <returns></returns>
        public static string ExecuteCommandSync(string command)
        {
            Process proc = null;
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                var procStartInfo = new ProcessStartInfo("cmd", "/c " + command) { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                // Do not create the black window.
                // Now we create a process, assign its ProcessStartInfo and start it
                proc = new Process { StartInfo = procStartInfo };
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();

                if (string.IsNullOrEmpty(result))
                    result = proc.ExitCode.ToString();

                // Display the command output.
                Console.WriteLine(result);
                return result;
            }
            catch (Exception objException)
            {
                return string.Empty;
            }
            finally
            {
                if (proc != null)
                {
                    proc.Close();
                    proc.Dispose();
                }
            }
        }
    }
    public class OPFClass
    {
        public string id { get; set; }
        public string mediaType { get; set; }

    }
}
