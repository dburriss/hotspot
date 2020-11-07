namespace Hotspot

open System
open Spectre.Cli
open Microsoft.Extensions.DependencyInjection

type TypeResolver(provider : IServiceProvider) =
    interface ITypeResolver with
        member this.Resolve(t : Type) =
            if(t.FullName.Contains("Hotspot")) then printfn "RESOLVE: %A" t
            provider.GetRequiredService(t)

type TypeRegistrar(builder : IServiceCollection) =
    interface ITypeRegistrar with
    
        member this.Build() = TypeResolver(builder.BuildServiceProvider()) :> ITypeResolver
        member this.Register(service : Type, implementation : Type) =
            if(service.FullName.Contains("Hotspot")) then printfn "REGISTER: %A : %A" service implementation
            builder.AddSingleton(service, implementation) |> ignore
        member this.RegisterInstance(service : Type, implementation : obj) = builder.AddSingleton(service, implementation) |> ignore
