﻿using System.IO;

namespace Nop.Services.ExportImport {
    /// <summary>
    /// Import manager interface
    /// </summary>
    public partial interface IImportManager {
        /// <summary>
        /// Import newsletter subscribers from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported subscribers</returns>
        int ImportNewsletterSubscribersFromTxt(Stream stream);
    }
}
