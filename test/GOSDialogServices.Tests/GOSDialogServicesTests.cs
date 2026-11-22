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

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task BuildSaveOptionsAsync_ShouldSuggestNoFolder_WhenNoFolderIsRequested(string? initialFolder)
    {
        var storage = new Mock<IStorageProvider>();

        FilePickerSaveOptions options = await Dialogs.BuildSaveOptionsAsync(
            storage.Object, Picker(Documents), Extensions("sin"), initialFolder, "Workfile.sin");

        options.SuggestedStartLocation.Should().BeNull();
        storage.Verify(s => s.TryGetFolderFromPathAsync(It.IsAny<Uri>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task BuildFolderOptionsAsync_ShouldSuggestNoFolder_WhenNoFolderIsRequested(string? initialFolder)
    {
        var storage = new Mock<IStorageProvider>();

        FolderPickerOpenOptions options = await Dialogs.BuildFolderOptionsAsync(storage.Object, Picker(Documents), initialFolder);

        options.SuggestedStartLocation.Should().BeNull();
        storage.Verify(s => s.TryGetFolderFromPathAsync(It.IsAny<Uri>()), Times.Never);
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

    [Fact]
    public void ConfirmButtons_ShouldPutEnterOnTheLastButton_WhenTheActionComesFirst()
    {
        // E179, Parte 5: o Enter cai no botao seguro, o ultimo (Close, que devolve null), nunca no verbo.
        var layout = Dialogs.ConfirmButtons(["Delete", "Cancel"]);

        layout.Primary.Should().Be("Delete");
        layout.Close.Should().Be("Cancel");
        layout.Secondary.Should().BeNull();
        layout.Default.Should().Be(FluentAvalonia.UI.Controls.FAContentDialogButton.Close);
    }

    [Fact]
    public void ConfirmButtons_ShouldUseTheMiddleButtonAsSecondary_WhenThereAreThree()
    {
        var layout = Dialogs.ConfirmButtons(["Save", "Don't save", "Cancel"]);

        layout.Primary.Should().Be("Save");
        layout.Secondary.Should().Be("Don't save");
        layout.Close.Should().Be("Cancel");
        layout.Default.Should().Be(FluentAvalonia.UI.Controls.FAContentDialogButton.Close);
    }

    [Fact]
    public void ConfirmButtons_ShouldShowOnlyTheCloseButton_WhenThereIsOneButton()
    {
        // E183, A148: com um botao so o dialogo e um aviso; o mesmo texto nao pode virar Primary e Close ao mesmo tempo.
        var layout = Dialogs.ConfirmButtons(["Ok"]);

        layout.Primary.Should().BeNull();
        layout.Secondary.Should().BeNull();
        layout.Close.Should().Be("Ok");
        layout.Default.Should().Be(FluentAvalonia.UI.Controls.FAContentDialogButton.Close);
    }

    // Os primeiros botoes reais dos chamadores diretos de ConfirmDialog no Nimloth (E183, A9): o titulo diz a acao.
    [Theory]
    [InlineData(new[] { "Replace", "Cancel" }, "Replace?")]
    [InlineData(new[] { "Reset", "Cancel" }, "Reset?")]
    [InlineData(new[] { "Delete", "Cancel" }, "Delete?")]
    [InlineData(new[] { "Delete permanently", "Cancel" }, "Delete permanently?")]
    [InlineData(new[] { "Delete model", "Keep" }, "Delete model?")]
    [InlineData(new[] { "Yes", "No" }, "Confirm")]
    [InlineData(new[] { "Ok", "Cancel" }, "Confirm")]
    [InlineData(new[] { "Save", "Don't save", "Cancel" }, "Save?")]
    [InlineData(new[] { "Ok" }, "Notice")]
    [InlineData(new[] { "Close" }, "Notice")]
    [InlineData(new[] { "Open Windows Security", "Got it" }, "Notice")]
    [InlineData(new[] { "Allow", "Deny" }, "Allow?")]
    public void ConfirmTitle_ShouldNameTheAction_OfTheFirstButton(string[] buttons, string expected)
        => Dialogs.ConfirmTitle(buttons).Should().Be(expected);

    [Fact]
    public void ConfirmTitle_ShouldNotDoubleTheQuestionMark()
        => Dialogs.ConfirmTitle(["Discard changes?", "Cancel"]).Should().Be("Discard changes?");

    [Fact]
    public void MessageDialogClass_ShouldBeTheStyleClassTheAppLimitsInWidth()
        => Dialogs.MessageDialogClass.Should().Be("gos-message");
}
