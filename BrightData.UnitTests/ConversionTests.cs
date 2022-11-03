using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BrightData.UnitTests
{
    public class ConversionTests
    {
        [Fact]
        public void DateConversion()
        {
            var dateStrings = new[] {
                "20150225T000000",
                "2018-10-29 07:30:20 PM",
                "2018-10-29 10:02:48 AM",
                "2018-02-21 12:00:00",
                "February 21, 2018",
                "Feb 21, 2018",
                "February 21, 2018",
                "Feb 21, 2018",
                "1995-02-04",
                "19950204",
                "1994-11-05T08:15:30-05:00",
                "1994-11-05T13:15:30Z",
                "2007-04-05T14:30",
                "20070405143000",
                "200704051430"
            };

            foreach (var item in dateStrings)
                item.ToDateTime();
        }
    }
}
