using AwesomeAssertions;
using CraftedSolutions.MarBasCommon.Reflection;
using System.Numerics;

namespace CraftedSolutions.MarBasCommon.Tests.Reflection
{
    [TestClass]
    public class TimeSpanLiteralsTest
    {
        [TestMethod]
        [DataRow(12, DisplayName = "H_returning_TimeSpan_over_hours_given_less_than_24_hours_as_int")]
        [DataRow(10.5d, DisplayName = "H_returning_TimeSpan_over_hours_given_less_than_24_hours_as_double")]
        public void H_returning_TimeSpan_over_hours_given_less_than_24_hours<T>(T hours) where T : INumber<T>
        {
            var span = hours.h();
            span.Days.Should().Be(0);
            span.Seconds.Should().Be(0);
            span.Milliseconds.Should().Be(0);
            span.Microseconds.Should().Be(0);
            if (hours is int)
            {
                span.Hours.Should().Be(12);
                span.Minutes.Should().Be(0);
            }
            else
            {
                span.Hours.Should().Be(10);
                span.Minutes.Should().Be(30);
            }
        }
        [TestMethod]
        [DataRow(30, DisplayName = "H_returning_TimeSpan_over_days_and_hours_given_more_than_24_hours_as_int")]
        [DataRow(40.5d, DisplayName = "H_returning_TimeSpan_over_days_and_hours_given_more_than_24_hours_as_double")]
        public void H_returning_TimeSpan_over_days_and_hours_given_more_than_24_hours<T>(T hours) where T : INumber<T>
        {
            var span = hours.h();
            span.Days.Should().Be(1);
            span.Seconds.Should().Be(0);
            span.Milliseconds.Should().Be(0);
            span.Microseconds.Should().Be(0);
            if (hours is int)
            {
                span.Hours.Should().Be(6);
                span.Minutes.Should().Be(0);
            }
            else
            {
                span.Hours.Should().Be(16);
                span.Minutes.Should().Be(30);
            }
        }

        [TestMethod]
        [DataRow(45, DisplayName = "Min_returning_TimeSpan_over_minutes_given_less_than_60_minutes_as_int")]
        [DataRow(30.25d, DisplayName = "Min_returning_TimeSpan_over_minutes_given_less_than_60_minutes_as_double")]
        public void Min_returning_TimeSpan_over_minutes_given_less_than_60_minutes<T>(T minutes) where T : INumber<T>
        {
            var span = minutes.min();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(0);
            span.Milliseconds.Should().Be(0);
            span.Microseconds.Should().Be(0);
            if (minutes is int)
            {
                span.Minutes.Should().Be(45);
                span.Seconds.Should().Be(0);
            }
            else
            {
                span.Minutes.Should().Be(30);
                span.Seconds.Should().Be(15);
            }
        }
        [TestMethod]
        [DataRow(90, DisplayName = "Min_returning_TimeSpan_over_hours_and_minutes_given_more_than_60_minutes_as_int")]
        [DataRow(70.25d, DisplayName = "Min_returning_TimeSpan_over_hours_and_minutes_given_more_than_60_minutes_as_double")]
        public void Min_returning_TimeSpan_over_hours_and_minutes_given_more_than_60_minutes<T>(T minutes) where T : INumber<T>
        {
            var span = minutes.min();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(1);
            span.Milliseconds.Should().Be(0);
            span.Microseconds.Should().Be(0);
            if (minutes is int)
            {
                span.Minutes.Should().Be(30);
                span.Seconds.Should().Be(0);
            }
            else
            {
                span.Minutes.Should().Be(10);
                span.Seconds.Should().Be(15);
            }
        }

        [TestMethod]
        [DataRow(45, DisplayName = "Sec_returning_TimeSpan_over_seconds_given_less_than_60_seconds_as_int")]
        [DataRow(30.25d, DisplayName = "Sec_returning_TimeSpan_over_seconds_given_less_than_60_seconds_as_double")]
        public void Sec_returning_TimeSpan_over_seconds_given_less_than_60_seconds<T>(T seconds) where T : INumber<T>
        {
            var span = seconds.sec();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(0);
            span.Minutes.Should().Be(0);
            span.Microseconds.Should().Be(0);
            if (seconds is int)
            {
                span.Seconds.Should().Be(45);
                span.Milliseconds.Should().Be(0);
            }
            else
            {
                span.Seconds.Should().Be(30);
                span.Milliseconds.Should().Be(250);
            }
        }
        [TestMethod]
        [DataRow(90, DisplayName = "Sec_returning_TimeSpan_over_minutes_and_seconds_given_more_than_60_seconds_as_int")]
        [DataRow(70.25d, DisplayName = "Sec_returning_TimeSpan_over_minutes_and_seconds_given_more_than_60_seconds_as_double")]
        public void Sec_returning_TimeSpan_over_minutes_and_seconds_given_more_than_60_seconds<T>(T seconds) where T : INumber<T>
        {
            var span = seconds.sec();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(0);
            span.Minutes.Should().Be(1);
            span.Microseconds.Should().Be(0);
            if (seconds is int)
            {
                span.Seconds.Should().Be(30);
                span.Milliseconds.Should().Be(0);
            }
            else
            {
                span.Seconds.Should().Be(10);
                span.Milliseconds.Should().Be(250);
            }
        }

        [TestMethod]
        [DataRow(456, DisplayName = "Ms_returning_TimeSpan_over_milliseconds_given_less_than_1000_milliseconds_as_int")]
        [DataRow(234.25d, DisplayName = "Ms_returning_TimeSpan_over_milliseconds_given_less_than_1000_milliseconds_as_double")]
        public void Ms_returning_TimeSpan_over_milliseconds_given_less_than_1000_milliseconds<T>(T milliseconds) where T : INumber<T>
        {
            var span = milliseconds.ms();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(0);
            span.Minutes.Should().Be(0);
            span.Seconds.Should().Be(0);
            if (milliseconds is int)
            {
                span.Milliseconds.Should().Be(456);
                span.Microseconds.Should().Be(0);
            }
            else
            {
                span.Milliseconds.Should().Be(234);
                span.Microseconds.Should().Be(250);
            }
        }
        [TestMethod]
        [DataRow(1456, DisplayName = "Ms_returning_TimeSpan_over_seconds_and_milliseconds_given_more_than_1000_milliseconds_as_int")]
        [DataRow(1234.25d, DisplayName = "Ms_returning_TimeSpan_over_seconds_and_milliseconds_given_more_than_1000_milliseconds_as_double")]
        public void Ms_returning_TimeSpan_over_seconds_and_milliseconds_given_more_than_1000_milliseconds<T>(T milliseconds) where T : INumber<T>
        {
            var span = milliseconds.ms();
            span.Days.Should().Be(0);
            span.Hours.Should().Be(0);
            span.Minutes.Should().Be(0);
            span.Seconds.Should().Be(1);
            if (milliseconds is int)
            {
                span.Milliseconds.Should().Be(456);
                span.Microseconds.Should().Be(0);
            }
            else
            {
                span.Milliseconds.Should().Be(234);
                span.Microseconds.Should().Be(250);
            }
        }
    }
}
