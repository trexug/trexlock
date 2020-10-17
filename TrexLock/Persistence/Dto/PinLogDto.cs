using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;

namespace TrexLock.Persistence.Dto
{
	public class PinLogDto
	{
		public int? Id { get; set; }
		public int Pin { get; set; }
		public string Reason { get; set; }
		public GpioPinValue PinState { get; set; }
		public DateTime Time { get; set; }
	}
}
