using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Document
{
    public class FileUtil : IDisposable
    {
        private FileStream fs;

        public FileStream Fs
        {
            get { return fs; }
            set { fs = value; }
        }

        private StreamWriter sw;

        public StreamWriter Sw
        {
            get { return sw; }
            set { sw = value; }
        }

        private string nameFile;

        public string NameFile
        {
            set { nameFile = value; }
            get { return nameFile; }
        }

        /// <summary>
        /// Constructor por defecto de la Clase
        /// </summary>
        public FileUtil()
        {
        }

        public FileUtil(string name, bool open = true)
        {
            nameFile = name;
            if (open)
            {
                fs = new FileStream(nameFile, FileMode.OpenOrCreate, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.GetEncoding("iso-8859-1"));
            }
        }

        public void createFile()
        {
            fs = new FileStream(nameFile, FileMode.CreateNew, FileAccess.Write);
            sw = new StreamWriter(fs);
        }

        public void openWriteFile()
        {
            fs = new FileStream(nameFile, FileMode.Open, FileAccess.Write);
            sw = new StreamWriter(fs);
        }

        public void openWriteFile(string _nameFile)
        {
            fs = new FileStream(_nameFile, FileMode.Open, FileAccess.Write);
            sw = new StreamWriter(fs);
        }

        public void closeFile()
        {
            fs.Close();
        }

        public void writeFile(string data)
        {
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(data);
            sw.Flush();
        }

        public bool existFile()
        {
            return File.Exists(nameFile);
        }

        public bool deleteFile()
        {
            try
            {
                if (File.Exists(nameFile))
                {
                    File.Delete(nameFile);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetData()
        {
            if (this == null)
            {
                throw new Exception("Objeto Archivo util no instanciado");
            }
            /*FileStream stream = new FileStream(this.nameFile, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);*/
            string vData = "";

            /*while (reader.Peek() > -1)
                vData += reader.ReadLine();*/
            vData = File.ReadAllText(nameFile, Encoding.GetEncoding("UTF-8"));
            return vData;
        }

        /// <summary>
        /// Permite transformar un archivo de cualquier tipo de extension, en un array de bytes
        /// </summary>
        /// <param name="pFilePath">Parametro que establece la ruta física donde se encuentra el Archivo a Convertir</param>
        /// <param name="pArrayBytes">Parametro por referencia que obtiene el array de bytes</param>
        /// <exception cref="ArgumentNullException">Error desplegado cuando los parametros no contienen valores</exception>
        /// <exception cref="FileNotFoundException">Error desplegado cuando el parametro <para>pFilePath</para> contiene una ruta inexistente</exception>
        public void ReadArrayMemoryOfBytes(string pFilePath, ref byte[] pArrayBytes)
        {
            try
            {
                if (string.IsNullOrEmpty(pFilePath.Trim()))
                    throw new ArgumentNullException();
                else if (!File.Exists(pFilePath))
                    throw new FileNotFoundException();
                else
                    pArrayBytes = File.ReadAllBytes(pFilePath);
            }
            catch (ArgumentNullException ex)
            {
                pArrayBytes = null;
                throw new ArgumentNullException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                pArrayBytes = null;
                throw new FileNotFoundException(ex.Message);
            }
        }

        /// <summary>
        /// Permite convertir un array de bytes en un documento físico, y almacenarlo en un directorio
        /// </summary>
        /// <param name="pFilePath">Parametro que establece la ruta física donde se almacenarla el archivo <remarks>Esta ruta debe incluir la extensión original</remarks></param>
        /// <param name="pArrayBytes">Parametro que obtiene el array de bytes que contiene los datos a convertir</param>
        /// <exception cref="ArgumentNullException">Error desplegado cuando los parametros no contienen valores</exception>
        /// <exception cref="FileNotFoundException">Error desplegado cuando el parametro <para>pFilePath</para> contiene una ruta inexistente</exception>
        public void WriteArrayMemoryOfBytes(string pFilePath, byte[] pArrayBytes)
        {
            try
            {
                if (string.IsNullOrEmpty(pFilePath.Trim()) || pArrayBytes.Length <= 0)
                    throw new ArgumentNullException();
                else if (File.Exists(pFilePath))
                    throw new FileNotFoundException();
                using (FileStream vArchivo = new FileStream(pFilePath, FileMode.CreateNew))
                {
                    vArchivo.Write(pArrayBytes, 0, pArrayBytes.Length);
                    vArchivo.Flush();
                    vArchivo.Close();
                }
            }
            catch (ArgumentNullException ex)
            {
                pArrayBytes = null;
                throw new ArgumentNullException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                pArrayBytes = null;
                throw new FileNotFoundException(ex.Message);
            }
        }

        public void Dispose()
        {
            fs?.Close();
            fs?.Dispose();
        }
    }
}
