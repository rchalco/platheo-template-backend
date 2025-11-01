using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using VectorStinger.Foundation.Utilities.CrossUtil;
using PlumbingProps.Document;

namespace VectorStinger.Foundation.Utilities.Document
{
    public class ExcelHelper
    {
        private List<Type> typeNumerics = new List<Type>();

        public class Formula
        {
            public string FormulaValue { get; set; }

            public int ColIndex { get; set; }
        }

        public class CellCustom
        {
            public object Value { get; set; }

            public int ColIndex { get; set; }
        }

        public class Row
        {
            /// <summary>
            /// indice de la columna a la cual pertenece, si es 0 la fila iniciara en la coordenada de la tabla a la cual pertence
            /// </summary>
            public int IndexColumn { get; set; }

            /// <summary>
            /// bancdera que indica que si la fila es un titulo
            /// </summary>
            public bool IsTitle { get; set; }

            /// <summary>
            /// Datos de la fila, los valores de las propiedades del objeto se refelejaran en una celda en particular
            /// </summary>
            public object Data { get; set; }
        }

        public class Table
        {
            public Table()
            {
                Rows = new List<Row>();
            }

            /// <summary>
            /// indice inicial de la fila
            /// </summary>
            public int IndexRow { get; set; }

            /// <summary>
            /// inidice inicial de al columna
            /// </summary>
            public int IndexColumn { get; set; }

            /// <summary>
            /// Datos de las filas de la tabla
            /// </summary>
            public List<Row> Rows { get; set; }

            /// <summary>
            /// Nombre de la tabla
            /// </summary>
            public string NameTable { get; set; }

            public void AddRow(object objData, int indexcolumn = -1, bool isTitle = false)
            {
                Rows.Add(new Row() { Data = objData, IndexColumn = indexcolumn, IsTitle = isTitle });
            }
        }

        private StringBuilder _data = new StringBuilder();

        public Table[] DataExcel { get; set; }

        /// <summary>
        /// inicializa un objeto generador de excel
        /// </summary>
        /// <param name="PathFilePlantilla">direccion completa de la ruta de la plantilla</param>
        /// <param name="cantidadTablas">nro de tablas a dibujar en el excel</param>
        public ExcelHelper(string PathFilePlantilla, int cantidadTablas)
        {
            FileUtil file = new FileUtil();
            file.NameFile = PathFilePlantilla;
            _data = new StringBuilder(file.GetData());
            DataExcel = new Table[cantidadTablas];
            for (int i = 0; i < cantidadTablas; i++)
            {
                DataExcel[i] = new Table();
            }

            typeNumerics.Add(typeof(int));
            typeNumerics.Add(typeof(int?));
            typeNumerics.Add(typeof(int));
            typeNumerics.Add(typeof(int?));
            typeNumerics.Add(typeof(long?));
            typeNumerics.Add(typeof(long));
            typeNumerics.Add(typeof(float));
            typeNumerics.Add(typeof(float?));
            typeNumerics.Add(typeof(decimal));
            typeNumerics.Add(typeof(decimal?));
            typeNumerics.Add(typeof(decimal));
            typeNumerics.Add(typeof(decimal?));
            typeNumerics.Add(typeof(double));
            typeNumerics.Add(typeof(double?));
        }

        public string GenerarDocumento(object Source, string PathTemp)
        {
            string vResul = CustomGuid.GetGuid();
            FileUtil file = new FileUtil();
            file.NameFile = string.Format("{0}\\{1}.xml", PathTemp, vResul);

            try
            {
                XNamespace ns = "urn:schemas-microsoft-com:office:spreadsheet";
                Dictionary<string, object> values = new Dictionary<string, object>();
                XDocument xDocExcel = XDocument.Parse(_data.ToString());

                var workSheetFirst = (from p in xDocExcel.Descendants()
                                      where p.Name.LocalName == ExcelParts.tagWorkSheet
                                      select p).FirstOrDefault();
                //verificamos si existe el elemento en la hoja
                if (workSheetFirst == null)
                {
                    throw new ArgumentException("La planitlla no contiene ninguna hoja (WorkSheet)");
                }

                var TableExcelInit = (from p in workSheetFirst.Descendants()
                                      where p.Name.LocalName == ExcelParts.tagTable
                                      select p).FirstOrDefault();

                foreach (var item in Source.GetType().GetProperties())
                {
                    values.Add(item.Name, item.GetValue(Source, null));
                }

                file.createFile();

                XElement TableExcel = CloneElement(TableExcelInit);
                List<XElement> lEliminar = new List<XElement>();

                foreach (var item in DataExcel)
                {
                    #region Rescatamos los modelos de celdas detalle

                    var elementMark = (from p in workSheetFirst.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable select p).FirstOrDefault();
                    if (elementMark == null)
                    {
                        continue; //si no existe la marca entonces continuamo con el siguiente registro
                    }

                    var RowModel = CloneElement(elementMark.Parent.Parent);
                    var cellModel = CloneElement(elementMark.Parent);
                    cellModel.RemoveNodes();//removemos el nodo valor

                    //destruimos la marca
                    elementMark = (from p in TableExcel.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable select p).FirstOrDefault();
                    //elementMark.Parent.Remove();
                    if (!lEliminar.Contains(elementMark.Parent)) lEliminar.Add(elementMark.Parent);

                    //removemos los atributos de index
                    XAttribute atrribindex = (from q in cellModel.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                    if (atrribindex != null)
                    {
                        atrribindex.Remove();
                    }
                    RowModel.RemoveNodes();
                    atrribindex = (from q in RowModel.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                    if (atrribindex != null)
                    {
                        atrribindex.Remove();
                    }

                    #endregion Rescatamos los modelos de celdas detalle

                    #region Rescatamos los modelos de celdas titulo

                    elementMark = (from p in workSheetFirst.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable + "Title" select p).FirstOrDefault();
                    XElement RowModelTitle = null;
                    XElement CellModelTitle = null;
                    if (elementMark != null)
                    {
                        RowModelTitle = CloneElement(elementMark.Parent.Parent);
                        CellModelTitle = CloneElement(elementMark.Parent);
                        CellModelTitle.RemoveNodes();//removemos el nodo valor

                        //destruimos la marca
                        elementMark = (from p in TableExcel.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable + "Title" select p).FirstOrDefault();
                        //elementMark.Parent.Remove();
                        if (!lEliminar.Contains(elementMark.Parent)) lEliminar.Add(elementMark.Parent);

                        //removemos los atributos de index
                        atrribindex = (from q in cellModel.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                        if (atrribindex != null)
                        {
                            atrribindex.Remove();
                        }
                        RowModelTitle.RemoveNodes();
                        atrribindex = (from q in RowModel.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                        if (atrribindex != null)
                        {
                            atrribindex.Remove();
                        }
                    }

                    #endregion Rescatamos los modelos de celdas titulo

                    #region Rescatamos los modelos de celdas Custom

                    elementMark = (from p in workSheetFirst.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable + "Custom" select p).FirstOrDefault();
                    XElement RowModelCustom = null;
                    XElement CellModelCustom = null;
                    if (elementMark != null)
                    {
                        RowModelCustom = CloneElement(elementMark.Parent.Parent);
                        CellModelCustom = CloneElement(elementMark.Parent);
                        CellModelCustom.RemoveNodes();//removemos el nodo valor

                        //destruimos la marca
                        elementMark = (from p in TableExcel.Descendants() where p.Name.LocalName == ExcelParts.tagData && p.Value == item.NameTable + "Custom" select p).FirstOrDefault();
                        //elementMark.Parent.Remove();
                        if (!lEliminar.Contains(elementMark.Parent)) lEliminar.Add(elementMark.Parent);

                        //removemos los atributos de index
                        atrribindex = (from q in CellModelCustom.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                        if (atrribindex != null)
                        {
                            atrribindex.Remove();
                        }
                        RowModelCustom.RemoveNodes();
                        atrribindex = (from q in RowModelCustom.Attributes() where q.Name.LocalName == ExcelParts.tagIndex select q).FirstOrDefault();
                        if (atrribindex != null)
                        {
                            atrribindex.Remove();
                        }
                    }

                    #endregion Rescatamos los modelos de celdas Custom

                    //indexamos todas las filas
                    var RowsContent = from p in TableExcel.Descendants() where p.Name.LocalName == ExcelParts.tagRow && !p.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex) select p;
                    int rowIndexGeneric = 1;

                    foreach (var itemRowInit in RowsContent)
                    {
                        while (TableExcel.Descendants().Any(x => x.Name.LocalName == ExcelParts.tagRow && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == rowIndexGeneric.ToString())))
                        {
                            rowIndexGeneric++;
                        }
                        itemRowInit.Add(new XAttribute(ns + ExcelParts.tagIndex, rowIndexGeneric.ToString()));
                        rowIndexGeneric++;
                    }

                    Table Elementos = item;
                    int rowIndex = Elementos.IndexRow;

                    foreach (Row itemElementos in Elementos.Rows)
                    {
                        XElement currentRow = null;
                        if (TableExcel.Descendants().Any(x => x.Name.LocalName == ExcelParts.tagRow && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == rowIndex.ToString())))
                        {
                            currentRow = TableExcel.Descendants().Where(x => x.Name.LocalName == ExcelParts.tagRow && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == rowIndex.ToString())).First();
                            //reindexamos las celdas
                            int cellIndexGeneric = 1;
                            var cellContent = from p in currentRow.Descendants() where p.Name.LocalName == ExcelParts.tagCell && !p.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex) select p;

                            foreach (var cellitem in cellContent)
                            {
                                while (currentRow.Descendants().Any(x => x.Name.LocalName == ExcelParts.tagCell && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == cellIndexGeneric.ToString())))
                                {
                                    cellIndexGeneric++;
                                }
                                cellitem.Add(new XAttribute(ns + ExcelParts.tagIndex, cellIndexGeneric.ToString()));
                                cellIndexGeneric++;
                            }
                        }
                        else
                        {
                            if (itemElementos.IsTitle && RowModelTitle != null)
                                currentRow = CloneElement(RowModelTitle);
                            else
                                currentRow = CloneElement(RowModel);

                            currentRow.Add(new XAttribute(ns + ExcelParts.tagIndex, rowIndex.ToString()));
                        }

                        rowIndex++;

                        int colIndex = itemElementos.IndexColumn > 0 ? itemElementos.IndexColumn : Elementos.IndexColumn;
                        foreach (var itemProp in itemElementos.Data.GetType().GetProperties())
                        {
                            var currentCell = CloneElement(cellModel);
                            if (itemElementos.IsTitle && CellModelTitle != null)
                                currentCell = CloneElement(CellModelTitle);

                            if (itemProp.PropertyType == typeof(CellCustom) && CellModelCustom != null)
                            {
                                currentCell = CloneElement(CellModelCustom);
                            }

                            XElement currenteData = null;
                            XAttribute atrribFromula = null;
                            Formula formula = null;

                            if (typeNumerics.Contains(itemProp.PropertyType))
                            {
                                currenteData = new XElement(ExcelParts.tagData, new XAttribute(ns + ExcelParts.tagType, ExcelParts.tagDataNumber));
                            }
                            else if (itemProp.PropertyType == typeof(Formula))
                            {
                                formula = (Formula)itemProp.GetValue(itemElementos.Data, null);
                                if (formula != null)
                                {
                                    atrribFromula = new XAttribute(ns + ExcelParts.tagFormula, formula.FormulaValue);
                                }
                            }
                            else if (itemProp.PropertyType == typeof(CellCustom) && typeNumerics.Contains(((CellCustom)itemProp.GetValue(itemElementos.Data, null)).Value.GetType()))
                            {
                                currenteData = new XElement(ExcelParts.tagData, new XAttribute(ns + ExcelParts.tagType, ExcelParts.tagDataNumber));
                            }
                            else
                            {
                                currenteData = new XElement(ExcelParts.tagData, new XAttribute(ns + ExcelParts.tagType, ExcelParts.tagDataString));
                            }
                            if (currenteData != null)
                            {
                                if (itemProp.PropertyType == typeof(CellCustom))
                                {
                                    currenteData.Value = Convert.ToString((itemProp.GetValue(itemElementos.Data, null) as CellCustom).Value);
                                }
                                else
                                {
                                    currenteData.Value = Convert.ToString(itemProp.GetValue(itemElementos.Data, null));
                                }
                            }

                            if (currenteData != null)
                            {
                                currentCell.Add(currenteData);
                            }

                            if (atrribFromula != null)
                            {
                                currentCell.Add(atrribFromula);
                            }
                            if (formula != null && formula.ColIndex > 0)
                            {
                                var cellDelete = currentRow.Descendants().FirstOrDefault(x => x.Name.LocalName == ExcelParts.tagCell && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == formula.ColIndex.ToString()));
                                if (cellDelete != null)
                                {
                                    cellDelete.Remove();
                                }

                                currentCell.Add(new XAttribute(ns + ExcelParts.tagIndex, formula.ColIndex.ToString()));
                            }
                            else
                            {
                                while (currentRow.Descendants().Any(x => x.Name.LocalName == ExcelParts.tagCell && x.Attributes().Any(y => y.Name.LocalName == ExcelParts.tagIndex && y.Value == colIndex.ToString())))
                                {
                                    colIndex++;
                                }
                                currentCell.Add(new XAttribute(ns + ExcelParts.tagIndex, colIndex.ToString()));
                            }

                            currentRow.Add(currentCell);
                            colIndex++;
                        }

                        var ordercell = currentRow.Elements().Where(x => x.Name.LocalName == ExcelParts.tagCell).ToArray();
                        ordercell = ordercell.OrderBy(e => Convert.ToInt32(e.Attribute(e.Attributes().Where(h => h.Name.LocalName == ExcelParts.tagIndex).First().Name).Value)).ToArray();
                        currentRow.RemoveNodes();
                        currentRow.Add(ordercell);

                        if (currentRow.Parent == null)
                        {
                            TableExcel.Add(currentRow);
                        }
                    }
                }

                //eliminamos todas las marcas
                lEliminar.ForEach(x => x.Remove());

                //adicionamos los valores del limite de las filas y columas
                var atribbRowCount = TableExcel.Attributes().Where(x => x.Name.LocalName == ExcelParts.tagExpandedRowCount).FirstOrDefault();
                atribbRowCount.Value = ExcelParts.limitRows;

                var atribbColumnCount = TableExcel.Attributes().Where(x => x.Name.LocalName == ExcelParts.tagExpandedColumnCount).FirstOrDefault();
                atribbColumnCount.Value = ExcelParts.limitColumn;

                //reordenamos las filas
                if (DataExcel.Count() > 0)
                {
                    var roworders = TableExcel.Elements().Where(x => x.Name.LocalName == ExcelParts.tagRow).ToArray();
                    roworders = roworders.OrderBy(e => Convert.ToInt32(e.Attribute(e.Attributes().Where(h => h.Name.LocalName == ExcelParts.tagIndex).First().Name).Value)).ToArray();
                    TableExcel.Elements().Where(x => x.Name.LocalName == ExcelParts.tagRow).ToList().ForEach(i => i.Remove());
                    TableExcel.Add(roworders);

                    //eliminamos la tabla original
                    TableExcelInit.Remove();
                    //adicionamos la tabla fabricada
                    workSheetFirst.Add(TableExcel);
                }


                #region Reemplazo de valores escalares

                _data = new StringBuilder("<?xml version=\"1.0\"?>" + xDocExcel.Document.ToString().Replace("<ss:", "<").Replace("</ss:", "</").Replace(" xmlns=\"\"", ""));
                foreach (var item in values)
                {
                    if (item.Value == null)
                    {
                        continue;
                    }
                    _data.Replace("Obj." + item.Key, Convert.ToString(item.Value));
                }

                #endregion Reemplazo de valores escalares

                file.writeFile(_data.ToString());
                file.closeFile();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return file.NameFile.Replace(@"\\", @"\");
        }

        private XElement CloneElement(XElement element)
        {
            return new XElement(element.Name,
                element.Attributes(),
                element.Nodes().Select(n =>
                {
                    XElement e = n as XElement;
                    if (e != null)
                        return CloneElement(e);
                    return n;
                }
                )
            );
        }

    }
}