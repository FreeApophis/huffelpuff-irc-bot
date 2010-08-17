/*
 *  The Calendarplugin can read ICS Calenders and remind you.
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
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
using DDay.iCal;
using Huffelpuff;
using Huffelpuff.Plugins;

namespace Plugin
{
    /// <summary>
    /// Calendar Plugin
    /// </summary>
    public class CalendarPlugin : AbstractPlugin
    {

        public CalendarPlugin(IrcBot botInstance) : base(botInstance) { }

        private IICalendarCollection calendars;
        public override void Init()
        {
            calendars = iCalendar.LoadFromUri(new Uri("http://www.piraten-partei.ch/calendar-event/ical"));

            foreach (var calendar in calendars)
            {
                foreach (var @event in calendar.Events)
                {
                    Console.WriteLine(@event.Location);
                    Console.WriteLine(@event.Start.ToString());
                }
            }
            
        }

        public override void Activate()
        {

            active = true;

            base.Activate();
        }

        public override void Deactivate()
        {
            active = false;

            base.Deactivate();
        }


        public override string AboutHelp()
        {
            return "Calendar Plugin Help";
        }
    }
}