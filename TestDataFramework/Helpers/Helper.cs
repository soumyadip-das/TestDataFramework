﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDataFramework.Helpers
{
    public delegate DateTime DateTimeProvider();

    public static class Helper
    {
        public static DateTime Now => DateTime.Now;
    }
}