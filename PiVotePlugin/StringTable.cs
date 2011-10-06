/*
 *  <project description>
 * 
 *  Copyright (c) 2010 Stefan Thöni <stefan@savvy.ch>
 *  File created by exception at 03.07.2009 18:53
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin
{
    public class StringTable
    {
        private readonly List<int> columns;
        private readonly List<List<string>> data;
        private bool hasHeader = true;

        public StringTable()
        {
            columns = new List<int>();
            data = new List<List<string>> { new List<string>() };
        }

        public void SetColumnCount(int count)
        {
            hasHeader = false;
            count.Times(() => columns.Add(1));
        }

        public void AddColumn(string header)
        {
            columns.Add(header.Length + 1);
            data[0].Add(header);
        }

        public void AddRow(params string[] text)
        {
            data.Add(new List<string>(text));

            for (int index = 0; index < text.Length; index++)
            {
                columns[index] = Math.Max(columns[index], text[index].Length + 1);
            }
        }

        public string Render()
        {
            var builder = new StringBuilder();

            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {
                var row = data[rowIndex];

                for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
                {
                    builder.Append(Fixed(row[columnIndex], columns[columnIndex]));
                }

                if (rowIndex == 0 && hasHeader)
                {
                    builder.AppendLine();

                    for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
                    {
                        builder.Append(Line(columns[columnIndex]));
                    }
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string Line(int length)
        {
            string text = string.Empty;

            while (text.Length < length)
            {
                text += "-";
            }

            return text;
        }

        private static string Fixed(string input, int length)
        {
            string text = input.Length > length - 1 ? input.Substring(0, length - 1) : input;

            while (text.Length < length)
            {
                text += " ";
            }

            return text;
        }
    }
}
