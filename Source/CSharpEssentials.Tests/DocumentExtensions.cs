using System.Threading;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests
{
    internal static class DocumentExtensions
    {
        public static Document WithFilePath(this Document document, string newFilePath)
        {
            var text = document.GetTextAsync(CancellationToken.None).Result;
            var project = document.Project.RemoveDocument(document.Id);

            return project.AddDocument(document.Name, text, folders: null, filePath: newFilePath);
        }
    }
}
