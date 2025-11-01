using iText.Html2pdf;
using iText.IO.Source;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.Util;
using VectorStinger.Foundation.Utilities.CrossUtil;

namespace VectorStinger.Foundation.Utilities.Document
{
    public class HtmlHelper
    {
        private StringBuilder _data = new StringBuilder();
        public string GenerarDocumento(object Source, string pathTempleta, string PathTemp)
        {
            string vResul = CustomGuid.GetGuid();
            if (!Directory.Exists(PathTemp))
            {
                Directory.CreateDirectory(PathTemp);
            }
            FileUtil file = new FileUtil();
            file.NameFile = PathTemp + "\\" + vResul + ".doc";
            string val_replace = File.ReadAllText(pathTempleta);

            try
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (var item in Source.GetType().GetProperties())
                {
                    values.Add(item.Name, item.GetValue(Source, null));
                }

                file.createFile();

                foreach (var item in values)
                {

                    if (item.Value != null)
                    {
                        val_replace = val_replace.Replace("{Obj." + item.Key + "}", Convert.ToString(item.Value));
                    }
                }
                file.writeFile(val_replace.ToString());
                file.closeFile();
            }
            catch
            {
                throw;
            }
            return file.NameFile.Replace(@"\\", @"\");
        }

        public string GenerarDocumentoPDF(object Source, string pathTempleta, string PathTemp)
        {
            string vResul = CustomGuid.GetGuid();
            if (!Directory.Exists(PathTemp))
            {
                Directory.CreateDirectory(PathTemp);
            }
            FileUtil file = new FileUtil();
            file.NameFile = System.IO.Path.Combine(PathTemp, vResul + ".html");
            string val_replace = File.ReadAllText(pathTempleta);
            try
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (var item in Source.GetType().GetProperties())
                {
                    values.Add(item.Name, item.GetValue(Source, null));
                }

                file.createFile();

                foreach (var item in values)
                {

                    if (item.Value != null)
                    {
                        val_replace = val_replace.Replace("{Obj." + item.Key + "}", Convert.ToString(item.Value));
                    }
                    else
                    {
                        val_replace = val_replace.Replace("{Obj." + item.Key + "}", "");
                    }
                }
                file.writeFile(val_replace.ToString());


                string pathtml = file.NameFile.Replace(@"\\", @"\");
                string patpdf = pathtml.Replace(".html", ".pdf");
                file.closeFile();
                file.Dispose();

                HtmlConverter.ConvertToPdf(new FileInfo(pathtml), new FileInfo(patpdf));
                //ConvertToPDF(pathtml, patpdf);
            }
            catch
            {
                throw;
            }
            return file.NameFile.Replace(@"\\", @"\").Replace(".html", ".pdf");
        }

        private void ConvertToPDF(string pathHtml, string pathPDF)
        {
            iText.IO.Source.ByteArrayOutputStream baos = new iText.IO.Source.ByteArrayOutputStream();
            PdfDocument pdfDocument = new PdfDocument(new PdfWriter(baos));
            pdfDocument.SetDefaultPageSize(PageSize.LETTER);
            FileStream fs = new FileStream(pathHtml, FileMode.OpenOrCreate);
            HtmlConverter.ConvertToPdf(fs, pdfDocument);

            PdfDocument resultantDocument = new PdfDocument(new PdfWriter(pathPDF));

            pdfDocument = new PdfDocument(new PdfReader(new PdfWriter(pathPDF)));
            // 2
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                PdfPage page = pdfDocument.GetPage(i);
                PdfFormXObject formXObject = page.CopyAsFormXObject(resultantDocument);
                PdfCanvas pdfCanvas = new PdfCanvas(resultantDocument.AddNewPage());
                // 3a and 3b
                //pdfCanvas.AddXObject(formXObject, 0.5f, 0, 0, 0.5f, 0, 0);
                pdfCanvas.AddXObject(formXObject);
            }

            pdfDocument.Close();
            resultantDocument.Close();
        }
    }
}
