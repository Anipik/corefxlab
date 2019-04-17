﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data
{
    /// <summary>
    /// A DataFrameTable is just a container that holds a number of DataFrameColumns. It mainly acts as a convenient store to allow DataFrame to implement its algorithms
    /// </summary>
    internal class DataFrameTable
    {
        private IList<BaseDataFrameColumn> _columns;

        private List<string> _columnNames = new List<string>();

        private Dictionary<string, int> _columnNameToIndexDictionary = new Dictionary<string, int>(StringComparer.Ordinal);

        public long RowCount { get; private set; } = 0;

        public int ColumnCount { get; private set; } = 0;

        public DataFrameTable()
        {
            _columns = new List<BaseDataFrameColumn>();
        }

        private DataFrameTable(IList<BaseDataFrameColumn> columns)
        {
            columns = columns ?? throw new ArgumentNullException(nameof(columns));
            _columns = columns;
            ColumnCount = columns.Count;
            if (columns.Count > 0)
            {
                RowCount = columns[0].Length;
                int cc = 0;
                foreach (var column in columns)
                {
                    _columnNames.Add(column.Name);
                    _columnNameToIndexDictionary.Add(column.Name, cc++);
                }
            }
        }

        public DataFrameTable(BaseDataFrameColumn column) : this(new List<BaseDataFrameColumn> { column }) { }

        public BaseDataFrameColumn Column(int columnIndex) => _columns[columnIndex];

        public IList<object> GetRow(long rowIndex)
        {
            var ret = new List<object>();
            for (int i = 0; i < ColumnCount; i++)
            {
                ret.Add(Column(i)[rowIndex]);
            }
            return ret;
        }

        public void InsertColumn<T>(int columnIndex, IEnumerable<T> column, string columnName)
            where T : struct
        {
            column = column ?? throw new ArgumentNullException(nameof(column));
            if (columnIndex < 0 || columnIndex > _columns.Count)
            {
                throw new ArgumentException($"Invalid columnIndex {columnIndex} passed into Table.AddColumn");
            }
            BaseDataFrameColumn newColumn = new PrimitiveDataFrameColumn<T>(columnName, column);
            InsertColumn(columnIndex, newColumn);
        }

        public void InsertColumn(int columnIndex, BaseDataFrameColumn column)
        {
            column = column ?? throw new ArgumentNullException(nameof(column));
            if (columnIndex < 0 || columnIndex > _columns.Count)
            {
                throw new ArgumentException($"Invalid columnIndex {columnIndex} passed into Table.AddColumn");
            }
            if (RowCount > 0 && column.Length != RowCount)
            {
                throw new ArgumentException($"Column's length {column.Length} must match Table's length {RowCount}");
            }
            if (_columnNameToIndexDictionary.ContainsKey(column.Name))
            {
                throw new ArgumentException($"Table already contains a column called {column.Name}");
            }
            RowCount = column.Length;
            _columnNames.Insert(columnIndex, column.Name);
            _columnNameToIndexDictionary[column.Name] = columnIndex;
            _columns.Insert(columnIndex, column);
            ColumnCount++;
        }

        public void SetColumn(int columnIndex, BaseDataFrameColumn column)
        {
            column = column ?? throw new ArgumentNullException(nameof(column));
            if (columnIndex < 0 || columnIndex >= ColumnCount)
            {
                throw new ArgumentException($"Invalid columnIndex {columnIndex} passed in to Table.SetColumn");
            }
            if (RowCount > 0 && column.Length != RowCount)
            {
                throw new ArgumentException($"Column's length {column.Length} must match table's length {RowCount}");
            }
            if (_columnNameToIndexDictionary.ContainsKey(column.Name))
            {
                throw new ArgumentException($"Table already contains a column called {column.Name}");
            }
            _columnNameToIndexDictionary.Remove(_columnNames[columnIndex]);
            _columnNames[columnIndex] = column.Name;
            _columnNameToIndexDictionary[column.Name] = columnIndex;
            _columns[columnIndex] = column;
        }

        public void RemoveColumn(int columnIndex)
        {
            _columnNameToIndexDictionary.Remove(_columnNames[columnIndex]);
            _columnNames.RemoveAt(columnIndex);
            _columns.RemoveAt(columnIndex);
            ColumnCount--;
        }

        public void RemoveColumn(string columnName)
        {
            int columnIndex = GetColumnIndex(columnName);
            if (columnIndex != -1)
            {
                RemoveColumn(columnIndex);
            }
        }

        public int GetColumnIndex(string columnName)
        {
            if (_columnNameToIndexDictionary.TryGetValue(columnName, out int columnIndex) )
            {
                return columnIndex;
            }
            return -1;
        }

    }
}
