﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table
{
    public class Consts
    {
        public const int MaxStackAllocSize = 512;
        public const int DefaultBlockSize = 32768;
        public const uint DefaultMaxDistinctCount = 32768;
        /// <summary>
        /// Default max write count
        /// </summary>
        public const uint MaxMetaDataWriteCount = 100;
        public const string HasBeenAnalysed                  = "HasBeenAnalysed";
        public const string IsNumeric                        = "IsNumeric";
        public const string CategoryPrefix                   = "Category:";
        public const string NormalizationType                = "NormalizationType";
    }
}
