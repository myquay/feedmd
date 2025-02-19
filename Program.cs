// See https://aka.ms/new-console-template for more information
using CommandLine;
using FeedMD.Infrastructure;
using FeedMD.Commands;


var parserResult = Parser.Default.ParseArguments<InitOptions, BuildOptions>(args);

return parserResult.Value switch
{
    InitOptions opts => await Run.InitOptions(opts),
    BuildOptions opts => await Run.BuildOptions(opts),
    _ => Run.Error()
};