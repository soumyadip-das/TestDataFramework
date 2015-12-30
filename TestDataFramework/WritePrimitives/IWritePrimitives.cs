﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDataFramework.RepositoryOperations.Model;

namespace TestDataFramework.WritePrimitives
{
    public interface IWritePrimitives
    {
        void Insert(IEnumerable<Column> columns);
        string SelectIdentity();
    }
}
