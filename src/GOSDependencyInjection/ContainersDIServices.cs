using BaseLibrary.DependencyInjection;
using Splat;

namespace GOSAvaloniaServices;

public static class ContainersDIServices
{
    static IContainer containerGOS = BaseLibrary.DependencyInjection.Locator.ConstanteContainer;
    static IMutableDependencyResolver containerSplat = Splat.Locator.CurrentMutable;
    public static void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
    {
        //Verifica se já existe um registro para o tipo TService no container do GOSDI se tiver registra também no container do Splat
        TService? obj = containerGOS.Resolve<TService>();
        if (obj is null)
        {
            obj = new TImplementation();
            containerSplat.RegisterConstant(obj, typeof(TService));
        }
    }
    public static void RegisterSingleton<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService no container do GOSDI se tiver registra também no container do Splat
        TService? obj = containerGOS.Resolve<TService>();
        if (obj is null)
        {
            obj = creator();
            containerSplat.RegisterConstant(obj, typeof(TService));
        }
    }
    public static void Register<TService>(Func<TService> creator)
    {
        // Verifica se já existe um registro para o tipo TService no container do GOSDI se tiver registra também no container do Splat
        TService? obj = containerGOS.Resolve<TService>();
        if (obj is null)
        {
            containerSplat.Register(() => creator(), typeof(TService));
        }
    }
    public static TService? Resolve<TService>()
    {
        return containerGOS.Resolve<TService>() ?? Splat.Locator.Current.GetService<TService>();
    }
}
