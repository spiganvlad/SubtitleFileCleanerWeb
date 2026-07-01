using System.Text;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.TestSubtitles;

public static class SrtTestSubtitle
{
    public static string SrtStringContent => "1\n00:00:31,540 --> 00:00:35,190\n<i>Hello, world!</i>\n\n2\n01:32:12,120 --> 01:34:22,330\n<u>Goodbye, world!</u>\n";
    public static byte[] SrtByteArrayContent => Encoding.UTF8.GetBytes(SrtStringContent);

    public static class ResultWithTags
    {
        public static string ConvertedStringContent => "<i>Hello, world!</i>\n<u>Goodbye, world!</u>\n";
        public static int ConvertedContentLength => ConvertedStringContent.Length;
    }

    public static class ResultWithoutTags
    {
        public static string ConvertedStringContent => "Hello, world!\nGoodbye, world!\n";
        public static int ConvertedContentLength => ConvertedStringContent.Length;
    }
}
