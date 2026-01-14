后端项目中，添加Package Reference的方式首选FrameworkReference（更现代），在.csproj中添加<FrameworkReference />。如果要build出.dll用于Unity中，则考虑用NuGet的方式，以支持.NET Standard 2.0。



FrameworkReference只能引用“框架”级别的东西（以.App结尾）——e.g. Microsoft.AspNetCore.App, Microsoft.NETCore.App，不能是Microsoft.AspNetCore.SignalR。通过这种方式引用框架并不会使最终dll变大，它只是告诉编译器：编译时让我访问这个框架下的定义，运行时，这些类型会由宿主应用提供。它并不会保证编译/构建时用的framework版本与宿主机上的版本完全一致（也不现实）。如果大版本一致，小版本不同，运行不会受影响（大版本内二进制兼容）。如果大版本不同，会显式报错。

——换句话说，FrameworkReference的哲学是“信任哲学”，它信任宿主环境会提供正确的框架。如果宿主没有，就明确报错。而PackageReference的哲学是“自给自足”。





