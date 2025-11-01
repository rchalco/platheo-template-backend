using PlumbingProps.Document;
using VectorStinger.Foundation.Utilities.CrossUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Document
{
    public class WordHelper
    {

        public class ItemValues
        {
            public string key { get; set; }
            public string values { get; set; }
        }

        public enum keys
        {
            enter = 13,
            SI,
            NO
        }

        public static string GetCodeKey(keys key)
        {
            string resul = string.Empty;
            switch (key)
            {
                case keys.enter:
                    resul = WordParts.tagEnter;
                    break;
                case keys.SI:
                    resul = WordParts.tagOK;
                    break;
                case keys.NO:
                    resul = WordParts.tagNO;
                    break;
                default:
                    break;
            }
            return resul;
        }
        public class CellTitles
        {
            public string Title { get; set; }

            public string Width { get; set; }

            private bool _Visible = true;

            public bool Visible
            {
                get { return _Visible; }
                set { _Visible = value; }
            }
        }

        private StringBuilder _data = new StringBuilder();

        public WordHelper(string PathFilePlantilla)
        {
            FileUtil file = new FileUtil();
            file.NameFile = PathFilePlantilla;
            _data = new StringBuilder(file.GetData());
        }

        public string GenerarDocumento(object Source, Dictionary<string, CellTitles[]> pTitles, string PathTemp)
        {
            string vResul = CustomGuid.GetGuid();
            FileUtil file = new FileUtil();
            file.NameFile = Path.Combine(PathTemp, vResul + ".doc");

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
                    if (item.Value != null && item.Value.GetType().FullName.Contains("List"))
                    {

                        CellTitles[] vCurrentTitles = pTitles.Keys.Contains(item.Key) ? pTitles[item.Key] : new CellTitles[0];

                        IList Elementos = (IList)item.Value;
                        StringBuilder tabalHtml = new StringBuilder(WordParts.tagTable);
                        tabalHtml.Append(WordParts.tagStyle);
                        tabalHtml.Append(WordParts.tagGrid);

                        for (int i = 0; i < vCurrentTitles.Length; i++)
                        {
                            if (vCurrentTitles[i].Visible)
                            {
                                tabalHtml.Append(string.Format(WordParts.tagGridColumn, vCurrentTitles[i].Width));
                            }
                        }
                        tabalHtml.Append(WordParts.tagGridEnd);
                        tabalHtml.Append(WordParts.tagGridRow);
                        for (int i = 0; i < vCurrentTitles.Length; i++)
                        {
                            if (vCurrentTitles[i].Visible)
                            {
                                tabalHtml.Append(string.Format(WordParts.tagGridCellTitle, vCurrentTitles[i].Title));
                            }
                        }
                        tabalHtml.Append(WordParts.tagGridRowEnd);
                        foreach (var itemElementos in Elementos)
                        {
                            tabalHtml.Append(WordParts.tagGridRow);
                            //foreach (var itemProp in itemElementos.GetType().GetProperties())
                            for (int i = 0; i < vCurrentTitles.Length; i++)
                            {
                                if (vCurrentTitles[i].Visible)
                                {
                                    var itemProp = itemElementos.GetType().GetProperties()[i];
                                    string valCell = Convert.ToString(itemProp.GetValue(itemElementos, null)).Replace("\n", WordParts.tagEnter);
                                    tabalHtml.Append(string.Format(WordParts.tagGridCellValue, valCell));
                                }
                            }
                            tabalHtml.Append(WordParts.tagGridRowEnd);
                        }

                        tabalHtml.Append(WordParts.tagTableEnd);
                        _data.Replace(item.Key, tabalHtml.ToString());
                    }
                    else
                    {
                        string val_replace = string.Empty;
                        if (item.Value != null)
                        {
                            val_replace = Convert.ToString(item.Value);
                        }
                        _data.Replace("Obj." + item.Key, val_replace);
                    }
                }
                file.writeFile(_data.ToString());
                file.closeFile();
            }
            catch
            {
                throw;
            }
            //return file.NameFile;
            return file.NameFile.Replace(@"\\", @"\");
            // return vResul + ".doc";
        }

        public string GenerarDocumento(List<ItemValues> valuesToReplace, Dictionary<string, CellTitles[]> pTitles, string PathTemp)
        {
            string vResul = CustomGuid.GetGuid();
            FileUtil file = new FileUtil();
            file.NameFile = PathTemp + "\\" + vResul + ".doc";

            try
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                valuesToReplace.ForEach(x =>
                {
                    values.Add(x.key, x.values);
                });



                file.createFile();

                foreach (var item in values)
                {
                    if (item.Value != null && item.Value.GetType().FullName.Contains("List"))
                    {

                        CellTitles[] vCurrentTitles = pTitles.Keys.Contains(item.Key) ? pTitles[item.Key] : new CellTitles[0];

                        IList Elementos = (IList)item.Value;
                        string tabalHtml = WordParts.tagTable;
                        tabalHtml += WordParts.tagStyle;
                        tabalHtml += WordParts.tagGrid;

                        for (int i = 0; i < vCurrentTitles.Length; i++)
                        {
                            if (vCurrentTitles[i].Visible)
                            {
                                tabalHtml += string.Format(WordParts.tagGridColumn, vCurrentTitles[i].Width);
                            }
                        }
                        tabalHtml += WordParts.tagGridEnd;
                        tabalHtml += WordParts.tagGridRow;
                        for (int i = 0; i < vCurrentTitles.Length; i++)
                        {
                            if (vCurrentTitles[i].Visible)
                            {
                                tabalHtml += string.Format(WordParts.tagGridCellTitle, vCurrentTitles[i].Title);
                            }
                        }
                        tabalHtml += WordParts.tagGridRowEnd;
                        foreach (var itemElementos in Elementos)
                        {
                            tabalHtml += WordParts.tagGridRow;
                            //foreach (var itemProp in itemElementos.GetType().GetProperties())
                            for (int i = 0; i < vCurrentTitles.Length; i++)
                            {
                                if (vCurrentTitles[i].Visible)
                                {
                                    var itemProp = itemElementos.GetType().GetProperties()[i];
                                    tabalHtml += string.Format(WordParts.tagGridCellValue, Convert.ToString(itemProp.GetValue(itemElementos, null)));
                                }
                            }
                            tabalHtml += WordParts.tagGridRowEnd;
                        }

                        tabalHtml += WordParts.tagTableEnd;
                        _data.Replace(item.Key, tabalHtml);
                    }
                    else
                    {
                        string val_replace = string.Empty;
                        if (item.Value != null)
                        {
                            val_replace = Convert.ToString(item.Value);
                        }
                        _data.Replace("Obj." + item.Key, val_replace);
                    }
                }
                file.writeFile(_data.ToString());
                file.closeFile();
            }
            catch
            {
                throw;
            }
            //return file.NameFile;
            return file.NameFile.Replace(@"\\", @"\");
            // return vResul + ".doc";
        }
    }
}
