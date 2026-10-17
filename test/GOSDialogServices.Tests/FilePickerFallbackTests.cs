using System.Runtime.InteropServices;

using Avalonia.Platform.Storage;

using FluentAssertions;

using GOSAvaloniaServices;

using Moq;

namespace GOSDialogServices.Tests;

/// <summary>
/// <see cref="FilePickerFallback"/>: a pasta em que o seletor de arquivos abre e a queda para o seletor gerenciado.
/// Excecao de producao de 16/10/2026: <see cref="COMException"/> "GetItemAt failed" no seletor nativo do Windows ao
/// abrir o Import da Library, quatro vezes seguidas na mesma maquina.
/// </summary>
public class FilePickerFallbackTests
{
    private const string Documents = @"C:\Users\someone\Documents";

    private static FilePickerFallback Create(params string[] existing)
        => new(path => existing.Contains(path), () => Documents);

    [Fact]
    public void ResolveStartFolder_ShouldUseTheRequestedFolder_WhenItExists()
    {
        Create(@"D:\data", Documents).ResolveStartFolder(@"D:\data").Should().Be(@"D:\data");
    }

    [Theory]
    [InlineData(@"D:\removed")]
    [InlineData(null)]
    [InlineData("  ")]
    public void ResolveStartFolder_ShouldUseTheUserDocuments_WhenTheRequestedFolderDoesNotExist(string? requested)
    {
        Create(Documents).ResolveStartFolder(requested).Should().Be(Documents);
    }

    [Fact]
    public void ResolveStartFolder_ShouldReturnNull_WhenNeitherTheRequestedFolderNorTheDocumentsExist()
    {
        Create().ResolveStartFolder(@"D:\removed").Should().BeNull();
    }

    [Fact]
    public async Task OpenAsync_ShouldReturnTheNativeResult_AndNotOpenTheManagedPicker_WhenTheNativePickerWorks()
    {
        bool managedOpened = false;

        string[]? files = await Create().OpenAsync(
            () => Task.FromResult<string[]?>([@"D:\data\a.zip"]),
            () => { managedOpened = true; return Task.FromResult<string[]?>([]); });

        files.Should().Equal(@"D:\data\a.zip");
        managedOpened.Should().BeFalse();
    }

    [Fact]
    public async Task OpenAsync_ShouldOpenTheManagedPicker_WhenTheNativePickerThrowsComException()
    {
        // HResult e mensagem do relato de producao (E_NOINTERFACE, "GetItemAt failed").
        string[]? files = await Create().OpenAsync(
            () => throw new COMException("GetItemAt failed", unchecked((int)0x80004002)),
            () => Task.FromResult<string[]?>([@"D:\data\a.zip"]));

        files.Should().Equal(@"D:\data\a.zip");
    }

    [Fact]
    public async Task OpenAsync_ShouldLetOtherExceptionsReachTheCaller()
    {
        Func<Task> act = () => Create().OpenAsync(
            () => throw new InvalidOperationException("not a picker failure"),
            () => Task.FromResult<string[]?>([]));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ResolveStartLocationAsync_ShouldAskTheProviderForTheDocuments_WhenTheRequestedFolderDoesNotExist()
    {
        var folder = Mock.Of<IStorageFolder>();
        var storage = new Mock<IStorageProvider>();
        storage.Setup(s => s.TryGetFolderFromPathAsync(It.IsAny<Uri>())).ReturnsAsync(folder);

        IStorageFolder? start = await Create(Documents).ResolveStartLocationAsync(storage.Object, @"D:\removed");

        start.Should().BeSameAs(folder);
        storage.Verify(s => s.TryGetFolderFromPathAsync(It.Is<Uri>(u => u.LocalPath.TrimEnd(Path.DirectorySeparatorChar) == Documents)), Times.Once);
    }

    [Fact]
    public async Task ResolveStartLocationAsync_ShouldReturnNull_WithoutAskingTheProvider_WhenNoFolderExists()
    {
        var storage = new Mock<IStorageProvider>(MockBehavior.Strict);

        IStorageFolder? start = await Create().ResolveStartLocationAsync(storage.Object, @"D:\removed");

        start.Should().BeNull();
    }
}
