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
using System.Linq;
using System.Text;

namespace PiVotePlugin
{
  public class StringTable
  {
    private List<int> columns;
    private List<List<string>> data;

    public StringTable()
    {
      this.columns = new List<int>();
      this.data = new List<List<string>>();
      this.data.Add(new List<string>());
    }

    public void AddColumn(string header, int length)
    {
      this.columns.Add(length);
      this.data[0].Add(header);
    }

    public void AddRow(params string[] text)
    {
      this.data.Add(new List<string>(text));
    }

    public string Render()
    {
      StringBuilder builder = new StringBuilder();

      for (int rowIndex = 0; rowIndex < this.data.Count; rowIndex++)
      {
        var row = this.data[rowIndex];

        for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
        { 
          builder.Append(Fixed(row[columnIndex], this.columns[columnIndex]));
        }

        if (rowIndex == 0)
        {
          for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
          {
            builder.Append(Line(this.columns[columnIndex]));
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
