using Avalonia.Platform.Storage;

using FluentAssertions;

using GOSAvaloniaServices;

using Moq;

using Dialogs = GOSAvaloniaServices.GOSDialogServices;

namespace GOSDialogServices.Tests;

/// <summary>
/// Opcoes que o <see cref="Dialogs"/> monta para os seletores de arquivo e de pasta (E153): o filtro "todos os
/// arquivos" e a pasta inicial quando a pedida nao existe.
/// </summary>
public class GOSDialogServicesTests
{
    private const string Documents = @"C:\Users\someone\Documents";

    private static FilePickerFallback Picker(params string[] existing)
        => new(path => existing.Contains(path), () => Documents);

    private static Tuple<List<string>, string>[] Extensions(params string[] extensions)
        => [Tuple.Create(extensions.ToList(), "Files")];

    [Fact]
    public void ToFileTypes_ShouldKeepTheBareAsterisk_ForAllFiles()
    {
        Dialogs.ToFileTypes(Extensions("*")).Single().Patterns.Should().Equal("*");
    }

    [Theory]
    [InlineData("zip")]
    [InlineData(".zip")]
    [InlineData("*.zip")]
    public void ToFileTypes_ShouldPrefixTheExtension_WithOrWithoutTheCallersDotOrAsterisk(string extension)
    {
        Dialogs.ToFileTypes(Extensions(extension)).Single().Patterns.Should().Equal("*.zip");
    }

    [Fact]
    public void ToFileTypes_ShouldKeepTheNameAndEveryExtension_OfEachFilter()
    {
        List<FilePickerFileType> types = Dialogs.ToFileTypes(
            [Tuple.Create(new List<string> { "xy", "dat" }, "Data"), Tuple.Create(new List<string> { "*" }, "All files")]);

        types.Select(t => t.Name).Should().Equal("Data", "All files");
        types[0].Patterns.Should().Equal("*.xy", "*.dat");
        types[1].Patterns.Should().Equal("*");
    }

    [Fact]
    public async Task BuildSaveOptionsAsync_ShouldStartInTheDocuments_WhenTheRequestedFolderDoesNotExist()
    {
        var folder = Mock.Of<IStorageFolder>();
        var storage = new Mock<IStorageProvider>();
        storage.Setup(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == Documents))).ReturnsAsync(folder);

        FilePickerSaveOptions options = await Dialogs.BuildSaveOptionsAsync(
            storage.Object, Picker(Documents), Extensions("sin"), @"D:\removed", "Workfile.sin");

        options.SuggestedStartLocation.Should().BeSameAs(folder);
        options.SuggestedFileName.Should().Be("Workfile.sin");
        options.FileTypeChoices!.Single().Patterns.Should().Equal("*.sin");
        storage.Verify(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == @"D:\removed")), Times.Never);
    }

    [Fact]
    public async Task BuildFolderOptionsAsync_ShouldStartInTheDocuments_WhenTheRequestedFolderDoesNotExist()
    {
        var folder = Mock.Of<IStorageFolder>();
        var storage = new Mock<IStorageProvider>();
        storage.Setup(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == Documents))).ReturnsAsync(folder);

        FolderPickerOpenOptions options = await Dialogs.BuildFolderOptionsAsync(storage.Object, Picker(Documents), @"D:\removed");

        options.SuggestedStartLocation.Should().BeSameAs(folder);
        options.AllowMultiple.Should().BeFalse();
        storage.Verify(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == @"D:\removed")), Times.Never);
    }

    [Fact]
    public async Task BuildOpenOptionsAsync_ShouldUseTheRequestedFolder_AndTheAllFilesFilter_WhenTheFolderExists()
    {
        var folder = Mock.Of<IStorageFolder>();
        var storage = new Mock<IStorageProvider>();
        storage.Setup(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == @"D:\data"))).ReturnsAsync(folder);

        FilePickerOpenOptions options = await Dialogs.BuildOpenOptionsAsync(
            storage.Object, Picker(@"D:\data", Documents), Extensions("*"), allowMultiple: true, @"D:\data", null);

        options.SuggestedStartLocation.Should().BeSameAs(folder);
        options.AllowMultiple.Should().BeTrue();
        options.Title.Should().Be("Open File(s)");
        options.FileTypeFilter!.Single().Patterns.Should().Equal("*");
    }
}
