﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDataFramework.ValueGenerator
{
    public interface IValueGenerator
    {
        object GetValue(Type forType);
    }
}
