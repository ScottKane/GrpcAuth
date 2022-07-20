# GrpcAuth

A simple project demonstrating how to set up Authentication/Authorization with jwt through Identity over gRPC using `protobuf-net.Grpc`. Only supports register/login to simplify the example but can easily be extended to support email confirmation/forgot password etc.

Apply migrations `dotnet ef database update -p Server\Server.csproj -s Server\Server.csproj` and run `Server` project.

Has two different roles for `Administrator` and `Basic`. Click user profile button in the top right and click either of the fill credential buttons and then login.
