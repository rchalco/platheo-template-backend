using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Infrastructure.DataAccess.Interface
{
    public interface IImageManager
    {
        /// <summary>
        /// Saves the image bytes to a specified folder with the given file name.
        /// </summary>
        /// <param name="imageBytes">The byte array of the image.</param>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="folder">The folder where the image will be saved.</param>
        void SaveImagePath(byte[] imageBytes, string fileName, string folder);
        /// <summary>
        /// Gets the base64 encoded string of the image with a data URI prefix.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve.</param>
        /// <param name="folder">The folder where the image is stored.</param>
        /// <returns>A base64 encoded string with a data URI prefix.</returns>
        string GetImageBase64WithPrefix(string fileName, string folder);

    }
}
