/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.07.2009 17:30
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

namespace Plugin
{
	public class TwitterUser {
	    internal TwitterUser() {}
	    
	    public long Id { get; set; }
	    public string Nick { get; set; }
	    public string Name { get; set; }
	    public string Location { get; set; }
	    public string Description { get; set; }
	    public DateTime Created { get; set; }
	    public int Followers { get; set; }
	    public int Statuses { get; set; }
	    public int Friends { get; set; }
	}
}


