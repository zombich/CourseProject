using Markdig;

namespace WebApp.Converters
{
    public static class MarkDownConverter
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml()
            .Build();

        public static string ConvertToHtml(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            return Markdown.ToHtml(markdown, Pipeline);
        }
    }
}
