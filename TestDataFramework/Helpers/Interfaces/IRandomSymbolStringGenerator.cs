﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDataFramework.Helpers.Interfaces
{
    public interface IRandomSymbolStringGenerator
    {
        string GetRandomString(int? length = null);
    }
}