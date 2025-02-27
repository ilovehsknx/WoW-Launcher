﻿// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;

namespace Arctium.WoW.Launcher.Misc;

static class Helpers
{
    public static (int Major, int Minor, int Revision, int Build) GetVersionValueFromClient(string fileName)
    {
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);

        return (fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart,
                fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
    }

    public static void PrintHeader(string serverName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(@"_____________World of Warcraft___________");
        Console.WriteLine(@"                   _   _                 ");
        Console.WriteLine(@"    /\            | | (_)                ");
        Console.WriteLine(@"   /  \   _ __ ___| |_ _ _   _ _ __ ___  ");
        Console.WriteLine(@"  / /\ \ | '__/ __| __| | | | | '_ ` _ \ ");
        Console.WriteLine(@" / ____ \| | | (__| |_| | |_| | | | | | |");
        Console.WriteLine(@"/_/    \_\_|  \___|\__|_|\__,_|_| |_| |_|");
        Console.WriteLine("");

        var sb = new StringBuilder();

        sb.Append("_________________________________________");

        var nameStart = (42 - serverName.Length) / 2;

        sb.Insert(nameStart, serverName);
        sb.Remove(nameStart + serverName.Length, serverName.Length);

        Console.WriteLine(sb);
        Console.WriteLine("{0,30}", "https://arctium.io");

        Console.WriteLine();
        Console.WriteLine($"Operating System: {RuntimeInformation.OSDescription}");
    }

    public static ReadOnlySpan<char> ParsePortal(string config)
    {
        const string portalKey = "SET portal";

        var portalIndex = config.IndexOf(portalKey, StringComparison.Ordinal);

        if (portalIndex == -1)
            throw new ArgumentException("Config file does not contain the portal variable.");

        var startQuoteIndex = config.IndexOf('"', portalIndex);

        if (startQuoteIndex == -1)
            throw new ArgumentException("Invalid format for the portal variable.");

        var endQuoteIndex = config.IndexOf('"', startQuoteIndex + 1);

        if (endQuoteIndex == -1)
            throw new ArgumentException("Invalid format for the portal variable.");

        var portalLength = endQuoteIndex - startQuoteIndex - 1;
        var portalSpan = config.AsSpan(startQuoteIndex + 1, portalLength);
        var colonIndex = portalSpan.IndexOf(':');
        var ipSpan = colonIndex != -1 ? portalSpan[..colonIndex] : portalSpan;
        var portalString = ipSpan.ToString().Trim();

        try
        {
            if (IPAddress.TryParse(portalString, out var ipAddress))
                return ipAddress.ToString().AsSpan();

            var ipv4Address = Dns.GetHostAddresses(portalString).FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            if (ipv4Address == null)
                throw new Exception("No IPv4 address found for the provided hostname.");

            return ipv4Address.ToString().AsSpan();
        }
        catch (SocketException)
        {
            Console.WriteLine("No valid portal found. Dev (Local) mode disabled.");

            return string.Empty.AsSpan();
        }
    }
}
