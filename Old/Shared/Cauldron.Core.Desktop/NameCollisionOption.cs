﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cauldron.Core
{
    /// <summary>
    /// Specifies what to do if a file or folder with the specified name already exists in the current folder when you copy, move, or rename a file or folder.
    /// </summary>
    public enum NameCollisionOption
    {
        /// <summary>
        /// Automatically append a number to the base of the specified name if the file or
        /// folder already exists. For example, if MyFile.txt already exists, then the new
        /// file is named MyFile (2).txt. If MyFolder already exists, then the new folder
        /// is named MyFolder (2).
        /// </summary>
        GenerateUniqueName = 0,

        /// <summary>
        /// Replace the existing item if the file or folder already exists.
        /// </summary>
        ReplaceExisting = 1,

        /// <summary>
        /// Raise an exception of type System.Exception if the file or folder already exists.Methods
        /// that don't explicitly pass a value from the CreationCollisionOption enumeration
        /// use the FailIfExists value as the default when you try to create, rename, copy,
        /// or move a file or folder.
        /// </summary>
        FailIfExists = 2,
    }
}