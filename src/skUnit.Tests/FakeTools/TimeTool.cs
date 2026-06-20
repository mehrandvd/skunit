using System.ComponentModel;
using System.Globalization;

namespace skUnit.Tests.FakeTools
{
    public class TimeTool   
    {
        [Description("Gets the current time.")]
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}