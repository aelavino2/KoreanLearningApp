using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoreanLearningApp.Services.Services
{
    internal static class DevPaths
    {
#if WINDOWS && DEBUG
    private static string? _projectRoot;
    private static bool _searched;
 
    /// <summary>
    /// Walks up from the running exe's folder (bin\Debug\...) to find the nearest
    /// folder containing a .csproj — i.e. the KoreanLearningApp head project's
    /// source folder. Only meaningful on Windows in Debug; elsewhere returns null.
    /// </summary>
    public static string? FindProjectRoot()
    {
        if (_searched)
            return _projectRoot;
 
        _searched = true;
 
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
            {
                _projectRoot = dir.FullName;
                break;
            }
 
            dir = dir.Parent;
        }
 
        return _projectRoot;
    }
#else
        public static string? FindProjectRoot() => null;
#endif
    }
}
