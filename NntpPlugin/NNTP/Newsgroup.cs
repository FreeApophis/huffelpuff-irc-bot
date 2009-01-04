using System;

namespace Nntp
{
	public class Newsgroup: IComparable
	{
		protected string group;
		protected int low;
		protected int high;

		public string Group
		{
			get
			{
				return group;
			}
			set
			{
				group = value;
			}
		}
		public int Low
		{
			get
			{
				return low;
			}
			set
			{
				low = value;
			}
		}
		public int High
		{
			get
			{
				return high;
			}
			set
			{
				high = value;
			}
		}
		public Newsgroup()
		{
		}
		public Newsgroup( string group, int low, int high )
		{
			this.group = group;
			this.low = low;
			this.high = high;
		}
		public int CompareTo( object r )
		{
			return this.Group.CompareTo(((Newsgroup)r).Group);
		}
	}
}
