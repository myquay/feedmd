// See https://aka.ms/new-console-template for more information
using CommandLine;
using FeedMD.Infrastructure;
using System.Xml;
using System;

using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Microsoft.SyndicationFeed.Atom;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using FeedMD;
using Fluid;
using System.Reflection;
using CommandLine.Text;
using FeedMD.Commands;


var parserResult = CommandLine.Parser.Default.ParseArguments<InitOptions, BuildOptions>(args);

return parserResult.Value switch
{
    InitOptions opts => await Run.InitOptions(opts),
    BuildOptions opts => await Run.BuildOptions(opts),
    _ => Run.Error()
};